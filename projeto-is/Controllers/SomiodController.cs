using SOMIOD;
using SOMIOD.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace SOMIOD.Controllers
{
    [RoutePrefix("api/somiod")]
    public class SomiodController : ApiController
    {
        // String de conexão definida no Web.config
        string connectionString = ConfigurationManager.ConnectionStrings["SomiodConnect"].ConnectionString;

        // =============================================================
        // REGION 1: DISCOVERY (ROOT)
        // =============================================================
        #region 1. DISCOVERY (ROOT)

        // DISCOVERY APLICAÇÃO (GET api/somiod/)
        // Responde ao Header 'somiod-discovery: application'
        [HttpGet]
        [Route("")]
        public IHttpActionResult DiscoverApplications()
        {
            var headers = Request.Headers;
            if (headers.Contains("somiod-discovery"))
            {
                string discoveryType = headers.GetValues("somiod-discovery").First();
                if (discoveryType == "application")
                {
                    List<string> apps = new List<string>();
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string sql = "SELECT Name FROM Applications";
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            apps.Add("/api/somiod/" + reader["Name"].ToString());
                        }
                    }
                    return Ok(apps);
                }
            }
            return BadRequest("Operação não suportada. Use o cabeçalho 'somiod-discovery: application'.");
        }

        #endregion

        // =============================================================
        // REGION 2: APPLICATION RESOURCES
        // =============================================================
        #region 2. APPLICATION RESOURCES

        // CRIAR APLICAÇÃO (POST api/somiod/)
        [HttpPost]
        [Route("")]
        public IHttpActionResult PostApplication([FromBody] ApplicationModel app)
        {
            if (app == null) return BadRequest("Dados inválidos.");
            if (app.res_type != "application") return BadRequest("O tipo de recurso deve ser 'application'.");

            if (string.IsNullOrEmpty(app.resource_name))
                app.resource_name = "App_" + Guid.NewGuid().ToString().Substring(0, 8);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                while (true)
                {
                    string sql = "INSERT INTO Applications (Name, CreationDate) VALUES (@name, @date)";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@name", app.resource_name);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        break;
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2627)
                        {
                            app.resource_name = "App_" + app.resource_name + "_" + Guid.NewGuid().ToString().Substring(0, 8);
                            continue;
                        }

                        return InternalServerError(ex);
                    }
                }
            }
            return Ok(app);
        }

        // LER APLICAÇÃO (GET api/somiod/{name})
        // Nota: Este método também gere o 'somiod-discovery: container'
        [HttpGet]
        [Route("{name}")]
        public IHttpActionResult GetApplication(string name)
        {
            // ------------------------------------------
            // A. LÓGICA DE DISCOVERY (Contentores)
            // ------------------------------------------
            var headers = Request.Headers;
            if (headers.Contains("somiod-discovery"))
            {
                string discoveryType = headers.GetValues("somiod-discovery").First();
                if (discoveryType == "container")
                {
                    List<string> containers = new List<string>();
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string sql = @"SELECT C.Name FROM Containers C 
                                       INNER JOIN Applications A ON C.ParentAppId = A.Id 
                                       WHERE A.Name = @appName";
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@appName", name);
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            containers.Add("/api/somiod/" + name + "/" + reader["Name"].ToString());
                        }
                    }
                    return Ok(containers);
                }
            }

            // ------------------------------------------
            // B. LÓGICA NORMAL (Ler App)
            // ------------------------------------------
            ApplicationModel app = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "SELECT * FROM Applications WHERE Name = @name";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", name);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    app = new ApplicationModel
                    {
                        id = (int)reader["Id"],
                        resource_name = (string)reader["Name"],
                        creation_datetime = (DateTime)reader["CreationDate"],
                        res_type = "application"
                    };
                }
            }
            if (app == null) return NotFound();
            return Ok(app);
        }

        // ATUALIZAR APLICAÇÃO (PUT api/somiod/{name})
        [HttpPut]
        [Route("{name}")]
        public IHttpActionResult PutApplication(string name, [FromBody] ApplicationModel app)
        {
            if (app == null) return BadRequest("Dados inválidos.");
            if (app.res_type != "application") return BadRequest("Tipo incorreto.");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sqlCheck = "SELECT Count(*) FROM Applications WHERE Name = @name";
                SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn);
                cmdCheck.Parameters.AddWithValue("@name", name);
                conn.Open();
                if ((int)cmdCheck.ExecuteScalar() == 0) return NotFound();

                string sqlUpdate = "UPDATE Applications SET Name = @newName WHERE Name = @oldName";
                SqlCommand cmdUpdate = new SqlCommand(sqlUpdate, conn);
                cmdUpdate.Parameters.AddWithValue("@newName", app.resource_name);
                cmdUpdate.Parameters.AddWithValue("@oldName", name);

                try
                {
                    cmdUpdate.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) return Conflict();
                    throw;
                }
            }
            return Ok(app);
        }

        // APAGAR APLICAÇÃO (DELETE api/somiod/{name})
        [HttpDelete]
        [Route("{name}")]
        public IHttpActionResult DeleteApplication(string name)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "DELETE FROM Applications WHERE Name = @name";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", name);
                conn.Open();
                if (cmd.ExecuteNonQuery() == 0) return NotFound();
            }
            return Ok();
        }

        #endregion

        // =============================================================
        // REGION 3: CONTAINER RESOURCES
        // =============================================================
        #region 3. CONTAINER RESOURCES

        // CRIAR CONTAINER (POST api/somiod/{appName})
        [HttpPost]
        [Route("{appName}")]
        public IHttpActionResult PostContainer(string appName, [FromBody] ContainerModel container)
        {
            if (container == null) return BadRequest("Dados inválidos.");
            if (container.res_type != "container") return BadRequest("O tipo de recurso deve ser 'container'.");

            if (string.IsNullOrEmpty(container.resource_name))
                container.resource_name = "Cont_" + Guid.NewGuid().ToString().Substring(0, 8);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sqlGetAppId = "SELECT Id FROM Applications WHERE Name = @appName";
                SqlCommand cmdGetId = new SqlCommand(sqlGetAppId, conn);
                cmdGetId.Parameters.AddWithValue("@appName", appName);
                object result = cmdGetId.ExecuteScalar();
                if (result == null) return NotFound();
                int parentId = (int)result;

                string sqlInsert = "INSERT INTO Containers (Name, CreationDate, ParentAppId) VALUES (@name, @date, @parentId)";
                SqlCommand cmdInsert = new SqlCommand(sqlInsert, conn);
                cmdInsert.Parameters.AddWithValue("@name", container.resource_name);
                cmdInsert.Parameters.AddWithValue("@date", DateTime.Now);
                cmdInsert.Parameters.AddWithValue("@parentId", parentId);

                try
                {
                    cmdInsert.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) return Conflict();
                    return InternalServerError(ex);
                }
            }
            return Ok(container);
        }

        // LER CONTAINER (GET api/somiod/{appName}/{containerName})
        [HttpGet]
        [Route("{appName}/{containerName}")]
        public IHttpActionResult GetContainer(string appName, string containerName)
        {
            // ------------------------------------------
            // A. LÓGICA DE DISCOVERY (Content-Instances e Subscriptions)
            // ------------------------------------------
            var headers = Request.Headers;
            if (headers.Contains("somiod-discovery"))
            {
                string discoveryType = headers.GetValues("somiod-discovery").First();

                // Opção 1: Descobrir Content-Instances
                if (discoveryType == "content-instance")
                {
                    List<string> dataRecords = new List<string>();
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string sql = @"SELECT CI.Name 
                                       FROM ContentInstances CI 
                                       INNER JOIN Containers C ON CI.ParentContainerId = C.Id 
                                       INNER JOIN Applications A ON C.ParentAppId = A.Id 
                                       WHERE A.Name = @appName AND C.Name = @contName";

                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@appName", appName);
                        cmd.Parameters.AddWithValue("@contName", containerName);

                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            // URL: /api/somiod/{appName}/{containerName}/{recordName}
                            dataRecords.Add("/api/somiod/" + appName + "/" + containerName + "/" + reader["Name"].ToString());
                        }
                    }
                    return Ok(dataRecords);
                }

                // Opção 2: Descobrir Subscriptions
                if (discoveryType == "subscription")
                {
                    List<string> subs = new List<string>();
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string sql = @"SELECT S.Name 
                                       FROM Subscriptions S 
                                       INNER JOIN Containers C ON S.ParentContainerId = C.Id 
                                       INNER JOIN Applications A ON C.ParentAppId = A.Id 
                                       WHERE A.Name = @appName AND C.Name = @contName";

                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@appName", appName);
                        cmd.Parameters.AddWithValue("@contName", containerName);

                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            // URL: /api/somiod/{appName}/{containerName}/subs/{subName}
                            subs.Add("/api/somiod/" + appName + "/" + containerName + "/subs/" + reader["Name"].ToString());
                        }
                    }
                    return Ok(subs);
                }
            }

            // ------------------------------------------
            // B. LÓGICA NORMAL (Ler Container)
            // ------------------------------------------
            ContainerModel container = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = @"SELECT C.Id, C.Name, C.CreationDate, C.ParentAppId 
                               FROM Containers C
                               INNER JOIN Applications A ON C.ParentAppId = A.Id
                               WHERE A.Name = @appName AND C.Name = @contName";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@appName", appName);
                cmd.Parameters.AddWithValue("@contName", containerName);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    container = new ContainerModel
                    {
                        id = (int)reader["Id"],
                        resource_name = (string)reader["Name"],
                        creation_datetime = (DateTime)reader["CreationDate"],
                        parent_id = (int)reader["ParentAppId"],
                        res_type = "container"
                    };
                }
            }
            if (container == null) return NotFound();
            return Ok(container);
        }

        // ATUALIZAR CONTAINER (PUT api/somiod/{appName}/{containerName})
        [HttpPut]
        [Route("{appName}/{containerName}")]
        public IHttpActionResult PutContainer(string appName, string containerName, [FromBody] ContainerModel container)
        {
            if (container == null) return BadRequest("Dados inválidos.");
            if (container.res_type != "container") return BadRequest("Tipo incorreto.");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sqlGetAppId = "SELECT Id FROM Applications WHERE Name = @appName";
                SqlCommand cmdGetId = new SqlCommand(sqlGetAppId, conn);
                cmdGetId.Parameters.AddWithValue("@appName", appName);
                object result = cmdGetId.ExecuteScalar();
                if (result == null) return NotFound();
                int parentId = (int)result;

                string sqlCheck = "SELECT Count(*) FROM Containers WHERE Name = @contName AND ParentAppId = @parentId";
                SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn);
                cmdCheck.Parameters.AddWithValue("@contName", containerName);
                cmdCheck.Parameters.AddWithValue("@parentId", parentId);
                if ((int)cmdCheck.ExecuteScalar() == 0) return NotFound();

                string sqlUpdate = "UPDATE Containers SET Name = @newName WHERE Name = @oldName AND ParentAppId = @parentId";
                SqlCommand cmdUpdate = new SqlCommand(sqlUpdate, conn);
                cmdUpdate.Parameters.AddWithValue("@newName", container.resource_name);
                cmdUpdate.Parameters.AddWithValue("@oldName", containerName);
                cmdUpdate.Parameters.AddWithValue("@parentId", parentId);

                try
                {
                    cmdUpdate.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) return Conflict();
                    throw;
                }
            }
            return Ok(container);
        }

        // APAGAR CONTAINER (DELETE api/somiod/{appName}/{containerName})
        [HttpDelete]
        [Route("{appName}/{containerName}")]
        public IHttpActionResult DeleteContainer(string appName, string containerName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = @"DELETE C FROM Containers C
                               INNER JOIN Applications A ON C.ParentAppId = A.Id
                               WHERE A.Name = @appName AND C.Name = @contName";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@appName", appName);
                cmd.Parameters.AddWithValue("@contName", containerName);
                conn.Open();
                if (cmd.ExecuteNonQuery() == 0) return NotFound();
            }
            return Ok();
        }

        #endregion

        // =============================================================
        // REGION 4: CONTENT-INSTANCE RESOURCES
        // =============================================================
        #region 4. CONTENT-INSTANCE RESOURCES

        // CRIAR CONTENT-INSTANCE (POST api/somiod/{appName}/{containerName})
        [HttpPost]
        [Route("{appName}/{containerName}")]
        public IHttpActionResult PostContentInstance(string appName, string containerName, [FromBody] ContentInstanceModel ci)
        {
            if (ci == null) return BadRequest("Dados inválidos.");
            if (ci.res_type != "content-instance") return BadRequest("O tipo de recurso deve ser 'content-instance'.");

            if (string.IsNullOrEmpty(ci.resource_name))
                ci.resource_name = "Data_" + Guid.NewGuid().ToString().Substring(0, 8);

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

                int containerId = (int)result;
                string sqlInsert = @"INSERT INTO ContentInstances 
                                     (Name, CreationDate, ContentType, Content, ParentContainerId) 
                                     VALUES (@name, @date, @ctype, @content, @parentId)";

                SqlCommand cmdInsert = new SqlCommand(sqlInsert, conn);
                cmdInsert.Parameters.AddWithValue("@name", ci.resource_name);
                cmdInsert.Parameters.AddWithValue("@date", DateTime.Now);
                cmdInsert.Parameters.AddWithValue("@ctype", ci.content_type ?? "application/json");
                cmdInsert.Parameters.AddWithValue("@content", ci.content ?? "");
                cmdInsert.Parameters.AddWithValue("@parentId", containerId);

                try
                {
                    cmdInsert.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) return Conflict();
                    return InternalServerError(ex);
                }

                var subscriptions = GetSubscriptions(conn, containerId, "1");

                // Starts sending notifications in the background without blocking the API response
                _ = Task.Run(async () =>
                {
                    foreach (var sub in subscriptions)
                    {
                        var notification = new NotificationModel
                        {
                            evt = "creation",
                            resource_name = ci.resource_name,
                            container = containerName,
                            timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                            content = ci.content,
                            content_type = ci.content_type
                        };

                        try
                        {
                            await HttpHelper.SendNotificationAsync(notification, sub.endpoint);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error sending notification: {ex.Message}");
                        }
                    }
                });

                MqttHelper.Publish($"api/somiod/{containerName}", $"ContentInstance created: {ci.resource_name}");
            }
            return Ok(ci);
        }

        // LER CONTENT-INSTANCE (GET api/somiod/{appName}/{containerName}/{recordName})
        [HttpGet]
        [Route("{appName}/{containerName}/{recordName}")]
        public IHttpActionResult GetContentInstance(string appName, string containerName, string recordName)
        {
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
                        id = (int)reader["Id"],
                        resource_name = (string)reader["Name"],
                        creation_datetime = (DateTime)reader["CreationDate"],
                        content_type = (string)reader["ContentType"],
                        content = (string)reader["Content"],
                        parent_id = (int)reader["ParentContainerId"],
                        res_type = "content-instance"
                    };
                }
            }
            if (ci == null) return NotFound();
            return Ok(ci);
        }

        // APAGAR CONTENT-INSTANCE (DELETE api/somiod/{appName}/{containerName}/{recordName})
        [HttpDelete]
        [Route("{appName}/{containerName}/{recordName}")]
        public IHttpActionResult DeleteContentInstance(string appName, string containerName, string recordName)
        {
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

                int containerId = (int)result;

                string sql = @"DELETE CI FROM ContentInstances CI
                               INNER JOIN Containers C ON CI.ParentContainerId = C.Id
                               INNER JOIN Applications A ON C.ParentAppId = A.Id
                               WHERE A.Name = @appName AND C.Name = @contName AND CI.Name = @dataName";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@appName", appName);
                cmd.Parameters.AddWithValue("@contName", containerName);
                cmd.Parameters.AddWithValue("@dataName", recordName);
                if (cmd.ExecuteNonQuery() == 0) return NotFound();

                var subscriptions = GetSubscriptions(conn, containerId, "2");

                // Starts sending notifications in the background
                _ = Task.Run(async () =>
                {
                    foreach (var sub in subscriptions)
                    {
                        var notification = new NotificationModel
                        {
                            evt = "deletion",
                            resource_name = recordName,
                            container = containerName,
                            timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                            content = null,
                            content_type = ""
                        };

                        try
                        {
                            await HttpHelper.SendNotificationAsync(notification, sub.endpoint);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error sending delete notification: {ex.Message}");
                        }
                    }
                });

                MqttHelper.Publish($"api/somiod/{containerName}", $"ContentInstance deleted: {recordName}");
            }
            return Ok();
        }

        #endregion

        // =============================================================
        // REGION 5: SUBSCRIPTION RESOURCES
        // =============================================================
        #region 5. SUBSCRIPTION RESOURCES

        // CRIAR SUBSCRICAO (POST api/somiod/{appName}/{containerName}/subs)
        [HttpPost]
        [Route("{appName}/{containerName}/subs")]
        public IHttpActionResult PostSubscription(string appName, string containerName, [FromBody] SubscriptionModel sub)
        {
            if (sub == null) return BadRequest("Dados inválidos.");
            if (sub.res_type != "subscription") return BadRequest("O tipo de recurso deve ser 'subscription'.");

            if (sub.evt != "1" && sub.evt != "2") return BadRequest("O evento (evt) deve ser '1' (Criação) ou '2' (Apagar).");
            if (string.IsNullOrEmpty(sub.endpoint)) return BadRequest("O endpoint é obrigatório.");

            if (string.IsNullOrEmpty(sub.resource_name))
                sub.resource_name = "Sub_" + Guid.NewGuid().ToString().Substring(0, 8);

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
                int containerId = (int)result;

                string sqlInsert = @"INSERT INTO Subscriptions 
                                     (Name, CreationDate, Event, Endpoint, ParentContainerId) 
                                     VALUES (@name, @date, @evt, @endpoint, @parentId)";
                SqlCommand cmdInsert = new SqlCommand(sqlInsert, conn);
                cmdInsert.Parameters.AddWithValue("@name", sub.resource_name);
                cmdInsert.Parameters.AddWithValue("@date", DateTime.Now);
                cmdInsert.Parameters.AddWithValue("@evt", sub.evt);
                cmdInsert.Parameters.AddWithValue("@endpoint", sub.endpoint);
                cmdInsert.Parameters.AddWithValue("@parentId", containerId);

                try
                {
                    cmdInsert.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) return Conflict();
                    return InternalServerError(ex);
                }
            }
            return Ok(sub);
        }

        // LER SUBSCRICAO (GET api/somiod/{appName}/{containerName}/subs/{subName})
        [HttpGet]
        [Route("{appName}/{containerName}/subs/{subName}")]
        public IHttpActionResult GetSubscription(string appName, string containerName, string subName)
        {
            SubscriptionModel sub = null;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = @"SELECT S.Id, S.Name, S.CreationDate, S.Event, S.Endpoint, S.ParentContainerId
                               FROM Subscriptions S
                               INNER JOIN Containers C ON S.ParentContainerId = C.Id
                               INNER JOIN Applications A ON C.ParentAppId = A.Id
                               WHERE A.Name = @appName AND C.Name = @contName AND S.Name = @subName";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@appName", appName);
                cmd.Parameters.AddWithValue("@contName", containerName);
                cmd.Parameters.AddWithValue("@subName", subName);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    sub = new SubscriptionModel
                    {
                        id = (int)reader["Id"],
                        resource_name = (string)reader["Name"],
                        creation_datetime = (DateTime)reader["CreationDate"],
                        evt = (string)reader["Event"],
                        endpoint = (string)reader["Endpoint"],
                        parent_id = (int)reader["ParentContainerId"],
                        res_type = "subscription"
                    };
                }
            }
            if (sub == null) return NotFound();
            return Ok(sub);
        }

        // APAGAR SUBSCRICAO (DELETE api/somiod/{appName}/{containerName}/subs/{subName})
        [HttpDelete]
        [Route("{appName}/{containerName}/subs/{subName}")]
        public IHttpActionResult DeleteSubscription(string appName, string containerName, string subName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = @"DELETE S FROM Subscriptions S
                               INNER JOIN Containers C ON S.ParentContainerId = C.Id
                               INNER JOIN Applications A ON C.ParentAppId = A.Id
                               WHERE A.Name = @appName AND C.Name = @contName AND S.Name = @subName";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@appName", appName);
                cmd.Parameters.AddWithValue("@contName", containerName);
                cmd.Parameters.AddWithValue("@subName", subName);
                conn.Open();
                if (cmd.ExecuteNonQuery() == 0) return NotFound();
            }
            return Ok();
        }

        private List<SubscriptionModel> GetSubscriptions(SqlConnection conn, int containerId, string evt)
        {
            var list = new List<SubscriptionModel>();

            string sqlSubs = @"SELECT Id, Name, Event, Endpoint 
                       FROM Subscriptions 
                       WHERE ParentContainerId = @id AND Event = @evt";

            SqlCommand cmd = new SqlCommand(sqlSubs, conn);
            cmd.Parameters.AddWithValue("@id", containerId);
            cmd.Parameters.AddWithValue("@evt", evt);

            SqlDataReader r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new SubscriptionModel
                {
                    id = (int)r["Id"],
                    resource_name = (string)r["Name"],
                    evt = (string)r["Event"],
                    endpoint = (string)r["Endpoint"],
                    res_type = "subscription"
                });
            }
            r.Close();
            return list;
        }
        #endregion
    }
}