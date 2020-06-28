using VSS.Productivity.Push.Models.Enums;

namespace VSS.Productivity.Push.Models.Notifications.Changes
{
  public class CacheChangeNotification : Notification
  {
    public const string CACHE_CHANGED_KEY = "CACHE_CHANGED_KEY";

    public CacheChangeNotification(string cacheTag) : base(CACHE_CHANGED_KEY, cacheTag, NotificationUidType.CacheUpdate)
    {
    }
  }
}
