using ApplicationA.Models;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ApplicationA
{
    public partial class ApplicationA : Form
    {
        // Cliente HTTP reutilizável (Singleton para evitar exaustão de sockets)
        private static readonly HttpClient client = new HttpClient();

        // Constantes do Cenário "Smart Parking"
        private const string APP_NAME = "smart-parking";
        private const string CONTAINER_NAME = "piso-01";

        public ApplicationA()
        {
            InitializeComponent();
            ConfigurarInterface();
        }

        private void ConfigurarInterface()
        {
            txtApiUrl.Text = "http://localhost:60000/api/somiod/";

            // Estado inicial: Desconectado
            btnEntrada.Enabled = false;
            btnSaida.Enabled = false;
            btnConnect.Enabled = true;
            lblStatus.Text = "Desligado";
            lblStatus.ForeColor = Color.Black;
        }

        // ========================================================
        // 1. BOOTSTRAP: Inicialização (Criar App e Container)
        // ========================================================
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            string baseUrl = txtApiUrl.Text.Trim();
            if (!baseUrl.EndsWith("/")) baseUrl += "/";

            btnConnect.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                // Garantir Aplicação
                await GarantirRecurso(baseUrl, APP_NAME, "application");

                // Garantir Contentor (URL base + AppName)
                string appUrl = baseUrl + APP_NAME;
                await GarantirRecurso(appUrl, CONTAINER_NAME, "container");

                // Sucesso
                MessageBox.Show($"Sensor ligado ao contentor '{CONTAINER_NAME}' com sucesso!", "Ligado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AtivarControlos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro de conexão:\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnConnect.Enabled = true;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void AtivarControlos()
        {
            btnEntrada.Enabled = true;
            btnSaida.Enabled = true;
            lblStatus.Text = "Pronto.";
            lblStatus.ForeColor = Color.Gray;
        }

        // Função Genérica para verificar e criar recursos
        private async Task GarantirRecurso(string parentUrl, string resourceName, string type)
        {
            string targetUrl = $"{parentUrl}/{resourceName}";

            // Tenta obter o recurso (GET)
            var response = await client.GetAsync(targetUrl);

            // Se não existir (404), cria (POST)
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                object payload;

                // Cria o modelo com PascalCase (ResourceName), o [JsonProperty] trata do JSON
                if (type == "application")
                {
                    payload = new ApplicationModel { ResourceName = resourceName, ResType = "application" };
                }
                else
                {
                    payload = new ContainerModel { ResourceName = resourceName, ResType = "container" };
                }

                string json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var createResponse = await client.PostAsync(parentUrl, content);

                if (!createResponse.IsSuccessStatusCode)
                {
                    string error = await createResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Falha ao criar {type} '{resourceName}'.\nStatus: {createResponse.StatusCode}\n{error}");
                }
            }
            else if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Erro ao verificar {type} '{resourceName}'. Status: {response.StatusCode}");
            }
        }

        // ========================================================
        // 2. SIMULAÇÃO: Envio de Dados (Sensor)
        // ========================================================
        private async void btnEntrar_Click(object sender, EventArgs e)
        {
            await EnviarEstado("occupied");
            lblStatus.Text = "Lugar A1: OCUPADO";
            lblStatus.ForeColor = Color.Red;
        }

        private async void btnSair_Click(object sender, EventArgs e)
        {
            await EnviarEstado("free");
            lblStatus.Text = "Lugar A1: LIVRE";
            lblStatus.ForeColor = Color.Green;
        }

        private async Task EnviarEstado(string status)
        {
            string baseUrl = txtApiUrl.Text.Trim();
            if (!baseUrl.EndsWith("/")) baseUrl += "/";

            // URL para criar dados: .../api/somiod/{appName}/{containerName}
            string url = $"{baseUrl}{APP_NAME}/{CONTAINER_NAME}";

            // Conteúdo XML (Validado pelo XSD na App B)
            string xmlContent = $"<parking><spot>A1</spot><status>{status}</status><time>{DateTime.Now:HH:mm:ss}</time></parking>";

            var data = new ContentInstanceModel
            {
                ResType = "content-instance",           // Importante: identifica o tipo
                ResourceName = "sensor_" + DateTime.Now.Ticks,
                Content = xmlContent,
                ContentType = "application/xml"
            };

            try
            {
                string json = JsonConvert.SerializeObject(data);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Erro API ({response.StatusCode}): {error}", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Falha de comunicação: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Handlers do Designer
        private void txtApiUrl_TextChanged(object sender, EventArgs e) { }
        private void ApplicationA_Load(object sender, EventArgs e) { }
    }
}