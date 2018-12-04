using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Push.Abstractions;
using VSS.Productivity3D.Push.Clients;
using VSS.Productivity3D.Push.Hubs;
using VSS.Productivity3D.Push.WebAPI;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Push
{
  public class Startup : BaseStartup
  {
    public Startup(IHostingEnvironment env) : base(env, "push")
    {
    }

    public override string ServiceName => "Push Service API";

    public override string ServiceDescription => "A service to manage distribution of notifications between services";

    public override string ServiceVersion => "v1";

    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
     services.AddMvc();

      // Required for authentication
      services.AddTransient<ICustomerProxy, CustomerProxy>();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddScoped<IErrorCodesProvider, PushResult>();

      services.AddPushServiceClient<INotificationHubClient, NotificationHubClient>();

      services.AddSignalR();
    }

    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory factory)
    {
      app.UseFilterMiddleware<PushAuthentication>();
      
      app.UseSignalR(route =>
      {
        route.MapHub<NotificationHub>("/notifications");
      });
      
      app.UseMvc();

    }
  }
}
