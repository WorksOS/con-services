using System;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.VisionLink.Interfaces.Events.MasterData.Models
{
  public class CreateAssetEvent : IAssetEvent
  {
    public string AssetName { get; set; }
    public long LegacyAssetId { get; set; }
    public string SerialNumber { get; set; } //Required Field
    public string MakeCode { get; set; } //Required Field
    public string Model { get; set; }
    public string AssetType { get; set; }
    public int? IconKey { get; set; }
    public string EquipmentVIN { get; set; }
    public int? ModelYear { get; set; }
    public Guid AssetUID { get; set; }
    public Guid? OwningCustomerUID { get; set; }
    public DateTime ActionUTC { get; set; } // Required Field
    public DateTime ReceivedUTC { get; set; }
  }
}
