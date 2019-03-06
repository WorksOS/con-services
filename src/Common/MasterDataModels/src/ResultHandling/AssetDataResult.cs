using System.Collections.Generic;
using System.Linq;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Models.ResultHandling
{
  /*
     todoJeannie
     this assetService endpoint is the only one with legacyAssetId
       which is used in 3dp (temporarily) to map between this and Uid for Raptor/TRex
     https://api-stg.trimble.com/t/trimble.com/vss-alpha-assetservice/1.0/Asset/List?customerUid=ead5f851-44c5-e311-aa77-00505688274d&pageSize=200000
    
   [
    {
        "AssetUID": "069e150c-b606-e311-9e53-0050568824d7",
        "AssetName": null,
        "LegacyAssetID": 930685523290321,
        "SerialNumber": "JMS05073",
        "MakeCode": "CAT",
        "Model": "980H",
        "AssetTypeName": "WHEEL LOADERS",
        "EquipmentVIN": null,
        "IconKey": 27,
        "ModelYear": 2009
    },
    {
        "AssetUID": "bd4333e8-1f21-e311-9ee2-00505688274d",
        "AssetName": null,
        "LegacyAssetID": 1721686110402429,
        "SerialNumber": "LCF00100",
        "MakeCode": "CAT",
        "Model": "CP68B",
        "AssetTypeName": "VIBRATORY SINGLE DRUM PAD",
        "EquipmentVIN": null,
        "IconKey": 88,
        "ModelYear": 0
    }
   ]
   */
  public class AssetDataResult : IMasterDataModel
  {
    public List<AssetData> assets { get; set; }

    public List<string> GetIdentifiers() =>
      assets?
        .SelectMany(c => c.GetIdentifiers())
        .Distinct()
        .ToList() ?? new List<string>();
  }
}
