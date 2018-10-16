using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.Productivity3D.Common.Filters;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Caching;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.WebApi.Common;
using WebApiContrib.Core.Formatter.Protobuf;

namespace VSS.Productivity3D.WebApi
{
  public partial class Startup
  {
    /// <summary>
    /// The name of this service for swagger etc.
    /// </summary>
    private const string SERVICE_TITLE = "3dpm Service API";

    /// <summary>
    /// Log4net repository logger name.
    /// </summary>
    public const string LOGGER_REPO_NAME = "WebApi";

    private ILogger log;
    private IServiceCollection serviceCollection;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
          .SetBasePath(env.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

      env.ConfigureLog4Net(repoName: LOGGER_REPO_NAME, configFileRelativePath: "log4net.xml");

      builder.AddEnvironmentVariables();
      Configuration = builder.Build();

      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    /// <summary>
    /// Gets the default configuration object.
    /// </summary>
    public IConfigurationRoot Configuration { get; }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddCommon<Startup>(SERVICE_TITLE, "API for 3D compaction and volume data");

      services.AddMvcCore(options =>
      {
        options.OutputFormatters.Add(new ProtobufOutputFormatter(new ProtobufFormatterOptions()));
      });

      services.AddResponseCompression();
      services.AddMemoryCache();
      services.AddCustomResponseCaching();
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

      /*services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
          options.Hosting.IgnorePatterns.Add(request => request.Request.GetUri().ToString().Contains("newrelic.com"));
        });
      });

      services.AddJaeger(SERVICE_TITLE);

      services.AddOpenTracing();*/


      ConfigureApplicationServices(services);
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    /// </summary>
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      /*loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();*/
      
      serviceCollection.AddSingleton(loggerFactory);
      var serviceProvider = serviceCollection.BuildServiceProvider();

      log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(Startup));

      //Enable CORS before TID so OPTIONS works without authentication
      app.UseCommon(SERVICE_TITLE);

      app.UseFilterMiddleware<RaptorAuthentication>();

      //Add stats
      if (Configuration["newrelic"] == "true")
      {
        app.UseFilterMiddleware<NewRelicMiddleware>();
      }

      app.UseRewriter(new RewriteOptions().Add(RewriteMalformedPath));
      app.UseResponseCaching();
      app.UseResponseCompression();
      app.UseMvc();

      CheckRaptorAvailabilityIfRequired(serviceProvider);
    }

    /// <summary>
    /// Checks whether the Raptor is available if the condition is met.
    /// </summary>
    /// <param name="serviceProvider"></param>
    private void CheckRaptorAvailabilityIfRequired(ServiceProvider serviceProvider)
    {
      if (Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_CMV") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_PASSCOUNT") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_MDP") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_CUTFILL") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_SPEED") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_TEMPERATURE") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_VOLUMES") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_TILES") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_SURFACE") != "true")
        ConfigureRaptor(serviceProvider);
    }

    /// <summary>
    /// Check if the configuration is correct and we are able to connect to Raptor.
    /// </summary>
    private void ConfigureRaptor(ServiceProvider serviceProvider)
    {
      log.LogInformation("Testing Raptor configuration with sending config request");

      try
      {
        serviceProvider.GetRequiredService<IASNodeClient>().RequestConfig(out string config);
        log.LogTrace("Received config {0}", config);
        if (config.Contains("Error retrieving Raptor config"))
          throw new Exception(config);
      }
      catch (Exception e)
      {
        log.LogError("Exception loading config: {0} at {1}", e.Message, e.StackTrace);
        log.LogCritical("Can't talk to Raptor for some reason - check configuration");
        Environment.Exit(138);
      }
    }

    /// <summary>
    /// Custom URL rewriter. Replaces all occurrences of // with /.
    /// </summary>
    /// <remarks>
    /// This catch all is needed to work around errant behaviour in TBC where invalid path strings are sent to 3DP.
    /// If that issue is addressed this rewrite could be removed.
    /// Note: Custom rewriter is required as framework AddRewrite() method doesn't cater for our needs.
    /// </remarks>
    private void RewriteMalformedPath(RewriteContext context)
    {
      var request = context.HttpContext.Request;

      var regex = new Regex(@"(?<!:)(\/\/+)");

      if (!regex.IsMatch(request.Path.Value))
      {
        return;
      }

      var newPathString = regex.Replace(request.Path.Value, "/");

      log.LogInformation($"Path rewritten to: '{newPathString}'");
      request.Path = new PathString(newPathString);
    }
  }
}
