using App.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using VSS.Common.ServiceDiscovery;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.FIlters;

namespace VSS.WebApi.Common
{
  /// <summary>
  /// Base Startup class which takes care of a lot of repetitive setup, such as logger, swagger etc
  /// </summary>
  public abstract class BaseStartup
  {
    /// <summary>
    /// Base constructor which setups up a configuration based on appsettings.json and Environment Variables
    /// </summary>
    /// <param name="env">Hosting Env</param>
    /// <param name="loggerRepoName">Logger Repo Name for Log4Net</param>
    protected BaseStartup(IHostingEnvironment env, string loggerRepoName)
    {
      Log4NetProvider.RepoName = loggerRepoName;
      env.ConfigureLog4Net("log4net.xml", loggerRepoName);
    }

    //Backing field
    private ILogger _logger;
    private IConfigurationStore _configuration;


    /// <summary>
    /// The service collection reference
    /// </summary>
    protected IServiceCollection Services { get; private set; }

    /// <summary>
    /// Gets the default IServiceProvider.
    /// </summary>
    protected ServiceProvider ServiceProvider { get; private set; }

    /// <summary>
    /// Provides access to configuration settings
    /// </summary>
    protected IConfigurationStore Configuration
    {
      get
      {
        if (_configuration == null)
        {
          _configuration = new GenericConfiguration(new NullLoggerFactory());
        }
        return _configuration;
      }
      set => _configuration = value;
    }
      

    /// <summary>
    /// Gets the ILogger type used for logging.
    /// </summary>
    protected ILogger Log
    {
      get
      {
        if (_logger == null)
        {
          _logger = new LoggerFactory().AddConsole().CreateLogger(nameof(BaseStartup));
        }
        return _logger;
      }
      set => _logger = value;
    }



    /// <summary>
    /// The name of this service for swagger etc.
    /// </summary>
    public abstract string ServiceName { get; }

    /// <summary>
    /// The service description, used for swagger documentation
    /// </summary>
    public abstract string ServiceDescription { get; }

    /// <summary>
    /// The service version, used for swagger documentation
    /// </summary>
    public abstract string ServiceVersion { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddCommon<BaseStartup>(ServiceName, ServiceDescription, ServiceVersion);
      services.AddJaeger(ServiceName);
      services.AddServiceDiscovery();

      services.AddMvcCore(
        config =>
        {
          // for jsonProperty validation
          config.Filters.Add(new ValidationFilterAttribute());
        }).AddMetricsCore();

      var metrics = AppMetrics.CreateDefaultBuilder()
        .Build();

      services.AddMetrics(metrics);
      services.AddMetricsTrackingMiddleware();

      ConfigureAdditionalServices(services);

      Services = services;

      ServiceProvider = Services.BuildServiceProvider();
      Log = ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(GetType().Name);
      Configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
    }

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// This method gets called by the run time
    /// </summary>
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      app.UseMetricsAllMiddleware();
      app.UseCommon(ServiceName);

      if (Configuration.GetValueBool("newrelic").HasValue && Configuration.GetValueBool("newrelic").Value)
      {
        app.UseMiddleware<NewRelicMiddleware>();
      }

      Services.AddSingleton(loggerFactory);
      ConfigureAdditionalAppSettings(app, env, loggerFactory);

      app.UseMvc();
    }

    /// <summary>
    /// Extra configuration that would normally be in ConfigureServices
    /// This is useful for binding interfaces to implementations
    /// </summary>
    protected abstract void ConfigureAdditionalServices(IServiceCollection services);

    /// <summary>
    /// Extra app and env setup options
    /// Useful for adding ASP related options, such as filter MiddleWhere
    /// </summary>
    protected abstract void ConfigureAdditionalAppSettings(IApplicationBuilder app,
      IHostingEnvironment env,
      ILoggerFactory factory);
  }
}
