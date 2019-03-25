using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  public class AssetOnDesignLayerPeriodsExecutionResult : ContractExecutionResult
  {
    [JsonProperty(PropertyName = "LayerIdDetailsArray")]
    public List<AssetOnDesignLayerPeriod> AssetOnDesignLayerPeriods { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private AssetOnDesignLayerPeriodsExecutionResult()
    {
    }

    public AssetOnDesignLayerPeriodsExecutionResult(List<AssetOnDesignLayerPeriod> assetOnDesignLayerPeriods)
    {
      AssetOnDesignLayerPeriods = assetOnDesignLayerPeriods;
    }

  }
}
