using VSS.VisionLink.Interfaces.Events.Notifications.Context;

namespace VSS.VisionLink.Interfaces.Events.Notifications.Updates
{
  public class UserAssetNotificationCountUpdatedEvent
  {
    public UserAssetDetail UserAsset { get; set; }
    public TimestampDetail Timestamp { get; set; }

    public string State { get; set; }
    public string CustomerUid { get; set; }
  }
}
