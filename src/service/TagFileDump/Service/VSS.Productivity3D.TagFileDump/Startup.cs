using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.Common.ServiceDiscovery;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Filter.Proxy;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Proxy;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Proxy;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Productivity3D.Push.WebAPI;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.TagFileDump
{
  /// <summary>
  /// VSS.Productivity3D.TagFileDump application startup.
  /// </summary>
  public class Startup : BaseStartup
  {
    /// <inheritdoc />
    public override string ServiceName => "Tag File Dump API";

    /// <inheritdoc />
    public override string ServiceDescription => "A service to Dump TAG Files to S3";

    /// <inheritdoc />
    public override string ServiceVersion => "v1";
    
    /// <summary>
    /// Configures services and the application request pipeline.
    /// </summary>
    public Startup()
    { }

    /// <inheritdoc />
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {

      // Required for authentication
      services.AddTransient<IProductivity3dV2ProxyCompaction, Productivity3dV2ProxyCompaction>();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddTransient<IWebRequest, GracefulWebRequest>();


      services.AddServiceDiscovery();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddScoped<IErrorCodesProvider, TagFileDumpExecutionStates>();

      services.AddScoped<ITransferProxy>(sp => new TransferProxy(sp.GetRequiredService<IConfigurationStore>(), sp.GetService<ILogger<TransferProxy>>(), "AWS_ALL_TAGFILE_BUCKET_NAME"));

      services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
        });
      });
    }

    /// <inheritdoc />
    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory factory)
    {
    }
  }
}
