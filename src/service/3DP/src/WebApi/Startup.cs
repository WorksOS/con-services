using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.ServiceDiscovery;
using VSS.Log4NetExtensions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Caching;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Push.Abstractions;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Productivity3D.Push.WebAPI;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;
using VSS.WebApi.Common;
using WebApiContrib.Core.Formatter.Protobuf;

namespace VSS.Productivity3D.WebApi
{
  public partial class Startup : BaseStartup
  {
    /// <inheritdoc />
    public override string ServiceName => "3dpm Service API";

    /// <inheritdoc />
    public override string ServiceDescription => "API for 3D compaction and volume data";

    /// <inheritdoc />
    public override string ServiceVersion => "v1";
    
    /// <summary>
    /// Log4net repository logger name.
    /// </summary>
    internal const string LoggerRepoName = "WebApi";
    
    /// <summary>
    /// Gets the default configuration object.
    /// </summary>
    public IConfigurationRoot Configuration { get; }

    /// <inheritdoc />
    public Startup(IHostingEnvironment env) : base(env, LoggerRepoName)
    {
      var builder = new ConfigurationBuilder()
          .SetBasePath(env.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

      builder.AddEnvironmentVariables();
      Configuration = builder.Build();

      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }
    
    /// <inheritdoc />
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      services.AddMvcCore(options =>
      {
        options.OutputFormatters.Add(new ProtobufOutputFormatter(new ProtobufFormatterOptions()));
      });

      services.AddResponseCompression();
      services.AddCustomResponseCaching();
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

      services.AddPushServiceClient<INotificationHubClient, NotificationHubClient>();
      services.AddSingleton<CacheInvalidationService>();

      services.AddTransient<IWebRequest, GracefulWebRequest>();
      services.AddServiceDiscovery();

      /*services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
        });
      });

      services.AddJaeger(SERVICE_TITLE);

      services.AddOpenTracing();*/

      ConfigureApplicationServices(services);
    }

    /// <inheritdoc />
    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory factory)
    {
      app.UseFilterMiddleware<RaptorAuthentication>();
  
      app.UseRewriter(new RewriteOptions().Add(RewriteMalformedPath));
      app.UseResponseCaching();
      app.UseResponseCompression();
      app.UseMvc();

      //TODO: Remove this when our custom response caching fixed
      if (CustomCachingPolicyProvider.ResponseCachingDisabled)
      {
        Log.LogWarning("Response caching disabled");
      }
#if RAPTOR
      CheckRaptorAvailabilityIfRequired();
#endif
    }
#if RAPTOR
    /// <summary>
    /// Checks whether the Raptor is available if the condition is met.
    /// </summary>
    private void CheckRaptorAvailabilityIfRequired()
    {
      if (Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_CMV") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_PASSCOUNT") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_MDP") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_CUTFILL") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_SPEED") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_TEMPERATURE") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_VOLUMES") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_TILES") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_SURFACE") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_VETA") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_EXPORT_PASSCOUNT") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_DESIGN_BOUNDARY") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_PROFILING") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_GRIDREPORT") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_STATIONOFFSET") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_LINEWORKFILE") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_CCA") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_CS") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_PATCHES") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_CELL_DATUM") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_PROJECTSTATISTICS") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_MACHINES") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_MACHINEDESIGNS") != "true" ||
          Environment.GetEnvironmentVariable("ENABLE_TREX_GATEWAY_LAYERS") != "true" )
        ConfigureRaptor();
    }

    /// <summary>
    /// Check if the configuration is correct and we are able to connect to Raptor.
    /// </summary>
    private void ConfigureRaptor()
    {
      Log.LogInformation("Testing Raptor configuration with sending config request");

      try
      {
        ServiceProvider.GetRequiredService<IASNodeClient>().RequestConfig(out string config);
        if (Log.IsTraceEnabled())
          Log.LogTrace("Received config {0}", config);

        if (config.Contains("Error retrieving Raptor config")) { throw new Exception(config); }
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception loading config");
        Log.LogCritical("Can't talk to Raptor for some reason - check configuration");
        Environment.Exit(138);
      }
    }
#endif
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

      Log.LogInformation($"Path rewritten to: '{newPathString}'");
      request.Path = new PathString(newPathString);
    }
  }
}
