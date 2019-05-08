using VSS.VisionLink.Interfaces.Events.Telematics.Context;
using VSS.VisionLink.Interfaces.Events.Telematics.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.Telematics.Machine
{
  public class FenceAlarmEvent : IAnnotatedEvent
  {
    public AssetDetail Asset { get; set; }
    public DeviceDetail Device { get; set; }
    public OwnerDetail Owner { get; set; }
    public TimestampDetail Timestamp { get; set; }
    public TracingMetadataDetail TracingMetadata { get; set; }

    public bool DisconnectSwitchUsed { get; set; }
    public FenceDetail ExclusiveFence { get; set; }
    public FenceDetail InclusiveFence { get; set; }
    public bool SatelliteBlockage { get; set; }
    public FenceDetail TimeFence { get; set; }
  }
}
