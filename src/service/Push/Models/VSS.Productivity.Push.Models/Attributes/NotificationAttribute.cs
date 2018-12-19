using System;
using VSS.Productivity.Push.Models.Enums;

namespace VSS.Productivity.Push.Models.Attributes
{
  /// <summary>
  /// Attribute to be used when requiring a method to be invoked on a certain type of notification from the INotificationHubClient
  /// The Key and Type must be an exact match for this Attribute to be hit
  /// Note: The method signature *must be* 'void method(Guid uid)` where the Uid is for the Type in question
  /// </summary>
  [AttributeUsage(AttributeTargets.Method, Inherited = false)]
  public class NotificationAttribute : Attribute
  {
    /// <summary>
    /// Notification Type required
    /// </summary>
    public NotificationUidType Type { get; }

    /// <summary>
    /// Specific Key for the notification
    /// </summary>
    public string Key { get; }

    public NotificationAttribute(NotificationUidType type, string key)
    {
      Type = type;
      Key = key;
    }
  }
}