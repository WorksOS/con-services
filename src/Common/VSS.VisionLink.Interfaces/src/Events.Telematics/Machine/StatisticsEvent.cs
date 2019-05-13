using VSS.VisionLink.Interfaces.Events.Telematics.Context;
using VSS.VisionLink.Interfaces.Events.Telematics.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.Telematics.Machine
{
  public class StatisticsEvent : IAnnotatedEvent
  {
    public AssetDetail Asset { get; set; }
    public DeviceDetail Device { get; set; }
    public OwnerDetail Owner { get; set; }
    public TimestampDetail Timestamp { get; set; }
    public TracingMetadataDetail TracingMetadata { get; set; }

    public double Average { get; set; }
    public int EcmSourceAddress { get; set; }
    public double Maximum { get; set; }
    public double Minimum { get; set; }
    public int Pgn { get; set; }
    public double? RuntimeHours { get; set; }
    public int Spn { get; set; }
    public double StandardDeviation { get; set; }
    public string UnitType { get; set; }
  }
}
