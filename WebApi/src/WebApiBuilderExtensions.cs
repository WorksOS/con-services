using System;
using Microsoft.AspNetCore.Builder;
using VSS.MasterData.Models.FIlters;

namespace VSS.WebApi.Common
{
  /// <summary>
  /// Extensions methods for adding common Web API middleware to the request execution pipeline
  /// </summary>
  public static class WebApiBuilderExtensions
  {
    /// <summary>
    /// Adds exceptions trap, CORS, Swagger, MVC, ... to the application builder
    /// </summary>
    public static IApplicationBuilder UseCommon(this IApplicationBuilder app, string serviceTitle)
    {
      if (app == null)
        throw new ArgumentNullException("app");

      app.UseExceptionTrap();


      app.UseCors("VSS");

      app.UseSwagger();

      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", serviceTitle);
      });

      //app.UseTIDAuthentication();
      app.UseMvc();
      return app;
    }
  }
}
