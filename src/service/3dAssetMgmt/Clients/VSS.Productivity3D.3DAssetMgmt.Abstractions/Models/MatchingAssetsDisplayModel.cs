using System;
using System.Collections.Generic;
using System.Text;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.AssetMgmt3D.Abstractions.Models 
{
  public class MatchingAssetsDisplayModel : ContractExecutionResult, IMasterDataModel
  {
    public MatchingAssetsDisplayModel()
    {
      
    }

    public MatchingAssetsDisplayModel(int code, string message = null) : base(code, message)
    {
      
    }

    public string AssetUID2D { get; set; }
    public string AssetUID3D { get; set; }
    public string Name { get; set; }
    public string SerialNumber2D { get; set; }
    public string SerialNumber3D { get; set; }
    public string MakeCode2D { get; set; }
    public string MakeCode3D { get; set; }
    public string Model { get; set; }
    public string CustomerName { get; set; }

    public List<string> GetIdentifiers()
    {
      return new List<string>(){ AssetUID2D.ToString() };
    }
  }
}
