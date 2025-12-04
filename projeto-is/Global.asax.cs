using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

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
