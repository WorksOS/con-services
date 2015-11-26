using System.Web.Http;
using VSP.MasterData.Project.WebAPI.Filters;

namespace VSP.MasterData.Project.WebAPI
{
  public static class WebApiConfig
  {
    public static void Register(HttpConfiguration config)
    {
      // Web API configuration and services

      // Web API routes
      config.MapHttpAttributeRoutes();

      //Applying Model Validation filter to All Actions.
      config.Filters.Add(new ValidateModelAttribute());
    }
  }
}