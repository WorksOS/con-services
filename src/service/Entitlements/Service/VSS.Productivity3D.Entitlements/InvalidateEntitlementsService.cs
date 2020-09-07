using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Productivity.Push.Models.Notifications.Changes;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Response;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Entitlements.WebApi
{
  public class InvalidateEntitlementsService : BaseHostedService
  {
    public InvalidateEntitlementsService(ILoggerFactory loggerFactory, IServiceScopeFactory scopeFactory) : base(loggerFactory, scopeFactory)
    {
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
      // If we restart our entitlements service, we should just invalidate other services just in case
      var scope = ScopeFactory.CreateScope();
      var notificationHub = scope.ServiceProvider.GetService<INotificationHubClient>();

      while (!notificationHub.Connected)
      {
        Logger.LogInformation($"Waiting for notification hub to come online...");
        await Task.Delay(10000, cancellationToken);
      }

      Logger.LogInformation($"Notification hub client is online, sending message to invalidate entitlements");
      await notificationHub.Notify(new CacheChangeNotification(EntitlementResponseModel.EntitlementCacheTag));
    }
  }
}
