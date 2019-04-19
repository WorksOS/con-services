using System;
using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.Models.Models
{
  public class AssetSummary : ContractExecutionResult, IMasterDataModel
  {
    public AssetUtilization TotalDay { get; set; }
    public AssetUtilization TotalWeek { get; set; }
    public AssetUtilization TotalMonth { get; set; }
    public AssetUtilization TargetDay { get; set; }
    public AssetUtilization TargetWeek { get; set; }
    public AssetUtilization TargetMonth { get; set; }
    public List<string> GetIdentifiers() => new List<string>();
  }
}
