using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity3D.Push.Abstractions.Notifications;

namespace VSS.Productivity3D.Push.Hubs
{
  public class NotificationHub : AuthenticatedHub<INotificationHub>, INotificationHub
  {
    public Task Notify(Notification notification)
    {
      return Clients.All.Notify(notification);
    }

    public NotificationHub(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
    }
  }
}
