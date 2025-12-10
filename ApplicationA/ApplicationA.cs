using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using ApplicationA.Models; // Certifique-se que o namespace bate certo

namespace ApplicationA
{
    public partial class ApplicationA : Form
    {
        // Cliente HTTP reutilizável
        private static readonly HttpClient client = new HttpClient();

        // Configurações do Cenário "Smart Parking"
        private string appName = "smart-parking";
        private string containerName = "piso-01";

        public ApplicationA()
        {
            InitializeComponent();

            // Configura o URL inicial
            txtApiUrl.Text = "http://localhost:51364/api/somiod/";

            // CORREÇÃO: Forçar os botões a começar DESATIVADOS
            btnEntrada.Enabled = false;
            btnSaida.Enabled = false;

            // O botão de ligar deve começar ativo
            btnConnect.Enabled = true;
        }

        // ========================================================
        // 1. BOOTSTRAP: Cria a App e o Container se não existirem
        // ========================================================
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            string baseUrl = txtApiUrl.Text.Trim();
            btnConnect.Enabled = false;

            try
            {
                // A. Verificar/Criar Aplicação
                string appUrl = baseUrl + appName;
                var responseApp = await client.GetAsync(appUrl);

                if (!responseApp.IsSuccessStatusCode)
                {
                    var newApp = new ApplicationModel { resource_name = appName };
                    string json = JsonConvert.SerializeObject(newApp);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var res = await client.PostAsync(baseUrl, content);
                    res.EnsureSuccessStatusCode();
                }

                // B. Verificar/Criar Contentor
                string containerUrl = appUrl + "/" + containerName;
                var responseCont = await client.GetAsync(containerUrl);

                if (!responseCont.IsSuccessStatusCode)
                {
                    var newCont = new ContainerModel { resource_name = containerName };
                    string json = JsonConvert.SerializeObject(newCont);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var res = await client.PostAsync(appUrl, content);
                    res.EnsureSuccessStatusCode();
                }

                MessageBox.Show($"Sensor ligado ao contentor '{containerName}' com sucesso!");
                btnEntrada.Enabled = true;
                btnSaida.Enabled = true;
                lblStatus.Text = "Pronto a enviar dados.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao ligar: " + ex.Message);
            }
            finally
            {
                btnConnect.Enabled = true;
            }
        }

        // ========================================================
        // 2. SIMULAR ENTRADA (OCUPADO)
        // ========================================================
        private async void btnEntrar_Click(object sender, EventArgs e)
        {
            await EnviarEstado("occupied");
            lblStatus.Text = "Lugar A1: OCUPADO";
            lblStatus.ForeColor = Color.Red;
        }

        // ========================================================
        // 3. SIMULAR SAÍDA (LIVRE)
        // ========================================================
        private async void btnSair_Click(object sender, EventArgs e)
        {
            await EnviarEstado("free");
            lblStatus.Text = "Lugar A1: LIVRE";
            lblStatus.ForeColor = Color.Green;
        }

        // Função auxiliar para enviar o POST
        private async System.Threading.Tasks.Task EnviarEstado(string status)
        {
            string baseUrl = txtApiUrl.Text.Trim();
            string url = baseUrl + appName + "/" + containerName;

            // Conteúdo XML obrigatório pelo requisito do projeto
            string xmlContent = $"<parking><spot>A1</spot><status>{status}</status><time>{DateTime.Now:HH:mm:ss}</time></parking>";

            var data = new ContentInstanceModel
            {
                resource_name = "sensor_" + DateTime.Now.Ticks,
                content = xmlContent,
                content_type = "application/xml"
            };

            try
            {
                string json = JsonConvert.SerializeObject(data);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, httpContent);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao enviar dados: " + ex.Message);
            }
        }

        // Função vazia apenas para corrigir o erro do Designer
        private void txtApiUrl_TextChanged(object sender, EventArgs e)
        {
        }

        private void ApplicationA_Load(object sender, EventArgs e)
        {

        }
    }
}