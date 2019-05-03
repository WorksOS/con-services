using System;
using VSS.VisionLink.Interfaces.Events.Telematics.Context;

namespace VSS.VisionLink.Interfaces.Events.Telematics.Manual
{
  public class CatInspectionEvent
  {
    public DeviceDetail Device { get; set; }
    public AssetDetail Asset { get; set; }
    public OwnerDetail Owner { get; set; }
    public TimestampDetail Timestamp { get; set; }
    public TracingMetadataDetail TracingMetadata { get; set; }

    public string InspectionName { get; set; }
    public DateTime InspectionUTC { get; set; }
    public string InspectorName { get; set; }
    public string Type { get; set; }
    public string System { get; set; }
    public string Number { get; set; }
    public string Description { get; set; }
    public string StatusLabel { get; set; }
    public int Severity { get; set; }
    public string MeterValue { get; set; }
    public string MeterUnit { get; set; }
    public string InspectorComments { get; set; }
    public string AssignmentName { get; set; }
  }
}
