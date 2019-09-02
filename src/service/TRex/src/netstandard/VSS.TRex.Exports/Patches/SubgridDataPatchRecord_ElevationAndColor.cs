using System;
using System.Drawing;
using VSS.TRex.Common;
using VSS.TRex.Common.Records;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types.CellPasses;

namespace VSS.TRex.Exports.Patches
{
  /// <summary>
  /// Represents a sub grid patch result containing elevation data
  /// </summary>
  public class SubgridDataPatchRecord_ElevationAndColor : SubgridDataPatchRecordBase
  {
    /// <summary>
    /// Contains the elevation and color values for cells in the grid. This array is the same dimensions as a sub grid
    /// (currently 32x32) and contains positive elevation offsets from the ElevationOrigin member, expressed in integer 
    /// millimeters as well as colors.
    /// </summary>
    public PatchColorsRecord[,] Data { get; set; }

    /// <summary>
    /// Populate requested elevation information into the sub grid result
    /// </summary>
    /// <param name="subGrid"></param>
    public override void Populate(IClientLeafSubGrid subGrid)
    {
      base.Populate(subGrid);

      var elevSubGrid = (ClientHeightLeafSubGrid)subGrid;
      var elevations = elevSubGrid.Cells;
      IsNull = true;

      if (elevSubGrid.Cells != null)
      {
        var minElevation = CellPassConsts.NullHeight;

        SubGridUtilities.SubGridDimensionalIterator((x, y) =>
        {
          var value = elevations[x, y];

          if (Math.Abs(value - CellPassConsts.NullHeight) > Consts.TOLERANCE_DIMENSION && value < minElevation)
            minElevation = value;
        });

        if (Math.Abs(minElevation - CellPassConsts.NullHeight) < Consts.TOLERANCE_DIMENSION)
          return;

        // Set the appropriate values into the result
        Data = new PatchColorsRecord[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];
        IsNull = false;

        ElevationOrigin = minElevation;

        SubGridUtilities.SubGridDimensionalIterator((x, y) =>
        {
          var value = elevations[x, y];

          if (Math.Abs(value - CellPassConsts.NullHeight) > Consts.TOLERANCE_DIMENSION)
            Data[x, y] = new PatchColorsRecord((uint)Math.Floor((value - minElevation) * ELEVATION_OFFSET_FACTOR + ELEVATION_OFFSET_TOLERANCE), Color.Empty);
          else
          {
            Data[x, y] = new PatchColorsRecord(uint.MaxValue, Color.Empty);
          }
        });
      }
    }
  }
}
