using System.Web.Http;
using VSP.MasterData.Project.WebAPI.Filters;
using VSS.VisionLink.Utilization.WebApi.Configuration;

namespace VSP.MasterData.Project.WebAPI
{
  public static class WebApiConfig
  {
    public static void Register(HttpConfiguration config)
    {
      config.Filters.Add(new TIDAuthFilter());
      // Web API routes
      config.MapHttpAttributeRoutes();

      //Applying Model Validation filter to All Actions.
      config.Filters.Add(new ValidateModelAttribute());
    }
  }
}