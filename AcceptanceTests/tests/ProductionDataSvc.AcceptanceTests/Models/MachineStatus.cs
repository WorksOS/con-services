using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class MachineStatus : MachineDetails
  {
    public string lastKnownDesignName { get; set; }
    public ushort? lastKnownLayerId { get; set; }
    public DateTime? lastKnownTimeStamp { get; set; }
    public double lastKnownLatitude { get; set; }
    public double? lastKnownLongitude { get; set; }
    public double? lastKnownX { get; set; }
    public double? lastKnownY { get; set; }
  }
}
