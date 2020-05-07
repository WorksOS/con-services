using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Cache.MemoryCache;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.WebApi.Common.Swagger;

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
    public static IServiceCollection AddCommon<T>(this IServiceCollection services, string serviceTitle, string serviceDescription = null, string version = "v1")
    {
      if (services == null) { throw new ArgumentNullException(nameof(services)); }
      
      // Guard against BaseStartup::ServiceVersion not being implemented in Startup.cs; else the following call to setup Swagger fails with 'null value for key'.
      // Hard code to v1, as it's not important (our endpoints have quite differening versions)
      // This must match whats provided in the SwaggerEndpoint call
      version = "v1";
      
      //Configure swagger
      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc(version, new OpenApiInfo  { Title = serviceTitle, Description = serviceDescription, Version = version });
        // Allows swagger to index models on it's full name, rather than class name - which causes conflicts if a class name is used more than once in the swagger documentation
        // https://stackoverflow.com/questions/46071513/swagger-error-conflicting-schemaids-duplicate-schemaids-detected-for-types-a-a
        c.CustomSchemaIds(x => x.FullName);
        c.OperationFilter<AddRequiredHeadersParameter>();
        c.DescribeAllParametersInCamelCase();
      });

      services.ConfigureSwaggerGen(options =>
      {
        string pathToXml;

        var moduleName = Assembly.GetEntryAssembly().ManifestModule.Name;
        var assemblyName = moduleName.Substring(0, moduleName.LastIndexOf('.'));

        if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), assemblyName + ".xml")))
          pathToXml = Directory.GetCurrentDirectory();
        else if (File.Exists(Path.Combine(AppContext.BaseDirectory, assemblyName + ".xml")))
          pathToXml = AppContext.BaseDirectory;
        else
        {
          var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
          pathToXml = Path.GetDirectoryName(pathToExe);
        }
        options.IncludeXmlComments(Path.Combine(pathToXml, assemblyName + ".xml"));
        options.IgnoreObsoleteProperties();
        options.DescribeAllEnumsAsStrings();
      });

      services.AddMemoryCache();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddSingleton<IDataCache, InMemoryDataCache>();
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
      services.AddTransient<IErrorCodesProvider, ContractExecutionStatesEnum>();//Replace with custom error codes provider if required
      services.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>();

      return services;
    }
  }
}
