using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Filters.Models;
using VSS.TRex.IO.Helpers;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// The content of each cell in a height client leaf sub grid. Each cell stores an elevation only.
  /// </summary>
  public class ClientProgressiveHeightsLeafSubGrid : ClientLeafSubGrid, IClientProgressiveHeightsLeafSubGrid, IDisposable
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ClientHeightLeafSubGrid>();

    public static readonly float[,] NullHeights = InitialiseNullHeights();

    private static float[,] InitialiseNullHeights()
    {
      var result = new float[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];
      SubGridUtilities.SubGridDimensionalIterator((x, y) => result[x, y] = Consts.NullHeight);
      return result;
    }

    public const int MaxNumberOfHeightLayers = 1000;

    public List<float[,]> Heights { get; set; }

    private int _numberOfHeightLayers;

    public int NumberOfHeightLayers
    {
      get => _numberOfHeightLayers;
      set
      {
        if (value > MaxNumberOfHeightLayers)
        {
          throw new ArgumentException($"No more than {MaxNumberOfHeightLayers} progressions may be requested at one time");
        }

        _numberOfHeightLayers = value;
        Heights = new List<float[,]>(_numberOfHeightLayers);
        for (var i = 0; i < _numberOfHeightLayers; i++)
        {
          Heights.Add(GenericTwoDArrayCacheHelper<float>.Caches().Rent());
          Array.Copy(NullHeights, 0, Heights[i], 0, SubGridTreeConsts.CellsPerSubGrid);
        }

        Clear();
      }
    }

    private void Initialise()
    {
      _gridDataType = TRex.Types.GridDataType.ProgressiveVolumes;
    }

    /// <summary>
    /// Constructs a default client sub grid with no owner or parent, at the standard leaf bottom sub grid level,
    /// and using the default cell size and index origin offset
    /// </summary>
    public ClientProgressiveHeightsLeafSubGrid() : base(null, null, SubGridTreeConsts.SubGridTreeLevels, SubGridTreeConsts.DefaultCellSize, SubGridTreeConsts.DefaultIndexOriginOffset)
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
    public ClientProgressiveHeightsLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, int indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
    {
      Initialise();
    }

    /// <summary>
    /// Determine if a filtered height is valid (not null)
    /// </summary>
    /// <param name="filteredValue"></param>
    /// <returns></returns>
    public override bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue) => filteredValue.FilteredPass.Height == Consts.NullHeight;
    
    public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext context)
    { }

    /// <summary>
    /// Assign filtered height value from a filtered pass to a cell
    /// </summary>
    /// <param name="heightIndex">The index of the height array in Heights to assign the elevation to</param>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <param name="height"></param>
    public void AssignFilteredValue(int heightIndex, byte cellX, byte cellY, float height)
    {
      Heights[heightIndex][cellX, cellY] = height;
    }

    /// <summary>
    /// Fills the contents of the client leaf sub grid with a known, non-null test pattern of values
    /// </summary>
    public override void FillWithTestPattern()
    {
      NumberOfHeightLayers = 2;

      for (var i = 0; i < _numberOfHeightLayers; i++)
      {
        var ii = i;
        ForEach((x, y) => Heights[ii][x, y] = ii);
      }
    }

    /// <summary>
    /// Determines if the leaf content of this sub grid is equal to 'other'
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool LeafContentEquals(IClientLeafSubGrid other)
    {
      var result = true;

      var otherCopy = (ClientProgressiveHeightsLeafSubGrid)other;
      for (var i = 0; i < _numberOfHeightLayers; i++)
      {
        var ii = i;
        ForEach((x, y) => result &= Heights[ii][x, y] == otherCopy.Heights[ii][x, y]);
      }

      return result;
    }

    public override void AssignFromCachedPreProcessedClientSubGrid(ISubGrid source)
    {
      // Not supported in this client grid
    }

    public override void AssignFromCachedPreProcessedClientSubGrid(ISubGrid source, SubGridTreeBitmapSubGridBits map)
    {
      // Not supported in this client grid
    }

    /// <summary>
    /// Determines if the height at the cell location is null or not.
    /// For the multi-layered progressive height arrays this function simple returns true
    /// delegating the management of this aspect to upper layers.
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <returns></returns>
    public override bool CellHasValue(byte cellX, byte cellY) => true;

    /// <summary>
    /// Sets all cell heights to null and clears the first pass and surveyed surface pass maps
    /// </summary>
    public override void Clear()
    {
      base.Clear();

      for (var i = 0; i < _numberOfHeightLayers; i++)
      {
        Array.Copy(ClientHeightLeafSubGrid.NullCells, 0, Heights[i], 0, SubGridTreeConsts.SubGridTreeCellsPerSubGrid);
      }
    }

    public override void DumpToLog(string title)
    {
      Log.LogDebug(title);
      Log.LogDebug($"Number of layers: {Heights.Count}");

      for (var i = 0; i < _numberOfHeightLayers; i++)
      {
        Log.LogDebug("Height layer {i}");

        var sb = new StringBuilder();

        for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
        {
          sb.Clear();

          for (var k = 0; k < SubGridTreeConsts.SubGridTreeDimension; k++)
          {
            sb.Append(Heights[i][j, k].ToString(CultureInfo.InvariantCulture));
            sb.Append(" ");
          }

          Log.LogDebug($"Row: {j}: {sb}");
        }

        Log.LogDebug("");
      }
    }

    /// <summary>
    /// Write the contents of the Items array using the supplied writer
    /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
    /// Override to implement if needed.
    /// </summary>
    /// <param name="writer"></param>
    public override void Write(BinaryWriter writer)
    {
      base.Write(writer);

      writer.Write(_numberOfHeightLayers);

      const int bufferSize = SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(float);

      var buffer = GenericArrayPoolCacheHelper<byte>.Caches().Rent(bufferSize);
      try
      {
        for (var i = 0; i < _numberOfHeightLayers; i++)
        {
          Buffer.BlockCopy(Heights[i], 0, buffer, 0, bufferSize);
          writer.Write(buffer, 0, bufferSize);
        }
      }
      finally
      {
        GenericArrayPoolCacheHelper<byte>.Caches().Return(ref buffer);
      }
    }

    /// <summary>
    /// Fill the items array by reading the binary representation using the provided reader. 
    /// This is an unimplemented override; a generic BinaryReader based implementation is not provided. 
    /// Override to implement if needed.
    /// </summary>
    /// <param name="reader"></param>
    public override void Read(BinaryReader reader)
    {
      base.Read(reader);

      NumberOfHeightLayers = reader.ReadInt32();

      const int bufferSize = SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(float);

      var buffer = GenericArrayPoolCacheHelper<byte>.Caches().Rent(bufferSize);
      try
      {
        for (var i = 0; i < _numberOfHeightLayers; i++)
        {
          reader.Read(buffer, 0, bufferSize);
          Buffer.BlockCopy(buffer, 0, Heights[i], 0, bufferSize);
        }
      }
      finally
      {
        GenericArrayPoolCacheHelper<byte>.Caches().Return(ref buffer);
      }
    }

    /// <summary>
    /// Return an indicative size for memory consumption of this class to be used in cache tracking
    /// </summary>
    /// <returns></returns>
    public override int IndicativeSizeInBytes()
    {
      return base.IndicativeSizeInBytes() +
             _numberOfHeightLayers * SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(float);
    }

    private void ReleaseHeightsRental()
    {
      for (var i = 0; i < _numberOfHeightLayers; i++)
      {
        var tmp = Heights[i];
        GenericTwoDArrayCacheHelper<float>.Caches().Return(ref tmp);
        Heights[i] = null;
      }
    }

    public void Dispose()
    {
      ReleaseHeightsRental();
    }
  }
}
