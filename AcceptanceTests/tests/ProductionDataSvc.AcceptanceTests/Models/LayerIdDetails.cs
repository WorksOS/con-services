using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class LayerIdDetails
  {
    public long AssetId { get; set; }
    public long DesignId { get; set; }
    public long LayerId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
  }
}
