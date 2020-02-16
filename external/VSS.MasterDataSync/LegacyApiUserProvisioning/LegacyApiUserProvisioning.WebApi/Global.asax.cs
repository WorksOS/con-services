using System;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace LegacyApiUserProvisioning.WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            IocContainer.RegisterItems();
            log4net.Config.XmlConfigurator.Configure();
        }
    }
}
