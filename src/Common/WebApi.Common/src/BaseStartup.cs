using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Log4Net.Extensions;

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

    /// <summary>
    /// The service collection reference
    /// </summary>
    protected IServiceCollection Services { get; private set; }

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
      services.AddMemoryCache();
      
      Services = services;
      ConfigureAdditionalServices(services);
    }

    // ReSharper disable once UnusedMember.Global
    /// <summary>
    /// This method gets called by the run time
    /// </summary>
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      app.UseCommon(ServiceName);

      Services.AddSingleton(loggerFactory);
      ConfigureAdditionalAppSettings(app, env, loggerFactory);
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