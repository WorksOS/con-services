using System;
using VSS.Productivity.Push.Models.Enums;

namespace VSS.Productivity.Push.Models.Notifications
{
  /// <summary>
  /// Project specific notification, which sets the type
  /// </summary>
  public abstract class ProjectNotification : Notification
  {
    /// <summary>
    /// Project Notification
    /// </summary>
    /// <param name="key">Notification Key</param>
    /// <param name="projectUid">Project Uid</param>
    protected ProjectNotification(string key, Guid projectUid) : base(key, projectUid, NotificationUidType.Project)
    {
    }
  }
}