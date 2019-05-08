using VSS.VisionLink.Interfaces.Events.Telematics.Context;
using VSS.VisionLink.Interfaces.Events.Telematics.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.Telematics.Machine
{
  public class ParametersEvent : IAnnotatedEvent
  {
    public AssetDetail Asset { get; set; }
    public DeviceDetail Device { get; set; }
    public OwnerDetail Owner { get; set; }
    public TimestampDetail Timestamp { get; set; }
    public TracingMetadataDetail TracingMetadata { get; set; }

    public int EcmSourceAddress { get; set; }
    public int Pgn { get; set; }
    public int Severity { get; set; }
    public int Spn { get; set; }
    public string UnitType { get; set; }
    public double? ValueDouble { get; set; }
    public string ValueString { get; set; }
  }
}
