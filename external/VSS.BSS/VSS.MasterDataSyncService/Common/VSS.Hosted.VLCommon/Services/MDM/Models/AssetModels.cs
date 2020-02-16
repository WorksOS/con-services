using System;

namespace VSS.Hosted.VLCommon.Services.MDM.Models
{
  public class CreateAssetEvent 
  {
    public string AssetName { get; set; }
      
    public long LegacyAssetID { get; set; }

    public string SerialNumber { get; set; }

    public string MakeCode { get; set; }

    public string Model { get; set; }

    public string AssetType { get; set; } // Product Family

    public int? IconKey { get; set; }

    public string EquipmentVIN { get; set; }

    public int? ModelYear { get; set; }

    public Guid AssetUID { get; set; }

    public DateTime ActionUTC { get; set; }

    public DateTime ReceivedUTC { get; set; }
  }

  public class UpdateAssetEvent
  {
    public string AssetName { get; set; }

    public long? LegacyAssetID { get; set; }

    public string Model { get; set; }
    public string AssetType { get; set; } //Product Family

    public int? IconKey { get; set; }

    public string EquipmentVIN { get; set; }

    public int? ModelYear { get; set; }

    public Guid AssetUID { get; set; }

    public DateTime ActionUTC { get; set; }

    public DateTime ReceivedUTC { get; set; }

  }
}
