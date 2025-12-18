using System.Web.Http;

namespace projeto_is
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Habilitar Attribute Routing
            config.MapHttpAttributeRoutes();

            // Configuração do Formatador JSON 
            var jsonSettings = config.Formatters.JsonFormatter.SerializerSettings;

            // Formato de data ISO simplificado (sem milissegundos)
            jsonSettings.DateFormatString = "yyyy-MM-dd'T'HH:mm:ss";

            // Dados transferidos sempre em JSON
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            // Formata o JSON para ser mais legível quando testas no browser
            jsonSettings.Formatting = Newtonsoft.Json.Formatting.Indented;

            // Rota Padrão (Fallback)
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}