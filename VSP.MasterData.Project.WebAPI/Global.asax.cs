using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace VSP.MasterData.Project.WebAPI
{
  public class WebApiApplication : System.Web.HttpApplication
  {
    protected void Application_Start()
    {
      GlobalConfiguration.Configure(WebApiConfig.Register);
      var container = new AutofacContainer();
      container.ApplyDependencyInjection();
      RouteConfig.RegisterRoutes(RouteTable.Routes);
    }
  }
}
