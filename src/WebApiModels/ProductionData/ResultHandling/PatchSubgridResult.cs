using Newtonsoft.Json;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// A subgrid of information within a patch result
  /// </summary>
  public class PatchSubgridResult
  {
    private PatchSubgridResult()
    { }

    /// <summary>
    /// The X ordinate location of the cell in the catersian coorindate space of cell address indexes centered on the catersian grid coordinate system of the project
    /// NOTE: As of the time of writing the cell origin positions returned need to be adjusted by the subgridtree IndexOriginOffset value - this should be taken into account when the subgrid is written into the patch.
    /// </summary>
    [JsonProperty(PropertyName = "cellOriginX")]
    public int CellOriginX { get; private set; }

    /// <summary>
    /// The X ordinate location of the cell in the catersian coorindate space of cell address indexes centered on the catersian grid coordinate system of the project
    /// NOTE: As of the time of writing the cell origin positions returned need to be adjusted by the subgridtree IndexOriginOffset value - this should be taken into account when the subgrid is written into the patch.
    /// </summary>
    [JsonProperty(PropertyName = "cellOriginY")]
    public int CellOriginY { get; private set; }

    /// <summary>
    /// If true there are no non-null cells of information retruned by the query for this subgrid.
    /// </summary>
    [JsonProperty(PropertyName = "isNull")]
    public bool IsNull { get; private set; }

    /// <summary>
    /// The elevation origin referenced by all cell elevations in the binary representation of the patch subgrids. Values are expressed in meters.
    /// </summary>
    [JsonProperty(PropertyName = "elevationOrigin")]
    public float ElevationOrigin { get; private set; }

    /// <summary>
    /// The grid of cells that make up this subgrid in the patch
    /// </summary>
    [JsonProperty(PropertyName = "cells")]
    public PatchCellResult[,] Cells { get; private set; }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static PatchSubgridResult CreatePatchSubgridResult(int cellOriginX, int cellOriginY, bool isNull, float elevationOrigin, PatchCellResult[,] cells)
    {
      return new PatchSubgridResult
      {
        CellOriginX = cellOriginX,
        CellOriginY = cellOriginY,
        IsNull = isNull,
        ElevationOrigin = elevationOrigin,
        Cells = cells
      };
    }
  }
}
