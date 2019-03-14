using Microsoft.Extensions.Logging;
using System;
using System.IO;
using VSS.TRex.Filters.Models;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// Client leaf sub grid that tracks height and time for each cell
  /// This class is derived from the height leaf sub grid and decorated with times to allow efficient copy
  /// operations for serialisation and assignation to the height leaf sub grid where the times are removed.
  /// </summary>
 public class ClientHeightAndTimeLeafSubGrid : ClientHeightLeafSubGrid
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ClientHeightAndTimeLeafSubGrid>();

    /// <summary>
    /// Time values for the heights stored in the height and time structure. Times are expressed as the DateTime ticks format to promote efficient copying of arrays
    /// </summary>
    public long[,] Times = new long[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

    /// <summary>
    /// An array containing the content of null times for all the cells in the sub grid
    /// </summary>
    public static long[,] nullTimes = new long[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

    /// <summary>
    /// Initialise the null cell values for the client sub grid
    /// </summary>
    static ClientHeightAndTimeLeafSubGrid()
    {
      long nullValue = DateTime.MinValue.Ticks;

      SubGridUtilities.SubGridDimensionalIterator((x, y) => nullTimes[x, y] = nullValue);
    }

    private void Initialise()
    {
      _gridDataType = TRex.Types.GridDataType.HeightAndTime;
    }

    /// <summary>
    /// Constructs a default client sub grid with no owner or parent, at the standard leaf bottom sub grid level,
    /// and using the default cell size and index origin offset
    /// </summary>
    public ClientHeightAndTimeLeafSubGrid() : base()
    {
      Initialise();
    }

    /// <summary>
    /// Constructor. Set the grid to HeightAndTime.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="parent"></param>
    /// <param name="level"></param>
    /// <param name="cellSize"></param>
    /// <param name="indexOriginOffset"></param>
    //public ClientHeightAndTimeLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, uint indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
    //{
    //  Initialise();
    //}

  /// <summary>
  /// Assign filtered height value from a filtered pass to a cell
  /// </summary>
  /// <param name="cellX"></param>
  /// <param name="cellY"></param>
  /// <param name="Context"></param>
  public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext Context)
    {
      base.AssignFilteredValue(cellX, cellY, Context);

      Times[cellX, cellY] = Context.FilteredValue.FilteredPassData.FilteredPass.Time.Ticks;
    }

    /// <summary>
    /// Sets all cell heights to null and clears the surveyed surface pass map
    /// </summary>
    public override void Clear()
    {
      base.Clear();

      Array.Copy(nullTimes, 0, Times, 0, SubGridTreeConsts.SubGridTreeCellsPerSubGrid);
    }

    /// <summary>
    /// Write the contents of the Items array using the supplied writer
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="buffer"></param>
    public override void Write(BinaryWriter writer, byte[] buffer)
    {
      base.Write(writer, buffer);

      Buffer.BlockCopy(Times, 0, buffer, 0, SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(long));
      writer.Write(buffer, 0, SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(long));
    }

    /// <summary>
    /// Fill the items array by reading the binary representation using the provided reader. 
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="buffer"></param>
    public override void Read(BinaryReader reader, byte[] buffer)
    {
      base.Read(reader, buffer);

      reader.Read(buffer, 0, SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(long));
      Buffer.BlockCopy(buffer, 0, Times, 0, SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(long));
    }

    /// <summary>
    /// Assign cell information from a previously cached result held in the general sub grid result cache
    /// using the supplied map to control which cells from the caches sub grid should be copied into this
    /// client leaf sub grid
    /// </summary>
    /// <param name="source"></param>
    public override void AssignFromCachedPreProcessedClientSubgrid(ISubGrid source)
    {
      base.AssignFromCachedPreProcessedClientSubgrid(source);

      SurveyedSurfaceMap.Assign(((ClientHeightAndTimeLeafSubGrid)source).SurveyedSurfaceMap);
    }


    /// <summary>
    /// Assign cell information from a previously cached result held in the general sub grid result cache
    /// using the supplied map to control which cells from the caches sub grid should be copied into this
    /// client leaf sub grid
    /// </summary>
    /// <param name="source"></param>
    /// <param name="map"></param>
    public override void AssignFromCachedPreProcessedClientSubgrid(ISubGrid source, SubGridTreeBitmapSubGridBits map)
    {
      base.AssignFromCachedPreProcessedClientSubgrid(source, map);

      SurveyedSurfaceMap.Assign(((ClientHeightLeafSubGrid)source).SurveyedSurfaceMap);
      SurveyedSurfaceMap.AndWith(map);
    }
  }
}
