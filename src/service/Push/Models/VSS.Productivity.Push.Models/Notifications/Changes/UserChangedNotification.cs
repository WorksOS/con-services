using System;
using VSS.Productivity.Push.Models.Enums;

namespace VSS.Productivity.Push.Models.Notifications.Changes
{
  public class UserChangedNotification : Notification
  {
    public const string USER_CHANGED_KEY = "USER_CHANGED_KEY";

    public UserChangedNotification(Guid uid) : base(USER_CHANGED_KEY, uid, NotificationUidType.User)
    {
    }
  }
}