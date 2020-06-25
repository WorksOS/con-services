using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.Common.ServiceDiscovery;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Entitlements.Authentication;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using VSS.Productivity3D.Push.WebAPI;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Entitlements
{
    /// <summary>
    /// VSS.Productivity3D.Entitlements application startup.
    /// </summary>
    public class Startup : BaseStartup
    {
        /// <inheritdoc />
        public override string ServiceName => "Entitlements API";

        /// <inheritdoc />
        public override string ServiceDescription => "A service to handle Entitlements";

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
            services.AddSingleton<IConfigurationStore, GenericConfiguration>();
            services.AddTransient<IWebRequest, GracefulWebRequest>();

            services.AddServiceDiscovery();
            services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
            services.AddScoped<IErrorCodesProvider, EntitlementsExecutionStates>();

            services.AddPushServiceClient<INotificationHubClient, NotificationHubClient>();
            services.AddSingleton<CacheInvalidationService>();

            services.AddOpenTracing(builder =>
            {
                builder.ConfigureAspNetCore(options =>
                {
                    options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
                });
            });
            services.AddHostedService<InvalidateEntitlementsService>();
        }

        /// <inheritdoc />
        protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory factory)
        {
          app.UseMiddleware<AuthenticationMiddleware>();
        }
    }
}


