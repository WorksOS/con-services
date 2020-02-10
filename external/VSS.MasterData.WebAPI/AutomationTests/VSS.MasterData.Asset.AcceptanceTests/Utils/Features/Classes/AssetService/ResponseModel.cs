using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Asset.AcceptanceTests.Utils.Features.Classes.AssetService
{
  public class ResponseModel
  {
    public string AssetUID { get; set; }
    public string AssetName { get; set; }
    public long LegacyAssetID { get; set; }
    public string SerialNumber { get; set; }
    public string MakeCode { get; set; }
    public string Model { get; set; }
    public string AssetTypeName { get; set; }
    public object EquipmentVIN { get; set; }
    public int IconKey { get; set; }
    public int ModelYear { get; set; }

  }
}
