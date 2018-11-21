using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using VSS.MasterData.Models.FIlters;

namespace VSS.WebApi.Common
{
  /// <summary>
  /// Extensions methods for setting up common Web API service configuration
  /// </summary>
  public static class ServiceCollectionExtensions
  {
    /// <summary>
    /// Adds CORS, MVC, Swagger ... for Web API service. T is the Startup class for the service.
    /// </summary>
    public static IServiceCollection AddCommon<T>(this IServiceCollection services, string serviceTitle, string serviceDescription=null, string version="v1")
    {
      if (services == null)
        throw new ArgumentNullException("services");

      services.AddCors(options =>
      {
        options.AddPolicy("VSS", builder => builder.AllowAnyOrigin()
          .WithHeaders("Origin", "X-Requested-With", "Content-Type", "Accept", "Authorization",
            "X-VisionLink-CustomerUID", "X-VisionLink-UserUid", "X-Jwt-Assertion", "X-VisionLink-ClearCache", "Cache-Control")
          .WithMethods("OPTIONS", "TRACE", "GET", "HEAD", "POST", "PUT", "DELETE")
          .SetPreflightMaxAge(TimeSpan.FromSeconds(2520)));
      });

      services.AddMvc(
        config =>
        {
          config.Filters.Add(new ValidationFilterAttribute());
        }
      );

      //Configure swagger
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc(version, new Info { Title = serviceTitle, Description = serviceDescription, Version = version });
        // Allows swagger to index models on it's full name, rather than class name - which causes conflicts if a class name is used more than once in the swagger documentation
        // https://stackoverflow.com/questions/46071513/swagger-error-conflicting-schemaids-duplicate-schemaids-detected-for-types-a-a
        c.CustomSchemaIds(x => x.FullName);
      });

      services.ConfigureSwaggerGen(options =>
      {
        string pathToXml;

        var moduleName = typeof(T).GetTypeInfo().Assembly.ManifestModule.Name;
        var assemblyName = moduleName.Substring(0, moduleName.LastIndexOf('.'));

        if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), assemblyName + ".xml")))
          pathToXml = Directory.GetCurrentDirectory();
        else if (File.Exists(Path.Combine(System.AppContext.BaseDirectory, assemblyName + ".xml")))
          pathToXml = System.AppContext.BaseDirectory;
        else
        {
          var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
          pathToXml = Path.GetDirectoryName(pathToExe);
        }
        options.IncludeXmlComments(Path.Combine(pathToXml, assemblyName + ".xml"));
        options.IgnoreObsoleteProperties();
        options.DescribeAllEnumsAsStrings();
      });

      return services;
    }
  }
}
