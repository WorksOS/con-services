using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.AssetMgmt3D.Abstractions
{
  public class AssetMatchDisplayModel : ContractExecutionResult, IMasterDataModel
  {

    public KeyValuePair<BaseAsset,Asset3D> Asset { get; set; }

    public List<string> GetIdentifiers()
    {
      return Asset.Key?.AssetUid !=null ? new List<string>() { Asset.Key.AssetUid.ToString() } : new List<string>();
    }
  }
}
