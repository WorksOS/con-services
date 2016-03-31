using System.Web;
using System.Web.Http;
using System.Web.Routing;
using java.util;
using org.apache.kafka.clients.producer;
using VSS.Kafka.Ikvm.Client;

namespace VSP.MasterData.Project.WebAPI
{
  public class WebApiApplication : System.Web.HttpApplication
  {
    protected void Application_Start()
    {
      IkvmKafkaInitializer.Initialize();
      GlobalConfiguration.Configure(WebApiConfig.Register);
      var container = new AutofacContainer();
      container.ApplyDependencyInjection();
      RouteConfig.RegisterRoutes(RouteTable.Routes);
    }
  }
}
