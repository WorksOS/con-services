using VSS.VisionLink.Interfaces.Events.Telematics.Context;
using VSS.VisionLink.Interfaces.Events.Telematics.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.Telematics.Machine
{
  public class TirePressureMonitorInfoEvent : IAnnotatedEvent
  {
    public AssetDetail Asset { get; set; }
    public DeviceDetail Device { get; set; }
    public OwnerDetail Owner { get; set; }
    public TimestampDetail Timestamp { get; set; }
    public TracingMetadataDetail TracingMetadata { get; set; }

    public int AxlePosition { get; set; }
    public string Description { get; set; }
    public string EcmId { get; set; }
    public bool? InstallationStatus { get; set; }
    public string SensorId { get; set; }
    public string SourceAddress { get; set; }
    public int SourceId { get; set; }
    public int? TireCount { get; set; }
    public int TirePosition { get; set; }
  }
}
