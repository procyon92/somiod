using SOMIOD.Utils;
using System.Web.Http;

namespace projeto_is
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            MqttHelper.Connect();
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }

        protected void Application_End()
        {
            MqttHelper.Disconnect();
        }
    }
}
