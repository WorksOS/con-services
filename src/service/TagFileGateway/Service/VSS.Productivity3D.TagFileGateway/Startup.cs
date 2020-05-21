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
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Proxy;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Productivity3D.Push.WebAPI;
using VSS.Productivity3D.TagFileGateway.Common.Abstractions;
using VSS.Productivity3D.TagFileGateway.Common.Proxy;
using VSS.Productivity3D.TagFileGateway.Common.Services;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.TagFileGateway
{
    /// <summary>
    /// VSS.Productivity3D.TagFileGateway application startup.
    /// </summary>
    public class Startup : BaseStartup
    {
        /// <inheritdoc />
        public override string ServiceName => "Tag File Gateway API";

        /// <inheritdoc />
        public override string ServiceDescription => "A service to Accept TAG Files";

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
            services.AddScoped<IErrorCodesProvider, TagFileGatewayExecutionStates>();
            services.AddTransient<ITagFileForwarder, TagFileForwarderProxy>();
            services.AddTransient<ITransferProxy, TransferProxy>(provider => 
              ActivatorUtilities.CreateInstance<TransferProxy>(provider, "AWS_ALL_TAGFILE_BUCKET_NAME"));

            services.AddPushServiceClient<INotificationHubClient, NotificationHubClient>();
            services.AddSingleton<CacheInvalidationService>();

            services.AddHostedService<TagFileSqsService>();

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


