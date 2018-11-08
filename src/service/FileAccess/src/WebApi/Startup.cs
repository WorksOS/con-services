using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.Productivity3D.FileAccess.WebAPI.Filters;
using VSS.Productivity3D.FileAccess.WebAPI.Models.Utilities;
using VSS.TCCFileAccess;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.FileAccess.WebAPI
{
  /// <summary>
  /// Application startup class.
  /// </summary>
  public class Startup
  {
    /// <summary>
    /// The name of this service for swagger etc.
    /// </summary>
    private const string SERVICE_TITLE = "FileAccess Service API";

    /// <summary>
    /// Log4net repository logger name.
    /// </summary>
    public const string LOGGER_REPO_NAME = "WebApi";

    private IServiceCollection serviceCollection;
    private readonly ILoggerFactory loggerFactory;
    private IConfigurationRoot Configuration { get; }
   
    /// <summary>
    /// Default constructor.
    /// </summary>
    public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

      env.ConfigureLog4Net(repoName: LOGGER_REPO_NAME, configFileRelativePath: "log4net.xml");

      this.loggerFactory = loggerFactory;

      builder.AddEnvironmentVariables();
      Configuration = builder.Build();
    }

    /// <summary>
    /// Called by the runtime to instanatiate application services.
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
      var logger = loggerFactory.CreateLogger<Startup>();

      services.AddCommon<Startup>(SERVICE_TITLE);

      services.AddSingleton<IConfigurationStore, GenericConfiguration>();

      var tccUrl = (new GenericConfiguration(new LoggerFactory())).GetValueString("TCCBASEURL");
      var useMock = string.IsNullOrEmpty(tccUrl) || tccUrl == "mock";
      
      logger.LogInformation($"Application initialized with TCCBASEURL={tccUrl}, UseMock={useMock}");

      if (useMock)
        services.AddTransient<IFileRepository, MockFileRepository>();
      else
        services.AddTransient<IFileRepository, FileRepository>();

      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
          options.Hosting.IgnorePatterns.Add(request => request.Request.GetUri().ToString().Contains("newrelic.com"));
        });
      });

      services.AddJaeger(SERVICE_TITLE);
      services.AddOpenTracing();

      serviceCollection = services;
    }

    /// <summary>
    /// Called by the runtime to configure the HTTP request pipeline.
    /// </summary>
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.BuildServiceProvider();

      app.UseCommon(SERVICE_TITLE);

      if (Configuration["newrelic"] == "true")
      {
        app.UseMiddleware<NewRelicMiddleware>();
      }
      app.UseMvc();
    }
  }
}
