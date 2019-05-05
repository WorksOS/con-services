using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  public class MachineDesignsExecutionResult : ContractExecutionResult
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
  }
}
