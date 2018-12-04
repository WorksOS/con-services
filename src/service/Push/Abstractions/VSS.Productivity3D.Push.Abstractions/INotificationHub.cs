using System.Threading.Tasks;
using VSS.Productivity.Push.Models.Notifications;

namespace VSS.Productivity3D.Push.Abstractions
{
  public interface INotificationHub
  {
    /// <summary>
    /// Send a notification to other services / clients
    /// </summary>
    Task Notify(Notification notification);
  }
}