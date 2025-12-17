using ApplicationB.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ApplicationB
{
    public partial class ApplicationB : Form
    {
        // --- CONFIGURAÇÕES ---
        private const string API_URL = "http://localhost:60000/api/somiod";
        private const string BROKER_IP = "127.0.0.1";

        private const string APP_NAME = "smart-parking";
        private const string CONTAINER_NAME = "piso-01";
        private const string SUBSCRIPTION_NAME = "SubAppB";

        // Tópico MQTT
        private readonly string TOPIC = $"api/somiod/{APP_NAME}/{CONTAINER_NAME}";

        MqttClient mClient;
        HttpClient httpClient = new HttpClient();

        public ApplicationB()
        {
            InitializeComponent();
        }

        private void ApplicationB_Load(object sender, EventArgs e)
        {
            txtApiUrl.Text = API_URL;

            // Estado inicial dos botões
            btnUnsubscribe.Enabled = false;
            btnSubscribe.Enabled = false;
            txtBrokerIp.Text = BROKER_IP;
        }

        // =========================================================
        // 1. LIGAR AO BROKER (Apenas Conectar, NÃO Subscrever)
        // =========================================================
        private void btnListen_Click(object sender, EventArgs e)
        {
            try
            {
                string ip = string.IsNullOrEmpty(txtBrokerIp.Text) ? BROKER_IP : txtBrokerIp.Text;

                mClient = new MqttClient(ip);
                mClient.MqttMsgPublishReceived += MClient_MqttMsgPublishReceived;

                string clientId = Guid.NewGuid().ToString();
                mClient.Connect(clientId);

                if (mClient.IsConnected)
                {
                    MessageBox.Show($"Ligado ao Broker MQTT ({ip}) com sucesso!", "Conectado", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] [MQTT] Conectado ao Broker {ip}\r\n");
                    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] [MQTT] Aguardando subscrição manual...\r\n");
                    txtLog.AppendText("--------------------------------------------------\r\n");

                    btnSubscribe.Enabled = true; // Agora o utilizador pode subscrever
                    btnListen.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao ligar ao MQTT: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // 2. RECEBER NOTIFICAÇÃO (Callback MQTT)
        // =========================================================
        private void MClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    string jsonMsg = Encoding.UTF8.GetString(e.Message);
                    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] [RX] Notificação recebida via MQTT.\r\n");

                    // Deserializar
                    NotificationModel notif = JsonConvert.DeserializeObject<NotificationModel>(jsonMsg);

                    if (notif == null || string.IsNullOrEmpty(notif.Content))
                    {
                        txtLog.AppendText("[AVISO] JSON inválido ou conteúdo vazio.\r\n");
                        return;
                    }

                    // Gravar XML
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string fileName = $"parking_{timestamp}.xml";

                    try
                    {
                        File.WriteAllText(fileName, notif.Content);
                        txtLog.AppendText($" -> XML guardado: {fileName}\r\n");
                    }
                    catch (Exception exIO)
                    {
                        txtLog.AppendText($"[ERRO IO] Falha ao gravar ficheiro: {exIO.Message}\r\n");
                        return;
                    }

                    // Validar XSD
                    string xsdPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Schemas", "parking.xsd");

                    if (!File.Exists(xsdPath))
                        xsdPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\Schemas\parking.xsd"));

                    if (ValidateXml(fileName, xsdPath))
                    {
                        txtLog.AppendText(" -> [VALIDAÇÃO] O XML respeita o Schema XSD! (Sucesso)\r\n");
                    }
                    else
                    {
                        txtLog.AppendText(" -> [VALIDAÇÃO] O XML é INVÁLIDO segundo o XSD.\r\n");
                    }
                    txtLog.AppendText("--------------------------------------------------\r\n");
                }
                catch (Exception ex)
                {
                    txtLog.AppendText($"[ERRO GERAL] {ex.Message}\r\n");
                }
            });
        }

        private bool ValidateXml(string xmlPath, string xsdPath)
        {
            if (!File.Exists(xsdPath))
            {
                txtLog.AppendText($"[ERRO XSD] Ficheiro de Schema não encontrado: {xsdPath}\r\n");
                return false;
            }

            bool isValid = true;
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Schemas.Add(null, xsdPath);
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationEventHandler += (s, e) =>
                {
                    isValid = false;
                    txtLog.AppendText($"   [XSD Detalhe] {e.Message}\r\n");
                };

                using (XmlReader reader = XmlReader.Create(xmlPath, settings))
                {
                    while (reader.Read()) { }
                }
            }
            catch (Exception ex)
            {
                isValid = false;
                txtLog.AppendText($"   [XSD Crítico] {ex.Message}\r\n");
            }
            return isValid;
        }

        // =========================================================
        // 3. CRIAR SUBSCRICAO NA API + ATIVAR MQTT (Subscribe)
        // =========================================================
        private async void btnSubscribe_Click(object sender, EventArgs e)
        {
            btnSubscribe.Enabled = false;
            Cursor = Cursors.WaitCursor;
            bool activateMqtt = false;

            try
            {
                string endpointUrl = $"{API_URL}/{APP_NAME}/{CONTAINER_NAME}/subs";
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] [API] A criar subscrição em: {endpointUrl}...\r\n");

                SubscriptionModel sub = new SubscriptionModel
                {
                    ResourceName = SUBSCRIPTION_NAME,
                    ResType = "subscription",
                    Evt = 1, // 1 = Creation
                    Endpoint = $"mqtt://{BROKER_IP}:1883",
                    CreationDateTime = DateTime.Now
                };

                string json = JsonConvert.SerializeObject(sub);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(endpointUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] [API] SUCESSO: Subscrição criada na BD.\r\n");
                    MessageBox.Show("Subscrição criada com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    activateMqtt = true;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] [API] AVISO: A subscrição já existia (Conflict 409).\r\n");
                    MessageBox.Show("A subscrição já existe na base de dados. A ativar escuta.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    activateMqtt = true;
                }
                else
                {
                    string err = await response.Content.ReadAsStringAsync();
                    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] [API] ERRO ({response.StatusCode}): {err}\r\n");
                    MessageBox.Show($"Erro API: {response.StatusCode}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnSubscribe.Enabled = true;
                }

                if (activateMqtt && mClient != null && mClient.IsConnected)
                {
                    mClient.Subscribe(new string[] { TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] [MQTT] Tópico subscrito localmente: {TOPIC}\r\n");

                    btnUnsubscribe.Enabled = true; // Só agora podemos cancelar
                }

                txtLog.AppendText("--------------------------------------------------\r\n");
            }
            catch (Exception ex)
            {
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] [ERRO HTTP] {ex.Message}\r\n");
                MessageBox.Show($"Erro de conexão: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnSubscribe.Enabled = true;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        // =========================================================
        // 4. APAGAR SUBSCRICAO + PARAR MQTT (Unsubscribe)
        // =========================================================
        private async void btnUnsubscribe_Click(object sender, EventArgs e)
        {
            try
            {
                // Apagar na API
                string endpointUrl = $"{API_URL}/{APP_NAME}/{CONTAINER_NAME}/subs/{SUBSCRIPTION_NAME}";
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] [API] A apagar subscrição: {endpointUrl}...\r\n");

                var response = await httpClient.DeleteAsync(endpointUrl);

                if (response.IsSuccessStatusCode)
                {
                    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] [API] SUCESSO: Subscrição removida da BD.\r\n");
                    MessageBox.Show("Subscrição removida da API.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] [API] Aviso ao apagar ({response.StatusCode}). Assumindo removido.\r\n");
                }

                // Parar de escutar MQTT (Unsubscribe local)
                if (mClient != null && mClient.IsConnected)
                {
                    mClient.Unsubscribe(new string[] { TOPIC });
                    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] [MQTT] Unsubscribed localmente. Escuta parada.\r\n");
                }

                txtLog.AppendText("--------------------------------------------------\r\n");

                btnSubscribe.Enabled = true;
                btnUnsubscribe.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao apagar: {ex.Message}");
                // Reset seguro em caso de erro
                btnSubscribe.Enabled = true;
                btnUnsubscribe.Enabled = false;
            }
        }
    }
}