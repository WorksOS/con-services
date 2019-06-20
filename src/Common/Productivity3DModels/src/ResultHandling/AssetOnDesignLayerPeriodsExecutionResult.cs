using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  public class AssetOnDesignLayerPeriodsExecutionResult : ContractExecutionResult, IMasterDataModel
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

    public List<string> GetIdentifiers()
    {
      return AssetOnDesignLayerPeriods?
        .Where(p => p.AssetUid.HasValue)
        .Select(p => p.AssetUid.Value.ToString())
        .ToList();
    }
  }
}
