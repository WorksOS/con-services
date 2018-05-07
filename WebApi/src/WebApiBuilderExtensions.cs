using System;
using Microsoft.AspNetCore.Builder;

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
      app.UseFilterMiddleware<RequestIDMiddleware>();

      app.UseSwagger();
      //Swagger documentation can be viewed with http://localhost:5000/swagger/v1/swagger.json
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", serviceTitle);
      });

      //TIDAuthentication added by those servicesd which need it
      //MVC must be last; added by individual services after their custom services.
      return app;
    }
  }
}
