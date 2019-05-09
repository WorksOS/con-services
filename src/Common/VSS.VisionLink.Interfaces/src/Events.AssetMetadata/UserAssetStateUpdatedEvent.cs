using VSS.VisionLink.Interfaces.Events.AssetMetadata.Context;

namespace VSS.VisionLink.Interfaces.Events.AssetMetadata
{
  public class UserAssetStateUpdatedEvent
  {
    public AssetDetail Asset { get; set; }
    public TimestampDetail Timestamp { get; set; }

    public string State { get; set; }
  }
}