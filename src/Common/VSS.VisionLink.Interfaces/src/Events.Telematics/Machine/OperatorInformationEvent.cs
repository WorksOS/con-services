using VSS.VisionLink.Interfaces.Events.Telematics.Context;
using VSS.VisionLink.Interfaces.Events.Telematics.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.Telematics.Machine
{
  public class OperatorInformationEvent : IAnnotatedEvent

  {
    public AssetDetail Asset { get; set; }
    public DeviceDetail Device { get; set; }
    public OwnerDetail Owner { get; set; }
    public TimestampDetail Timestamp { get; set; }
    public TracingMetadataDetail TracingMetadata { get; set; }

    public string FullName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string ExternalCustomerKeyType { get; set; }
    public string ExternalCustomerKeyValue { get; set; }
    public string ExternalOperatorID { get; set; }
    public string Description { get; set; }
  }
}
