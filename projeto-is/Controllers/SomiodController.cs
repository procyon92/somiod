using SOMIOD.Models; // Garante que este using bate certo com o namespace do teu Modelo
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Http;

namespace SOMIOD.Controllers
{
    // Define o prefixo base para todos os métodos deste controlador
    [RoutePrefix("api/somiod")]
    public class SomiodController : ApiController
    {
        // Vai buscar a string de conexão definida na Web.config
        string connectionString = ConfigurationManager.ConnectionStrings["SomiodConnect"].ConnectionString;

        // ==========================================
        // 1. CRIAR APLICAÇÃO (POST api/somiod/)
        // ==========================================
        [HttpPost]
        [Route("")] // O URL é apenas api/somiod/
        public IHttpActionResult PostApplication([FromBody] ApplicationModel app)
        {
            // Validações básicas
            if (app == null) return BadRequest("Dados inválidos.");
            if (app.res_type != "application") return BadRequest("O tipo de recurso deve ser 'application'.");

            // Se o utilizador não der nome, temos de gerar um
            if (string.IsNullOrEmpty(app.resource_name))
            {
                app.resource_name = "App_" + Guid.NewGuid().ToString().Substring(0, 8);
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "INSERT INTO Applications (Name, CreationDate) VALUES (@name, @date)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@name", app.resource_name);
                cmd.Parameters.AddWithValue("@date", DateTime.Now);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    // Erro 2627 é violação de chave única (nome duplicado)
                    if (ex.Number == 2627)
                    {
                        return Conflict(); // Retorna erro 409 Conflict
                    }
                    return InternalServerError(ex);
                }
            }

            // Retorna os dados completos da aplicação criada
            return Ok(app);
        }

        // ==========================================
        // 2. LER APLICAÇÃO (GET api/somiod/{name})
        // ==========================================
        [HttpGet]
        [Route("{name}")] // O URL captura o nome, ex: api/somiod/lighting
        public IHttpActionResult GetApplication(string name)
        {
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

            if (app == null)
            {
                return NotFound(); // Retorna 404 se não encontrar
            }

            return Ok(app);
        }
    }
}