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
      FilterResult baseFilter,
      Guid? referenceDesignUid,
      double? referenceDesignOffset,
      bool positionsAreGrid,
      double startX,
      double startY,
      double endX,
      double endY,
      bool returnAllPassesAndLayers) : base (projectUid, baseFilter, referenceDesignUid, referenceDesignOffset, positionsAreGrid, startX, startY, endX, endY)
    {
      ReturnAllPassesAndLayers = returnAllPassesAndLayers;
    }
  }
}
