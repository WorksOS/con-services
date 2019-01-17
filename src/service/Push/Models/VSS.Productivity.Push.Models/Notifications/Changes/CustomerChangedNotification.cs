using System;
using VSS.Productivity.Push.Models.Enums;

namespace VSS.Productivity.Push.Models.Notifications.Changes
{
  public class CustomerChangedNotification : Notification
  {
    public const string CUSTOMER_CHANGED_KEY = "CUSTOMER_CHANGED_KEY";

    public CustomerChangedNotification(Guid uid) : base(CUSTOMER_CHANGED_KEY, uid, NotificationUidType.Customer)
    {
    }
  }
}