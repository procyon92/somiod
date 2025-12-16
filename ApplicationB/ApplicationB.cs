using Newtonsoft.Json;
using SOMIOD.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace ApplicationB
{
    public partial class ApplicaionB : Form
    {
        // --- CONFIGURAÇÕES ---
        // CONFIRMA SE O PORTO É 60000 (Muda se a tua API estiver noutro porto, ex: 50000, 44301)
        string apiUrl = "http://localhost:60000/api/somiod";
        string brokerIp = "127.0.0.1";

        string appName = "smart-parking";
        string containerName = "piso-01";
        string topic = "api/somiod/smart-parking/piso-01";
        string mySubscriptionName = "SubAppB";

        MqttClient mClient;
        HttpClient httpClient = new HttpClient();

        public ApplicaionB()
        {
            InitializeComponent();
        }

        private void ApplicaionB_Load(object sender, EventArgs e)
        {
            // Botões começam desativados até ligar ao broker
            btnUnsubscribe.Enabled = false;
            btnSubscribe.Enabled = false;
        }

        // =========================================================
        // 1. RECEBER NOTIFICAÇÃO (Lógica Principal)
        // =========================================================
        private void MClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                try
                {
                    string jsonMsg = Encoding.UTF8.GetString(e.Message);
                    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] [RX] JSON recebido.\r\n"); // Encurtei para não encher o log

                    // A. Deserializar o JSON para Objeto C#
                    NotificationModel notif = JsonConvert.DeserializeObject<NotificationModel>(jsonMsg);

                    if (notif == null || string.IsNullOrEmpty(notif.content))
                    {
                        txtLog.AppendText("[ERRO] JSON vazio ou sem conteúdo.\r\n");
                        return;
                    }

                    // =================================================================
                    // B. MUDANÇA CRÍTICA: GRAVAR APENAS O CONTEÚDO XML (<parking>...)
                    // =================================================================
                    // Formata a data para Ano-Mes-Dia_Hora-Minuto-Segundo
                    string dataFormatada = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                    string fileName = $"parking_{dataFormatada}.xml";

                    try
                    {
                        // O 'notif.content' já contém a string "<parking><spot>...</parking>"
                        // Vamos carregar isso como um documento XML limpo
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(notif.content);

                        // Guardar no disco (O ficheiro vai começar com <parking>)
                        doc.Save(fileName);
                    }
                    catch (Exception exXml)
                    {
                        txtLog.AppendText($"[ERRO] O conteúdo não é XML válido: {exXml.Message}\r\n");
                        return; // Se não é XML, não vale a pena validar
                    }

                    // C. Validar com XSD
                    // O caminho relativo para a pasta Schemas (conforme a tua imagem)
                    string caminhoXsd = @"..\..\..\Schemas\parking.xsd";

                    if (ValidateXml(fileName, caminhoXsd))
                    {
                        txtLog.AppendText($" -> XML guardado e VALIDADO: {fileName}\r\n");
                    }
                    else
                    {
                        txtLog.AppendText($" -> XML guardado mas INVÁLIDO (Erro XSD): {fileName}\r\n");
                    }
                    txtLog.AppendText("-------------------------------\r\n");
                }
                catch (Exception ex)
                {
                    txtLog.AppendText($"[ERRO] Processamento: {ex.Message}\r\n");
                }
            });
        }

        // Função auxiliar de validação XML
        private bool ValidateXml(string xmlPath, string xsdPath)
        {
            if (!File.Exists(xsdPath)) return false;
            bool isValid = true;
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Schemas.Add(null, xsdPath);
                settings.ValidationType = ValidationType.Schema;
                settings.ValidationEventHandler += (s, e) => { isValid = false; };
                using (XmlReader reader = XmlReader.Create(xmlPath, settings))
                {
                    while (reader.Read()) { }
                }
            }
            catch { isValid = false; }
            return isValid;
        }

        private void MClient_MqttMsgPublishSubscribed(object sender, MqttMsgSubscribedEventArgs e)
        {
            // Confirmação silenciosa ou MessageBox se preferires
        }

        // =========================================================
        // 2. LIGAR AO BROKER (Listen)
        // =========================================================
        private void btnListen_Click_1(object sender, EventArgs e)
        {
            try
            {
                // Se o user escreveu um IP na caixa de texto, usa esse
                if (!string.IsNullOrEmpty(txtBrokerIp.Text))
                    brokerIp = txtBrokerIp.Text;

                mClient = new MqttClient(brokerIp);
                mClient.MqttMsgPublishReceived += MClient_MqttMsgPublishReceived;
                // mClient.MqttMsgSubscribed += MClient_MqttMsgPublishSubscribed; // Opcional

                string clientId = Guid.NewGuid().ToString();
                mClient.Connect(clientId);

                if (mClient.IsConnected)
                {
                    // Subscrever o tópico
                    mClient.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

                    MessageBox.Show($"Ligado ao Broker MQTT!\nA escutar: {topic}");

                    // Ativar botões da API
                    btnSubscribe.Enabled = true;
                    btnListen.Enabled = false; // Bloqueia para não ligar 2 vezes
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao ligar ao MQTT: {ex.Message}");
            }
        }

        // =========================================================
        // 3. CRIAR SUBSCRICAO NA API (Subscribe)
        // =========================================================
        private async void btnSubscribe_Click_1(object sender, EventArgs e)
        {
            btnSubscribe.Enabled = false; // Evita duplo clique
            try
            {
                string endpointUrl = $"{apiUrl}/{appName}/{containerName}/subs";

                SubscriptionModel sub = new SubscriptionModel
                {
                    resource_name = mySubscriptionName,
                    evt = "1", // 1 = Creation
                    endpoint = $"mqtt://{brokerIp}:1883",
                    parent_id = 0
                };

                string json = JsonConvert.SerializeObject(sub);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(endpointUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Sucesso: Subscrição criada na API!");
                    btnUnsubscribe.Enabled = true;
                }
                else
                {
                    string erro = await response.Content.ReadAsStringAsync();

                    // AJUDA DE DIAGNÓSTICO
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        MessageBox.Show($"Erro 404 (Not Found):\n\nO contentor '{containerName}' ou a app '{appName}' NÃO existem na base de dados.\n\nCorre a Aplicação A (ou usa o Postman) para criar a App e o Contentor antes de subscreveres.");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        MessageBox.Show("Aviso: Esta subscrição já existe na base de dados.");
                        btnUnsubscribe.Enabled = true; // Ativa o botão de apagar se já existe
                    }
                    else
                    {
                        MessageBox.Show($"Erro API: {response.StatusCode} - {erro}");
                    }

                    btnSubscribe.Enabled = true; // Deixa tentar de novo
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro de conexão HTTP: {ex.Message}\nVerifica se a API está a correr no porto correto.");
                btnSubscribe.Enabled = true;
            }
        }

        // =========================================================
        // 4. APAGAR SUBSCRICAO (Unsubscribe)
        // =========================================================
        private async void btnUnsubscribe_Click_1(object sender, EventArgs e)
        {
            try
            {
                // PASSO 1: APAGAR O REGISTO NA API (Obrigatório pelo Projeto)
                string endpointUrl = $"{apiUrl}/{appName}/{containerName}/subs/{mySubscriptionName}";

                var response = await httpClient.DeleteAsync(endpointUrl);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Subscrição removida da Base de Dados com sucesso!");
                }
                else
                {
                    // Se a API der erro (ex: 404 Not Found), não faz mal.
                    // O importante é garantir que a tua App para de ouvir.
                    string erro = await response.Content.ReadAsStringAsync();
                    // Opcional: MessageBox.Show($"Aviso API: {response.StatusCode}");
                }

                // PASSO 2: PARAR DE ESCUTAR O MQTT (Obrigatório para parar a UI)
                if (mClient != null && mClient.IsConnected)
                {
                    // Diz ao Broker: "Não me mandes mais nada deste tópico"
                    mClient.Unsubscribe(new string[] { topic });

                    txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] [STOP] Subscrição cancelada. Não receberás mais notificações.\r\n");
                    txtLog.AppendText("-------------------------------\r\n");
                }

                // PASSO 3: GERIR BOTÕES
                // Como já não estamos subscritos, ativamos o botão para poder voltar a subscrever
                btnSubscribe.Enabled = true;
                btnUnsubscribe.Enabled = false;

                // Opcional: Reativar o botão de Listen se quiseres permitir reconexão limpa
                // btnListen.Enabled = true; 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao fazer Unsubscribe: " + ex.Message);

                // Em caso de erro crítico, assumimos que a ligação caiu e deixamos tentar de novo
                btnSubscribe.Enabled = true;
                btnUnsubscribe.Enabled = false;
            }
        }
    }
}