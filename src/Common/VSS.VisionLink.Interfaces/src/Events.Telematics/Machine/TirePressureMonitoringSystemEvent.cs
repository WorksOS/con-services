using VSS.VisionLink.Interfaces.Events.Telematics.Context;
using VSS.VisionLink.Interfaces.Events.Telematics.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.Telematics.Machine
{
  public class TirePressureMonitoringSystemEvent : IAnnotatedEvent
  {
    public AssetDetail Asset { get; set; }
    public DeviceDetail Device { get; set; }
    public OwnerDetail Owner { get; set; }
    public TimestampDetail Timestamp { get; set; }
    public TracingMetadataDetail TracingMetadata { get; set; }

    public int AxlePosition { get; set; }
    public int? EcmCount { get; set; }
    public string EcmDescription { get; set; }
    public string EcmSourceAddress { get; set; }
    public int? EcmTireCount { get; set; }
    public int SensorAspectId { get; set; }    
    public string SensorType { get; set; }
    public double? SensorValue { get; set; }
    public int TirePosition { get; set; }
  }
}
