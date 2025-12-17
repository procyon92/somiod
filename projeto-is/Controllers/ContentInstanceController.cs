using Newtonsoft.Json;
using SOMIOD.Models;
using SOMIOD.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace SOMIOD.Controllers
{
    [RoutePrefix("api/somiod")]
    public class ContentInstanceController : ApiController
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ConnStr"].ConnectionString;

        // NOTA: O método POST foi movido para o ContainerController.

        /// <summary>
        /// Obtém os detalhes de uma Instância de Conteúdo (registo de dados).
        /// </summary>
        /// <remarks>
        /// Recupera a informação guardada (campo content) de uma instância específica.
        /// 
        /// **Comportamento Discovery:**
        /// Como a Content-Instance é um recurso de nível inferior (folha), se o header `somiod-discovery` for enviado, a API retorna uma lista vazia `[]`, indicando que não existem sub-recursos hierárquicos.
        /// </remarks>
        /// <param name="appName">O nome da aplicação pai.</param>
        /// <param name="containerName">O nome do contentor pai.</param>
        /// <param name="recordName">O nome da instância de conteúdo a recuperar.</param>
        /// <returns>O objeto ContentInstance com os dados.</returns>
        /// <response code="200">Sucesso: Retorna o objeto (ou lista vazia em discovery).</response>
        /// <response code="404">Erro: Recurso não encontrado (App, Contentor ou Instância inexistentes).</response>
        [ResponseType(typeof(ContentInstanceModel))]
        [HttpGet]
        // A REGEX impede que este método capture a rota ".../subs", que pertence ao SubscriptionController
        [Route("{appName}/{containerName}/{recordName:regex(^(?!subs$).*)}")]
        public IHttpActionResult GetContentInstance(string appName, string containerName, string recordName)
        {
            // 1. Validar existência (Hierarquia completa)
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sqlCheck = @"SELECT CI.Id FROM ContentInstances CI
                                    INNER JOIN Containers C ON CI.ParentContainerId = C.Id
                                    INNER JOIN Applications A ON C.ParentAppId = A.Id
                                    WHERE A.Name = @appName AND C.Name = @contName AND CI.Name = @dataName";
                SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn);
                cmdCheck.Parameters.AddWithValue("@appName", appName);
                cmdCheck.Parameters.AddWithValue("@contName", containerName);
                cmdCheck.Parameters.AddWithValue("@dataName", recordName);

                if (cmdCheck.ExecuteScalar() == null) return NotFound();
            }

            // 2. Discovery (Item folha -> Retorna vazio)
            if (Request.Headers.Contains("somiod-discovery"))
            {
                return Ok(new List<string>());
            }

            // 3. Leitura Normal
            ContentInstanceModel ci = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = @"SELECT CI.Id, CI.Name, CI.CreationDate, CI.ContentType, CI.Content, CI.ParentContainerId
                               FROM ContentInstances CI
                               INNER JOIN Containers C ON CI.ParentContainerId = C.Id
                               INNER JOIN Applications A ON C.ParentAppId = A.Id
                               WHERE A.Name = @appName AND C.Name = @contName AND CI.Name = @dataName";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@appName", appName);
                cmd.Parameters.AddWithValue("@contName", containerName);
                cmd.Parameters.AddWithValue("@dataName", recordName);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    ci = new ContentInstanceModel
                    {
                        Id = (int)reader["Id"],
                        ResourceName = (string)reader["Name"],
                        CreationDateTime = (DateTime)reader["CreationDate"],
                        ContentType = (string)reader["ContentType"],
                        Content = (string)reader["Content"],
                        ParentId = (int)reader["ParentContainerId"],
                        ResType = "content-instance"
                    };
                }
            }
            return Ok(ci);
        }

        /// <summary>
        /// Remove uma instância de conteúdo (dados).
        /// </summary>
        /// <remarks>
        /// Apaga o registo de dados da base de dados.
        /// 
        /// **Notificações:**
        /// Esta operação despoleta o processo de notificação. Os subscritores configurados para escutar o evento de **Eliminação (Type 2)** receberão uma mensagem a informar que este recurso foi apagado.
        /// </remarks>
        /// <param name="appName">O nome da aplicação pai.</param>
        /// <param name="containerName">O nome do contentor pai.</param>
        /// <param name="recordName">O nome do registo a apagar.</param>
        /// <returns>Sem conteúdo.</returns>
        /// <response code="200">Sucesso: O registo foi apagado e as notificações enviadas (assincronamente).</response>
        /// <response code="404">Erro: O recurso não foi encontrado.</response>
        [ResponseType(typeof(void))]
        [HttpDelete]
        // A REGEX também é aplicada aqui para evitar conflitos no Delete
        [Route("{appName}/{containerName}/{recordName:regex(^(?!subs$).*)}")]
        public IHttpActionResult DeleteContentInstance(string appName, string containerName, string recordName)
        {
            int containerId = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // 1. Obter ID do Pai e Validar Existência
                string sqlGetInfo = @"SELECT C.Id FROM ContentInstances CI
                                      INNER JOIN Containers C ON CI.ParentContainerId = C.Id 
                                      INNER JOIN Applications A ON C.ParentAppId = A.Id 
                                      WHERE A.Name = @appName AND C.Name = @contName AND CI.Name = @dataName";

                SqlCommand cmd = new SqlCommand(sqlGetInfo, conn);
                cmd.Parameters.AddWithValue("@appName", appName);
                cmd.Parameters.AddWithValue("@contName", containerName);
                cmd.Parameters.AddWithValue("@dataName", recordName);

                object result = cmd.ExecuteScalar();
                if (result == null) return NotFound();
                containerId = (int)result;

                // 2. Apagar Registo
                string sqlDelete = @"DELETE CI FROM ContentInstances CI
                                     INNER JOIN Containers C ON CI.ParentContainerId = C.Id
                                     INNER JOIN Applications A ON C.ParentAppId = A.Id
                                     WHERE A.Name = @appName AND C.Name = @contName AND CI.Name = @dataName";

                SqlCommand cmdDel = new SqlCommand(sqlDelete, conn);
                cmdDel.Parameters.AddWithValue("@appName", appName);
                cmdDel.Parameters.AddWithValue("@contName", containerName);
                cmdDel.Parameters.AddWithValue("@dataName", recordName);

                cmdDel.ExecuteNonQuery();
            }

            // 3. SISTEMA DE NOTIFICAÇÕES (Evento: Deletion = 2)
            var dummyModel = new ContentInstanceModel { ResourceName = recordName };
            _ = Task.Run(async () => await DispatchNotifications(appName, containerName, containerId, dummyModel, 2));

            return Ok();
        }

        // =============================================================
        // MÉTODOS AUXILIARES (NOTIFICAÇÕES)
        // =============================================================
        private async Task DispatchNotifications(string appName, string containerName, int containerId, ContentInstanceModel data, int eventType)
        {
            var notification = new NotificationModel
            {
                EventType = (eventType == 1) ? "creation" : "deletion",
                ResourceName = data.ResourceName,
                ContainerName = containerName,
                AppName = appName,
                Timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                Content = (eventType == 1) ? data.Content : null,
                ContentType = (eventType == 1) ? data.ContentType : null
            };

            string jsonPayload = JsonConvert.SerializeObject(notification);
            string topic = $"api/somiod/{appName}/{containerName}";

            // Envio MQTT (Best Effort)
            try
            {
                MqttHelper.Publish(topic, jsonPayload);
                System.Diagnostics.Debug.WriteLine($"[Middleware] MQTT Publicado em {topic}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Middleware] Erro MQTT: {ex.Message}");
            }

            // Envio HTTP (Apenas para subscritores registados na BD)
            List<string> endpoints = GetSubscribersEndpoints(containerId, eventType);

            foreach (var endpoint in endpoints)
            {
                if (endpoint.ToLower().StartsWith("http"))
                {
                    try
                    {
                        await HttpHelper.SendNotificationAsync(notification, endpoint);
                        System.Diagnostics.Debug.WriteLine($"[Middleware] HTTP Post enviado para {endpoint}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Middleware] Erro HTTP: {ex.Message}");
                    }
                }
            }
        }

        private List<string> GetSubscribersEndpoints(int containerId, int eventType)
        {
            List<string> eps = new List<string>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "SELECT Endpoint FROM Subscriptions WHERE ParentContainerId = @parentId AND Event = @evt";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@parentId", containerId);
                cmd.Parameters.AddWithValue("@evt", eventType);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    eps.Add(reader["Endpoint"].ToString());
                }
            }
            return eps;
        }
    }
}