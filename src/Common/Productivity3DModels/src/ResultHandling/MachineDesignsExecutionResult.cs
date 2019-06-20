using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  public class MachineDesignsExecutionResult : ContractExecutionResult, IMasterDataModel
  {
    /// <summary>
    /// The list of the on-machine designs available for the project.
    /// </summary>
    [JsonProperty(PropertyName = "designs")]
    public List<AssetOnDesignPeriod> AssetOnDesignPeriods { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private MachineDesignsExecutionResult()
    { }

    public MachineDesignsExecutionResult(List<AssetOnDesignPeriod> assetOnDesignPeriods)
    {
      AssetOnDesignPeriods = assetOnDesignPeriods;
    }

    public List<string> GetIdentifiers()
    {
      return AssetOnDesignPeriods?
        .Where(p => p.AssetUid.HasValue)
        .Select(p => p.AssetUid.Value.ToString())
        .ToList();
    }
  }
}
