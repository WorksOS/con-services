using System.Threading.Tasks;
using VSS.Productivity.Push.Models;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity3D.Push.Abstractions;

namespace VSS.Productivity3D.Push.Hubs
{
  public class NotificationHub : AuthenticatedHub<INotificationHub>, INotificationHub
  {
    public Task Notify(Notification notification)
    {
      return Clients.All.Notify(notification);
    }
  }
}