using System;
using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Models.Models
{
  public class AssetAggregateStatus : IMasterDataModel
  {
    public Guid CustomerUid { get; set; }

    public Guid ProjectUid { get; set; }

    public Guid AssetUid { get; set; } 

    public AssetDetails Details { get; set; }

    public AssetSummary Summary { get; set; }

    public MachineStatus Machine3D { get; set; }

    public List<string> GetIdentifiers()
    {
      return new List<string>()
      {
        CustomerUid.ToString(),
        ProjectUid.ToString(), 
        AssetUid.ToString()
      };
    }
  }
}
