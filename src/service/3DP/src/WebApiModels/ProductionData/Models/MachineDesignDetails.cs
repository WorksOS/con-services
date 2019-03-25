using System;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  public class MachineDesignDetails : MachineDetails
  {
    [JsonProperty(PropertyName = "designs")]
    public AssetOnDesignPeriod[] AssetOnDesignPeriods { get; private set; }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static MachineDesignDetails CreateMachineDesignDetails(long assetId, string machineName, bool isJohnDoe, AssetOnDesignPeriod[] assetOnDesignPeriods, Guid? assetUid = null )
    {
      return new MachineDesignDetails
      {
        AssetId = assetId,
        MachineName = machineName,
        IsJohnDoe = isJohnDoe,
        AssetOnDesignPeriods = assetOnDesignPeriods,
        AssetUid = assetUid
      };
    }
  }
}
