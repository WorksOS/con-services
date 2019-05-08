using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
  public class UpdateAssetEvent : IAssetEvent
  {
    public string AssetName { get; set; }
    public long? LegacyAssetId { get; set; }
    public string Model { get; set; }
    public string AssetType { get; set; }
    public int? IconKey { get; set; }
    public string EquipmentVIN { get; set; }
    public int? ModelYear { get; set; }
    public Guid AssetUID { get; set; } // Required Field
    public Guid? OwningCustomerUID { get; set; }
    public DateTime ActionUTC { get; set; } // Required Field
    public DateTime ReceivedUTC { get; set; }
  }
}