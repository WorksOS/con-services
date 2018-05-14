using System.Reflection;
using System.IO;
using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.TCCFileAccess;
using VSS.WebApi.Common;

#if NET_4_7
using VSS.Productivity3D.FileAccess.Service.WebAPI.Filters;
#endif

namespace VSS.Productivity3D.FileAccess.Service.WebAPI
{
  public class Startup
  {
    /// <summary>
    /// The name of this service for swagger etc.
    /// </summary>
    private const string SERVICE_TITLE = "3dpm Service API";

    /// <summary>
    /// Log4net repository logger name.
    /// </summary>
    public const string LOGGER_REPO_NAME = "WebApi";

    public IConfigurationRoot Configuration { get; }
    private IServiceCollection serviceCollection;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="env"></param>
    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
          .SetBasePath(env.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

      env.ConfigureLog4Net("log4net.xml", LOGGER_REPO_NAME);

      builder.AddEnvironmentVariables();
      Configuration = builder.Build();
    }

    /// <summary>
    /// Called by the runtime to instanatiate application services.
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddCommon<Startup>(SERVICE_TITLE, "API for 3D File Access");

      services.AddLogging();

      //Configure CORS
      services.AddCors(options =>
      {
        options.AddPolicy("VSS", builder => builder.AllowAnyOrigin()
                  .WithHeaders("Origin", "X-Requested-With", "Content-Type", "Accept", "Authorization",
                      "X-VisionLink-CustomerUid", "X-VisionLink-UserUid", "Cache-Control")
                  .WithMethods("OPTIONS", "TRACE", "GET", "HEAD", "POST", "PUT", "DELETE"));
      });

      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new Info { Title = "File Access API", Description = "API for File Access", Version = "v1" });
      });

      services.ConfigureSwaggerGen(options =>
      {
        string pathToXml;

        var moduleName = typeof(Startup).GetTypeInfo().Assembly.ManifestModule.Name;
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

      //Configure application services
      services
        .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddSingleton<IFileRepository, FileRepository>();

      services.AddMvc();

      serviceCollection = services;
    }

    /// <summary>
    /// Called by the runtime to configure the HTTP request pipeline.
    /// </summary>
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(LOGGER_REPO_NAME);

      //Enable CORS before TID so OPTIONS works without authentication
      app.UseCommon("VSS");

#if NET_4_7
      if (Configuration["newrelic"] == "true")
      {
        app.UseMiddleware<NewRelicMiddleware>();
      }
#endif

      app.UseMvc();
    }
  }
}
