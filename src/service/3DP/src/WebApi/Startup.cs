using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.ServiceDiscovery;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Productivity3D.Push.WebAPI;
using VSS.Productivity3D.WebApi.Middleware;
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
    /// Gets the default configuration object.
    /// </summary>
    public IConfigurationRoot ConfigurationRoot { get; }

    /// <inheritdoc />
    public Startup()
    {
      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();
    }

    /// <inheritdoc />
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      // Output formatters are honored in the order added so we repeat this part of what's done
      // in BaseStartup to ensure Protobuf comes after NewtonsoftJson.
      services.AddControllers().AddNewtonsoftJson(options =>
      {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
      }).AddProtobufFormatters();

      services.AddResponseCompression();
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
      services.AddPushServiceClient<INotificationHubClient, NotificationHubClient>();
      services.AddSingleton<CacheInvalidationService>();
      services.AddSingleton<ITransferProxyFactory, TransferProxyFactory>();

      services.AddTransient<IWebRequest, GracefulWebRequest>();
      services.AddServiceDiscovery();

      ConfigureApplicationServices(services);
    }

    /// <inheritdoc />
    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory factory)
    {
      app.UseFilterMiddleware<RaptorAuthentication>();

      app.UseRewriter(new RewriteOptions().Add(URLRewriter.RewriteMalformedPath));
      app.UseResponseCaching();
      app.UseResponseCompression();
    }
  }
}
