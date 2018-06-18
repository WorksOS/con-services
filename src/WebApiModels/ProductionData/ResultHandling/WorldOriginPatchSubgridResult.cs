using Newtonsoft.Json;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// A subgrid of information within a patch result
  /// </summary>
  public class WorldOriginPatchSubgridResult : PatchSubgridResultBase
  {
    private WorldOriginPatchSubgridResult()
    { }

    /// <summary>
    /// The X ordinate location of the cell in the catersian coorindate space of cell address indexes centered on the catersian grid coordinate system of the project
    /// NOTE: As of the time of writing the cell origin positions returned need to be adjusted by the subgridtree IndexOriginOffset value - this should be taken into account when the subgrid is written into the patch.
    /// </summary>
    [JsonProperty(PropertyName = "worldOriginX")]
    public double WorldOriginX { get; private set; }

    /// <summary>
    /// The X ordinate location of the cell in the catersian coorindate space of cell address indexes centered on the catersian grid coordinate system of the project
    /// NOTE: As of the time of writing the cell origin positions returned need to be adjusted by the subgridtree IndexOriginOffset value - this should be taken into account when the subgrid is written into the patch.
    /// </summary>
    [JsonProperty(PropertyName = "worldOriginY")]
    public double WorldOriginY { get; private set; }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static WorldOriginPatchSubgridResult Create(double worldOriginX, double worldOriginY, bool isNull, float elevationOrigin, PatchCellResult[,] cells)
    {
      return new WorldOriginPatchSubgridResult
      {
        WorldOriginX = worldOriginX,
        WorldOriginY = worldOriginY,
        IsNull = isNull,
        ElevationOrigin = elevationOrigin,
        Cells = cells
      };
    }
  }
}
