using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SOMIOD.Models;
using SOMIOD.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace SOMIOD.Controllers
{
    [RoutePrefix("api/somiod")]
    public class ContainerController : ApiController
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ConnStr"].ConnectionString;

        /// <summary>
        /// Obtém os dados de um contentor ou descobre recursos filhos.
        /// </summary>
        /// <remarks>
        /// **Comportamento Padrão:**
        /// Retorna os metadados do contentor.
        /// 
        /// **Comportamento Discovery (Header `somiod-discovery`):**
        /// * `content-instance`: Retorna uma lista de URIs das instâncias de conteúdo dentro deste contentor.
        /// * `subscription`: Retorna uma lista de URIs das subscrições dentro deste contentor (usando o caminho virtual `/subs`).
        /// </remarks>
        /// <param name="appName">O nome da aplicação pai.</param>
        /// <param name="name">O nome do contentor.</param>
        /// <returns>Um objeto ContainerModel ou uma Lista de Strings.</returns>
        /// <response code="200">Sucesso.</response>
        /// <response code="404">Erro: Contentor ou Aplicação não encontrados.</response>
        [ResponseType(typeof(ContainerModel))]
        [HttpGet]
        [Route("{appName}/{name}")]
        public IHttpActionResult GetContainer(string appName, string name)
        {
            // 1. Validar Existência da Hierarquia (App -> Container)
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sqlCheck = @"SELECT C.Id FROM Containers C 
                                    INNER JOIN Applications A ON C.ParentAppId = A.Id 
                                    WHERE A.Name = @appName AND C.Name = @contName";
                SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn);
                cmdCheck.Parameters.AddWithValue("@appName", appName);
                cmdCheck.Parameters.AddWithValue("@contName", name);

                if (cmdCheck.ExecuteScalar() == null) return NotFound();
            }

            // 2. Lógica de Discovery (Headers)
            var headers = Request.Headers;
            if (headers.Contains("somiod-discovery"))
            {
                string discoveryType = headers.GetValues("somiod-discovery").First().ToLower();
                List<string> results = new List<string>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Descobrir Content-Instances
                    if (discoveryType == "content-instance")
                    {
                        string sql = @"SELECT CI.Name FROM ContentInstances CI 
                                       INNER JOIN Containers C ON CI.ParentContainerId = C.Id 
                                       INNER JOIN Applications A ON C.ParentAppId = A.Id 
                                       WHERE A.Name = @appName AND C.Name = @contName";

                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@appName", appName);
                        cmd.Parameters.AddWithValue("@contName", name);
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            results.Add($"/api/somiod/{appName}/{name}/{reader["Name"]}");
                        }
                    }
                    // Descobrir Subscriptions
                    else if (discoveryType == "subscription")
                    {
                        string sql = @"SELECT S.Name FROM Subscriptions S 
                                       INNER JOIN Containers C ON S.ParentContainerId = C.Id 
                                       INNER JOIN Applications A ON C.ParentAppId = A.Id 
                                       WHERE A.Name = @appName AND C.Name = @contName";

                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@appName", appName);
                        cmd.Parameters.AddWithValue("@contName", name);
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            // Nota: Subscrições têm o segmento virtual '/subs/'
                            results.Add($"/api/somiod/{appName}/{name}/subs/{reader["Name"]}");
                        }
                    }
                    // Outros tipos (application, container) retornam lista vazia neste nível
                    else
                    {
                        return Ok(results);
                    }
                }
                return Ok(results);
            }

            // 3. Leitura Normal
            ContainerModel container = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = @"SELECT C.Id, C.Name, C.CreationDate, C.ParentAppId 
                               FROM Containers C
                               INNER JOIN Applications A ON C.ParentAppId = A.Id
                               WHERE A.Name = @appName AND C.Name = @contName";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@appName", appName);
                cmd.Parameters.AddWithValue("@contName", name);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    container = new ContainerModel
                    {
                        Id = (int)reader["Id"],
                        ResourceName = (string)reader["Name"],
                        CreationDateTime = (DateTime)reader["CreationDate"],
                        ParentId = (int)reader["ParentAppId"],
                        ResType = "container"
                    };
                }
            }
            return Ok(container);
        }

        /// <summary>
        /// Atualiza um contentor existente.
        /// </summary>
        /// <remarks>
        /// Permite alterar o nome de um contentor dentro de uma aplicação específica.
        /// O novo nome (`resource-name`) não pode estar em uso por outro contentor na mesma aplicação.
        /// 
        /// **Exemplo de Pedido:**
        /// 
        ///     PUT /api/somiod/smart-parking/piso-01
        ///     {
        ///        "res-type": "container",
        ///        "resource-name": "piso-01-v2"
        ///     }
        /// </remarks>
        /// <param name="appName">O nome da aplicação pai.</param>
        /// <param name="name">O nome atual do contentor a atualizar.</param>
        /// <param name="container">O objeto contendo os novos dados (novo nome).</param>
        /// <returns>Os dados do contentor atualizado.</returns>
        /// <response code="200">Sucesso: Contentor atualizado.</response>
        /// <response code="400">Erro: Dados inválidos ou tipo de recurso incorreto.</response>
        /// <response code="404">Erro: Aplicação ou Contentor não encontrados.</response>
        /// <response code="409">Conflito: O novo nome já existe nesta aplicação.</response>
        [ResponseType(typeof(ContainerModel))]
        [HttpPut]
        [Route("{appName}/{name}")]
        public IHttpActionResult PutContainer(string appName, string name, [FromBody] ContainerModel container)
        {
            if (container == null) return BadRequest("Dados inválidos.");
            if (container.ResType.ToLower() != "container") return BadRequest("Tipo incorreto.");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Obter ID da App Pai
                string sqlGetAppId = "SELECT Id FROM Applications WHERE Name = @appName";
                SqlCommand cmdGetId = new SqlCommand(sqlGetAppId, conn);
                cmdGetId.Parameters.AddWithValue("@appName", appName);
                object result = cmdGetId.ExecuteScalar();
                if (result == null) return NotFound();
                int parentId = (int)result;

                // Verificar existência do container
                string sqlCheck = "SELECT Count(*) FROM Containers WHERE Name = @contName AND ParentAppId = @parentId";
                SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn);
                cmdCheck.Parameters.AddWithValue("@contName", name);
                cmdCheck.Parameters.AddWithValue("@parentId", parentId);

                if ((int)cmdCheck.ExecuteScalar() == 0) return NotFound();

                // Update
                string sqlUpdate = "UPDATE Containers SET Name = @newName WHERE Name = @oldName AND ParentAppId = @parentId";
                SqlCommand cmdUpdate = new SqlCommand(sqlUpdate, conn);
                cmdUpdate.Parameters.AddWithValue("@newName", container.ResourceName);
                cmdUpdate.Parameters.AddWithValue("@oldName", name);
                cmdUpdate.Parameters.AddWithValue("@parentId", parentId);

                try
                {
                    cmdUpdate.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) return Content(HttpStatusCode.Conflict, "Container name already exists.");
                    return InternalServerError(ex);
                }
            }
            return Ok(container);
        }

        /// <summary>
        /// Remove um contentor existente.
        /// </summary>
        /// <remarks>
        /// Apaga o contentor da base de dados.
        /// **Atenção:** Se a base de dados estiver configurada com Cascade Delete, isto apagará também todas as Instâncias de Conteúdo e Subscrições dentro deste contentor.
        /// </remarks>
        /// <param name="appName">O nome da aplicação pai.</param>
        /// <param name="name">O nome do contentor a remover.</param>
        /// <returns>Sem conteúdo.</returns>
        /// <response code="200">Sucesso: Contentor apagado.</response>
        /// <response code="404">Erro: Aplicação ou Contentor não encontrados.</response>
        [ResponseType(typeof(void))]
        [HttpDelete]
        [Route("{appName}/{name}")]
        public IHttpActionResult DeleteContainer(string appName, string name)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Join para garantir hierarquia correta
                string sql = @"DELETE C FROM Containers C
                               INNER JOIN Applications A ON C.ParentAppId = A.Id
                               WHERE A.Name = @appName AND C.Name = @contName";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@appName", appName);
                cmd.Parameters.AddWithValue("@contName", name);
                conn.Open();

                if (cmd.ExecuteNonQuery() == 0) return NotFound();
            }
            return Ok();
        }

        // =============================================================
        // CRIAR RECURSO FILHO (Content-Instance OU Subscription)
        // URL: api/somiod/{appName}/{name}
        // Este método substitui os POSTs dos outros controllers para evitar conflito de rota
        // =============================================================

        /// <summary>
        /// Cria um recurso filho (Content-Instance ou Subscription) num contentor.
        /// </summary>
        /// <remarks>
        /// Este endpoint é dinâmico. O tipo de recurso criado depende do valor do campo **res-type** enviado no JSON.
        /// 
        /// **Opção A: Criar Content Instance (Dados)**
        /// Use `res-type: content-instance`.
        /// 
        ///     POST /api/somiod/smart-parking/piso-01
        ///     {
        ///        "res-type": "content-instance",
        ///        "resource-name": "medicao-01",
        ///        "content": "Lugar Ocupado"
        ///     }
        /// 
        /// **Opção B: Criar Subscription (Notificações)**
        /// Use `res-type: subscription`. O endpoint deve ser MQTT ou HTTP.
        /// 
        ///     POST /api/somiod/smart-parking/piso-01
        ///     {
        ///        "res-type": "subscription",
        ///        "resource-name": "sub-app-monitor",
        ///        "endpoint": "mqtt://127.0.0.1:1883",
        ///        "evt": 1
        ///     }
        /// </remarks>
        /// <param name="appName">O nome da aplicação pai.</param>
        /// <param name="name">O nome do contentor pai.</param>
        /// <param name="jsonBody">O corpo JSON contendo os dados do recurso (ContentInstance ou Subscription).</param>
        /// <returns>O objeto do recurso criado.</returns>
        /// <response code="201">Sucesso: Recurso criado.</response>
        /// <response code="400">Erro: O 'res-type' é inválido, está em falta, ou o JSON está malformado.</response>
        /// <response code="404">Erro: A aplicação ou contentor pai não existem.</response>
        /// <response code="409">Conflito: Já existe um recurso com esse nome.</response>
        [ResponseType(typeof(object))]
        [HttpPost]
        [Route("{appName}/{name}")]
        public IHttpActionResult PostChildResource(string appName, string name, [FromBody] JObject jsonBody)
        {
            if (jsonBody == null) return BadRequest("Body vazio.");

            // Identificar o tipo de recurso a criar
            string resType = jsonBody["res-type"]?.ToString().ToLower();

            if (resType == "content-instance")
            {
                var ci = jsonBody.ToObject<ContentInstanceModel>();
                return CreateContentInstanceInternal(appName, name, ci);
            }
            else if (resType == "subscription")
            {
                var sub = jsonBody.ToObject<SubscriptionModel>();
                return CreateSubscriptionInternal(appName, name, sub);
            }
            else
            {
                return BadRequest($"Tipo de recurso '{resType}' inválido para este endpoint. Esperado: 'content-instance' ou 'subscription'.");
            }
        }

        // --- Lógica Interna: Criar Content Instance ---
        private IHttpActionResult CreateContentInstanceInternal(string appName, string containerName, ContentInstanceModel ci)
        {
            int containerId = 0;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Obter ID do Container
                string sqlGetContId = @"SELECT C.Id FROM Containers C 
                                        INNER JOIN Applications A ON C.ParentAppId = A.Id 
                                        WHERE A.Name = @appName AND C.Name = @contName";
                SqlCommand cmdGetId = new SqlCommand(sqlGetContId, conn);
                cmdGetId.Parameters.AddWithValue("@appName", appName);
                cmdGetId.Parameters.AddWithValue("@contName", containerName);
                object result = cmdGetId.ExecuteScalar();
                if (result == null) return NotFound();
                containerId = (int)result;
            }

            // Naming
            bool isAutoGenerated = string.IsNullOrEmpty(ci.ResourceName);
            if (isAutoGenerated) ci.ResourceName = "Data_" + Guid.NewGuid().ToString().Substring(0, 8);
            ci.CreationDateTime = DateTime.Now;

            // Insert
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                while (true)
                {
                    string sqlInsert = @"INSERT INTO ContentInstances (Name, CreationDate, ContentType, Content, ParentContainerId) 
                                         VALUES (@name, @date, @ctype, @content, @parentId)";
                    SqlCommand cmdInsert = new SqlCommand(sqlInsert, conn);
                    cmdInsert.Parameters.AddWithValue("@name", ci.ResourceName);
                    cmdInsert.Parameters.AddWithValue("@date", ci.CreationDateTime);
                    cmdInsert.Parameters.AddWithValue("@ctype", ci.ContentType ?? "application/json");
                    cmdInsert.Parameters.AddWithValue("@content", ci.Content ?? "");
                    cmdInsert.Parameters.AddWithValue("@parentId", containerId);

                    try
                    {
                        cmdInsert.ExecuteNonQuery();
                        break;
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2627)
                        {
                            if (isAutoGenerated) { ci.ResourceName = "Data_" + Guid.NewGuid().ToString().Substring(0, 8); continue; }
                            return Content(HttpStatusCode.Conflict, "Resource name already exists.");
                        }
                        return InternalServerError(ex);
                    }
                }
            }

            // NOTIFICAÇÕES (Evento 1: Creation)
            _ = Task.Run(async () => await DispatchNotifications(appName, containerName, containerId, ci, 1));

            return Content(HttpStatusCode.Created, ci);
        }

        // --- Lógica Interna: Criar Subscription ---
        private IHttpActionResult CreateSubscriptionInternal(string appName, string containerName, SubscriptionModel sub)
        {
            if (string.IsNullOrEmpty(sub.Endpoint)) return BadRequest("O endpoint é obrigatório.");
            if (sub.Evt != 1 && sub.Evt != 2) return BadRequest("Evento deve ser 1 (Creation) ou 2 (Deletion).");

            int containerId = 0;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sqlGetContId = @"SELECT C.Id FROM Containers C 
                                        INNER JOIN Applications A ON C.ParentAppId = A.Id 
                                        WHERE A.Name = @appName AND C.Name = @contName";
                SqlCommand cmdGetId = new SqlCommand(sqlGetContId, conn);
                cmdGetId.Parameters.AddWithValue("@appName", appName);
                cmdGetId.Parameters.AddWithValue("@contName", containerName);
                object result = cmdGetId.ExecuteScalar();
                if (result == null) return NotFound();
                containerId = (int)result;
            }

            bool isAutoGenerated = string.IsNullOrEmpty(sub.ResourceName);
            if (isAutoGenerated) sub.ResourceName = "Sub_" + Guid.NewGuid().ToString().Substring(0, 8);
            sub.CreationDateTime = DateTime.Now;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                while (true)
                {
                    string sqlInsert = @"INSERT INTO Subscriptions (Name, CreationDate, Event, Endpoint, ParentContainerId) 
                                         VALUES (@name, @date, @evt, @endpoint, @parentId)";
                    SqlCommand cmdInsert = new SqlCommand(sqlInsert, conn);
                    cmdInsert.Parameters.AddWithValue("@name", sub.ResourceName);
                    cmdInsert.Parameters.AddWithValue("@date", sub.CreationDateTime);
                    cmdInsert.Parameters.AddWithValue("@evt", sub.Evt);
                    cmdInsert.Parameters.AddWithValue("@endpoint", sub.Endpoint);
                    cmdInsert.Parameters.AddWithValue("@parentId", containerId);

                    try
                    {
                        cmdInsert.ExecuteNonQuery();
                        break;
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2627)
                        {
                            if (isAutoGenerated) { sub.ResourceName = "Sub_" + Guid.NewGuid().ToString().Substring(0, 8); continue; }
                            return Content(HttpStatusCode.Conflict, "Subscription name already exists.");
                        }
                        return InternalServerError(ex);
                    }
                }
            }
            return Content(HttpStatusCode.Created, sub);
        }

        // --- Helpers de Notificação ---
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

            // MQTT
            try { MqttHelper.Publish(topic, jsonPayload); } catch { }

            // HTTP
            List<string> endpoints = GetSubscribersEndpoints(containerId, eventType);
            foreach (var endpoint in endpoints)
            {
                if (endpoint.ToLower().StartsWith("http"))
                {
                    try { await HttpHelper.SendNotificationAsync(notification, endpoint); } catch { }
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
                while (reader.Read()) { eps.Add(reader["Endpoint"].ToString()); }
            }
            return eps;
        }
    }
}