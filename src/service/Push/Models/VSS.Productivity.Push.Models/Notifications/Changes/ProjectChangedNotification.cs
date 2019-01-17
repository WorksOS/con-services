using System;
using VSS.Productivity.Push.Models.Enums;

namespace VSS.Productivity.Push.Models.Notifications.Changes
{
  public class ProjectChangedNotification : Notification
  {
    public const string PROJECT_CHANGED_KEY = "PROJECT_CHANGED_KEY";

    public ProjectChangedNotification(Guid uid) : base(PROJECT_CHANGED_KEY, uid, NotificationUidType.Project)
    {
    }
  }
}