using VSS.TRex.Exports.Patches.Interfaces;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Exports.Patches
{
  /// <summary>
  /// Base class for representing a sub grid result of a data patch request
  /// CellOriginX, CellOriginY are world space cell indices relative to the world centered
  /// origin of the grid (so they may have negative values);
  /// </summary>
  public class SubgridDataPatchRecordBase : ISubgridDataPatchRecord
  {
    protected const short ELEVATION_OFFSET_FACTOR = 1000;
    protected const double ELEVATION_OFFSET_TOLERANCE = 0.0005;

    /// <summary>
    /// X ordinate of the bottom left corner of the bottom left cell in the sub grid, expressed in cell coordinates (integer)
    /// </summary>
    public int CellOriginX { get; set; }

    /// <summary>
    /// Y ordinate of the bottom left corner of the bottom left cell in the sub grid, expressed in cell coordinates (integer)
    /// </summary>
    public int CellOriginY { get; set; }

    /// <summary>
    /// Notes if the content of all cells in this sub grid are null (and hence not otherwise encoded in the result)
    /// </summary>
    public bool IsNull { get; set; }

    /// <summary>
    /// The elevation of the lowest cell elevation in the elevation sub grid result, expressed in grid coordinates (meters)
    /// </summary>
    public float ElevationOrigin { get; set; }

    /// <summary>
    /// Populates base class information from the supplied client leaf sub grid
    /// </summary>
    /// <param name="subGrid"></param>
    public virtual void Populate(IClientLeafSubGrid subGrid)
    {
      CellOriginX = (int) subGrid.OriginX;
      CellOriginY = (int) subGrid.OriginY;
    }
  }
}
