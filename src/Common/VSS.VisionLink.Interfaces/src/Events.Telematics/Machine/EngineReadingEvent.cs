using VSS.VisionLink.Interfaces.Events.Telematics.Context;
using VSS.VisionLink.Interfaces.Events.Telematics.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.Telematics.Machine
{
  // Note: This is formerly data residing in NH_DATA..CustomDataEngineParameters.
  public class EngineReadingEvent : IAnnotatedEvent
  {
    public AssetDetail Asset { get; set; }
    public DeviceDetail Device { get; set; }
    public OwnerDetail Owner { get; set; }
    public TimestampDetail Timestamp { get; set; }
    public TracingMetadataDetail TracingMetadata { get; set; }

    public EcmDetail Ecm { get; set; }
    public EngineDetail Engine { get; set; }
    public string Timezone { get; set; }
  }
}
