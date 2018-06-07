using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using VSS.TRex.Filters;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Utilities;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// Client leaf sub grid that tracks height and time for each cell
  /// </summary>
  [Serializable]
  public class ClientHeightAndTimeLeafSubGrid : GenericClientLeafSubGrid<SubGridCellHeightAndTime>
  {
    [NonSerialized] private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// Surveyed surface map records which cells hold cell pass heights that were derived
    /// from a surveyed surface
    /// </summary>
    public SubGridTreeBitmapSubGridBits SurveyedSurfaceMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

    /// <summary>
    /// Initialise the null cell values for the client subgrid
    /// </summary>
    static ClientHeightAndTimeLeafSubGrid()
    {
      SubGridUtilities.SubGridDimensionalIterator((x, y) => NullCells[x, y].Clear());
    }

    /// <summary>
    /// Constructor. Set the grid to HeightAndTime.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="parent"></param>
    /// <param name="level"></param>
    /// <param name="cellSize"></param>
    /// <param name="indexOriginOffset"></param>
    public ClientHeightAndTimeLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, uint indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
    {
      _gridDataType = TRex.Types.GridDataType.HeightAndTime;
  }

  /// <summary>
  /// Assign filtered height value from a filtered pass to a cell
  /// </summary>
  /// <param name="cellX"></param>
  /// <param name="cellY"></param>
  /// <param name="Context"></param>
  public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext Context)
    {
      base.AssignFilteredValue(cellX, cellY, Context);

      Cells[cellX, cellY].Time = Context.FilteredValue.FilteredPassData.FilteredPass.Time.ToBinary();
      Cells[cellX, cellY].Height = Context.FilteredValue.FilteredPassData.FilteredPass.Height;
    }

    /// <summary>
    /// Sets all cell heights to null and clears the surveyed surface pass map
    /// </summary>
    public override void Clear()
    {
      base.Clear();
    }

    /// <summary>
    /// Write the contents of the Items array using the supplied writer
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="buffer"></param>
    public override void Write(BinaryWriter writer, byte[] buffer)
    {
      base.Write(writer, buffer);

      SubGridUtilities.SubGridDimensionalIterator((x, y) => Cells[x, y].Write(writer));
    }

    /// <summary>
    /// Fill the items array by reading the binary representation using the provided reader. 
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="buffer"></param>
    public override void Read(BinaryReader reader, byte[] buffer)
    {
      base.Read(reader, buffer);

      SubGridUtilities.SubGridDimensionalIterator((x, y) => Cells[x, y].Read(reader));
    }
  }
}
