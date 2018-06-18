using Newtonsoft.Json;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  public abstract class PatchSubgridResultBase
  {
    /// <summary>
    /// If true there are no non-null cells of information retruned by the query for this subgrid.
    /// </summary>
    [JsonProperty(PropertyName = "isNull")]
    protected bool IsNull { get; set; }

    /// <summary>
    /// The elevation origin referenced by all cell elevations in the binary representation of the patch subgrids. Values are expressed in meters.
    /// </summary>
    [JsonProperty(PropertyName = "elevationOrigin")]
    protected float ElevationOrigin { get; set; }

    /// <summary>
    /// The grid of cells that make up this subgrid in the patch
    /// </summary>
    [JsonProperty(PropertyName = "cells")]
    protected PatchCellResult[,] Cells { get; set; }
  }
}
