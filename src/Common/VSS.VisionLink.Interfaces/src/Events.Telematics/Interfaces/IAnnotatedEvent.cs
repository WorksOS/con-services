using VSS.VisionLink.Interfaces.Events.Telematics.Context;

namespace VSS.VisionLink.Interfaces.Events.Telematics.Interfaces
{
  public interface IAnnotatedEvent
  {
    AssetDetail Asset { get; set; }
    DeviceDetail Device { get; set; }
    OwnerDetail Owner { get; set; }
    TimestampDetail Timestamp { get; set; }
    TracingMetadataDetail TracingMetadata { get; set; }
  }
}
