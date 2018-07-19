using Newtonsoft.Json;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// A subgrid of information within a patch result
  /// </summary>
  public class PatchSubgridOriginResult : PatchSubgridResultBase
  {
    private PatchSubgridOriginResult()
    { }

    /// <summary>
    /// Gets the northing patch origin in meters, as a delta.
    /// </summary>
    [JsonProperty(PropertyName = "patchOriginN")]
    public double PatchOriginN { get; private set; }

    /// <summary>
    /// Gets the easting patch origin in meters, as a delta.
    /// </summary>
    [JsonProperty(PropertyName = "patchOriginE")]
    public double PatchOriginE { get; private set; }

    /// <summary>
    /// The grid of cells that make up this subgrid in the patch
    /// </summary>
    [JsonProperty(PropertyName = "cells")]
    protected PatchCellResult[,] Cells { get; set; }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static PatchSubgridOriginResult Create(double patchOriginN, double patchOriginE, bool isNull, float elevationOrigin, PatchCellResult[,] cells)
    {
      return new PatchSubgridOriginResult
      {
        PatchOriginN = patchOriginN,
        PatchOriginE = patchOriginE,
        IsNull = isNull,
        ElevationOrigin = elevationOrigin,
        Cells = cells
      };
    }
  }
}
