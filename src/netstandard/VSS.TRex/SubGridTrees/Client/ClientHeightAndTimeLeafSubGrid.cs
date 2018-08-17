using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using VSS.TRex.Filters;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// Client leaf sub grid that tracks height and time for each cell
  /// This class is derived from the height leaf subgrid and decorated with times to allow efficient copy
  /// operations for serialisation and asignation to the height leaf subgrid where the times are removed.
  /// </summary>
  [Serializable]
  public class ClientHeightAndTimeLeafSubGrid : ClientHeightLeafSubGrid
  {
    [NonSerialized] private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

    /// <summary>
    /// Time values for the heights stored in the height and time structure. Times are expressed as the binary DateTime format to promote efficient copying of arrays
    /// </summary>
    public long[,] Times = new long[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

    /// <summary>
    /// An array containing the content of null times for all the cell in the subgrid
    /// </summary>
    public static long[,] nullTimes = new long[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

    /// <summary>
    /// Initialise the null cell values for the client subgrid
    /// </summary>
    static ClientHeightAndTimeLeafSubGrid()
    {
      DateTime min = DateTime.MinValue;
      long nullValue = min.ToBinary();

      SubGridUtilities.SubGridDimensionalIterator((x, y) => nullTimes[x, y] = nullValue);
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

      Times[cellX, cellY] = Context.FilteredValue.FilteredPassData.FilteredPass.Time.ToBinary();
    }

    /// <summary>
    /// Sets all cell heights to null and clears the surveyed surface pass map
    /// </summary>
    public override void Clear()
    {
      base.Clear();

      Array.Copy(nullTimes, 0, Times, 0, SubGridTreeConsts.SubGridTreeCellsPerSubgrid);
    }

    /// <summary>
    /// Write the contents of the Items array using the supplied writer
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="buffer"></param>
    public override void Write(BinaryWriter writer, byte[] buffer)
    {
      base.Write(writer, buffer);

      Buffer.BlockCopy(Times, 0, buffer, 0, SubGridTreeConsts.SubGridTreeCellsPerSubgrid * sizeof(long));
      writer.Write(buffer, 0, SubGridTreeConsts.SubGridTreeCellsPerSubgrid * sizeof(long));
    }

    /// <summary>
    /// Fill the items array by reading the binary representation using the provided reader. 
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="buffer"></param>
    public override void Read(BinaryReader reader, byte[] buffer)
    {
      base.Read(reader, buffer);

      reader.Read(buffer, 0, SubGridTreeConsts.SubGridTreeCellsPerSubgrid * sizeof(long));
      Buffer.BlockCopy(buffer, 0, Times, 0, SubGridTreeConsts.SubGridTreeCellsPerSubgrid * sizeof(long));
    }
  }
}
