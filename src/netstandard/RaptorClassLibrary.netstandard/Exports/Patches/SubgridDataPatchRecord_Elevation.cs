using System;
using VSS.TRex.Cells;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Utilities;

namespace VSS.TRex.Exports.Patches
{
  /// <summary>
  /// Represents a Subgrid patch result containing elevation data
  /// </summary>
  public class SubgridDataPatchRecord_Elevation : SubgridDataPatchRecordBase
  {
    /// <summary>
    /// The elevation of the lowest cell elevation in the elevation subgrid result, expressed in grid coordinates (meters)
    /// </summary>
    public float ElevationOrigin { get; set; }

    /// <summary>
    /// Contains the elevation values for cells in the grid. This array is the same dimensions as a subgrid
    /// (currently 32x32) and contains positive elevation offsets from the Elevation Origin member, expressed in integer millimeters.
    /// </summary>
    private ushort[,] Data { get; set; }

    /// <summary>
    /// Populate requested elevation information into the subgrid result
    /// </summary>
    /// <param name="subGrid"></param>
    public override void Populate(IClientLeafSubGrid subGrid)
    {
      base.Populate(subGrid);

      IGenericClientLeafSubGrid<float> elevSubGrid = (IGenericClientLeafSubGrid<float>)subGrid;
      float[,] elevations = elevSubGrid.Cells;

      // Determine the minimum non-null elevation in the subgrid
      float MinElevation = CellPass.NullHeight;

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        float value = elevations[x, y];

        if (value != CellPass.NullHeight)
          MinElevation = value;
      });

      if (MinElevation == CellPass.NullHeight)
        return;

      // Set the appropriate values into the result
      IsNull = false;
      ElevationOrigin = MinElevation;
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        float value = elevations[x, y];

        Data[x, y] = (value == CellPass.NullHeight)
          ? ushort.MaxValue
          : (ushort)Math.Floor((value - MinElevation) * 1000 + 0.0005);
      });
    }
  }
}
