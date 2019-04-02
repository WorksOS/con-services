using System;
using System.Collections.Generic;
using System.Text;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.AssetMgmt3D.Abstractions.Models 
{
  public class MatchingAssetsDisplayModel : ContractExecutionResult, IMasterDataModel
  {
    public string AssetUID { get; set; }
    public string MatchingAssetUID { get; set; }
    public string Name { get; set; }
    public string SerialNumber { get; set; }
    public string MatchingSerialNumber { get; set; }
    public string MakeCode { get; set; }
    public string MatchingMakeCode { get; set; }
    public string Model { get; set; }
    public string CustomerName { get; set; }

    public List<string> GetIdentifiers()
    {
      return new List<string>(){ AssetUID };
    }
  }
}
