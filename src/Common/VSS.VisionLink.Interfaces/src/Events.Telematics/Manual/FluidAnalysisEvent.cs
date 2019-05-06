using System;
using VSS.VisionLink.Interfaces.Events.Telematics.Context;

namespace VSS.VisionLink.Interfaces.Events.Telematics.Manual
{
  public class FluidAnalysisEvent
  {
    public DeviceDetail Device { get; set; }
    public AssetDetail Asset { get; set; }
    public OwnerDetail Owner { get; set; }
    public TimestampDetail Timestamp { get; set; }
    public TracingMetadataDetail TracingMetadata { get; set; }

    public long SampleNumber { get; set; }
    public string TextID { get; set; }
    public string Description { get; set; }
    public DateTime SampleTakenDate { get; set; }
    public DateTime? SampleConfirmedUTC { get; set; }
    public string CompartmentName { get; set; }
    public string CompartmentID { get; set; }
    public double? MeterValue { get; set; }
    public string MeterValueUnit { get; set; }
    public string OverallEvaluation { get; set; }
    public string Status { get; set; }
    public long? ActionNumber { get; set; }
    public DateTime? ActionUTC { get; set; }
    public string ActionDescription { get; set; }
    public string ActionedByID { get; set; }
    public string ActionedByFirstName { get; set; }
    public string ActionedByLastName { get; set; }
  }
}
