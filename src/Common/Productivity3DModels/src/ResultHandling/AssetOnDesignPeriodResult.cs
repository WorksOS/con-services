using Newtonsoft.Json;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  /// <summary>
  /// This is a subset of AssetOnDesignPeriods used to return condensed
  /// lists to 3dp caller. Excludes machine-specific data.
  /// </summary>
  public class AssetOnDesignPeriodResult 
  {
    /// <summary>
    ///This design name comes from the tag file.
    ///  So long as the same tag files have been imported into Trex and Raptor,
    ///     the designNames will be the same in both systems and can be used for matching
    /// </summary>
    [JsonProperty(PropertyName = "designName")]
    public string OnMachineDesignName { get; private set; } 

    /// <summary>
    ///The Trex OR Raptor design identifier.
    ///   This is a value unique and internal to each system.
    ///        Eventually this should be phased out, but until Raptor is reworked
    ///   Use designName for matching between systems
    /// This will be obsolete sooon....
    /// </summary>
    [JsonProperty(PropertyName = "designId")]
    public long OnMachineDesignId { get; private set; }


    public AssetOnDesignPeriodResult(AssetOnDesignPeriod fullAssetOnDesignPeriod)
    {
      OnMachineDesignId = fullAssetOnDesignPeriod.OnMachineDesignId;
      OnMachineDesignName = fullAssetOnDesignPeriod.OnMachineDesignName;
    }
  }
}
