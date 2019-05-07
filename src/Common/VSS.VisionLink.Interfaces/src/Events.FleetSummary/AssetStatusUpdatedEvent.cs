using VSS.VisionLink.Interfaces.Events.FleetSummary.Context;

namespace VSS.VisionLink.Interfaces.Events.FleetSummary
{
  public class AssetStatusUpdatedEvent
  {
    public AssetDetail Asset { get; set; }
    public DeviceDetail Device { get; set; }
    public TimestampDetail Timestamp { get; set; }

    public string Status { get; set; }
  }
}
