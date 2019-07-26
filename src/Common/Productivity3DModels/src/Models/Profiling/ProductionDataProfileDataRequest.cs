using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models.Profiling
{
  /// <summary>
  /// The representation of a production data profile request
  /// </summary>
  public class ProductionDataProfileDataRequest : BaseProfileDataRequest
  {
    /// <summary>
    /// The filter to be used.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public FilterResult Filter { get; private set; }
    /// <summary>
    /// The flag indicates whether to return all passes and layers.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public bool ReturnAllPassesAndLayers { get; private set; }

    /// <summary>
    /// Default public constructor.
    /// </summary>
    public ProductionDataProfileDataRequest()
    { }

    public ProductionDataProfileDataRequest(
      Guid projectUid,
      FilterResult filter,
      bool returnAllPassesAndLayers,
      Guid? referenceDesignUid,
      double? referenceDesignOffset,
      bool positionsAreGrid,
      double startX,
      double startY,
      double endX,
      double endY,
      OverridingTargets overrides,
      LiftSettings liftSettings)  

      : base (projectUid, referenceDesignUid, referenceDesignOffset, positionsAreGrid, 
              startX, startY, endX, endY, overrides, liftSettings)
    {
      Filter = filter;
      ReturnAllPassesAndLayers = returnAllPassesAndLayers;
    }

    /// <summary>
    /// Validates all properties.
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      Filter?.Validate();
    }
  }
}
