using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  public class MachineDesignDetails : MachineDetails
  {
    [JsonProperty(PropertyName = "designs")]
    public List<AssetOnDesignPeriodResult> AssetOnDesignPeriods { get; private set; }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public MachineDesignDetails (long assetId, string machineName, bool isJohnDoe, AssetOnDesignPeriod[] assetOnDesignPeriods, Guid? assetUid = null )
    {
      AssetOnDesignPeriods = assetOnDesignPeriods.Select(d => new AssetOnDesignPeriodResult(d)).ToList();
      AssetId = assetId;
      MachineName = machineName;
      IsJohnDoe = isJohnDoe;
      AssetUid = assetUid;
    }
  }
}
