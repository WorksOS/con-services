using VSS.TRex.Exports.Patches.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Exports.Patches
{
  /// <summary>
  /// Base class for representing a subgrid result of a data patch request
  /// CellOriginX, CellOriginY are world space cell indices relative to the world centered
  /// origin of the grid (so they may have negative values);
  /// </summary>
  public class SubgridDataPatchRecordBase : ISubgridDataPatchRecord
  {
    /// <summary>
    /// X ordinate of the bottom left corner of the bottom left cell in the subgrid, expressed in cell coordinates (integer)
    /// </summary>
    public int CellOriginX { get; set; }

    /// <summary>
    /// Y ordinate of the bottom left corner of the bottom left cell in the subgrid, expressed in cell coordinates (integer)
    /// </summary>
    public int CellOriginY { get; set; }

    /// <summary>
    /// Notes if the content of all cells in this subgrid are null (and hence not otherwise encoded in the result)
    /// </summary>
    public bool IsNull { get; set; }

    /// <summary>
    /// Populates base class information from the supplied client leaf subgrid
    /// </summary>
    /// <param name="subGrid"></param>
    public virtual void Populate(IClientLeafSubGrid subGrid)
    {
      // todo: reconcile grid origin versus index origin offset basis for cell originX and celloriginY
      CellOriginX = (int) subGrid.OriginX;
      CellOriginY = (int) subGrid.OriginY;
    }
  }
}
