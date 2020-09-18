using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
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
    public static readonly float[,] NullHeights = InitialiseNullHeights();
    public static readonly long[,] NullTimes = new long[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];

    private static float[,] InitialiseNullHeights()
    {
      var result = new float[SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension];
      SubGridUtilities.SubGridDimensionalIterator((x, y) => result[x, y] = Consts.NullHeight);
      return result;
    }

    public const int MAX_NUMBER_OF_HEIGHT_LAYERS = 1000;

    public List<(float[,] Heights, long[,] Times, DateTime Date)> Layers { get; set; }

    private int _numberOfHeightLayers;

    public int NumberOfHeightLayers
    {
      get => _numberOfHeightLayers;
      set
      {
        if (value > MAX_NUMBER_OF_HEIGHT_LAYERS)
        {
          throw new ArgumentException($"No more than {MAX_NUMBER_OF_HEIGHT_LAYERS} progressions may be requested at one time");
        }

        _numberOfHeightLayers = value;
        Layers = new List<(float[,] Heights, long[,] Times, DateTime Date)>(_numberOfHeightLayers);
        for (var i = 0; i < _numberOfHeightLayers; i++)
        {
          var layer = (GenericTwoDArrayCacheHelper<float>.Caches().Rent(), GenericTwoDArrayCacheHelper<long>.Caches().Rent(), DateTime.MinValue);
          Layers.Add(layer);
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
    public ClientProgressiveHeightsLeafSubGrid(ISubGridTree owner, ISubGrid parent, byte level, double cellSize, int indexOriginOffset) : base(owner, parent, level, cellSize, indexOriginOffset)
    {
      Initialise();
    }

    /// <summary>
    /// Determine if a filtered height is valid (not null)
    /// </summary>
    // ReSharper disable once CompareOfFloatsByEqualityOperator
    public override bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue) => filteredValue.FilteredPass.Height == Consts.NullHeight;
    
    public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext context)
    { }

    /// <summary>
    /// Assign filtered height value from a filtered pass to a cell
    /// </summary>
    /// <param name="heightIndex">The index of the height array in Heights to assign the elevation to</param>
    public void AssignFilteredValue(int heightIndex, byte cellX, byte cellY, float height, long time)
    {
      Layers[heightIndex].Heights[cellX, cellY] = height;
      Layers[heightIndex].Times[cellX, cellY] = time;
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
        ForEach((x, y) => Layers[ii].Heights[x, y] = ii);
      }
    }

    /// <summary>
    /// Determines if the leaf content of this sub grid is equal to 'other'
    /// </summary>
    public override bool LeafContentEquals(IClientLeafSubGrid other)
    {
      var result = true;

      var otherCopy = (ClientProgressiveHeightsLeafSubGrid)other;
      for (var i = 0; i < _numberOfHeightLayers; i++)
      {
        var ii = i;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        ForEach((x, y) => result &= Layers[ii].Heights[x, y] == otherCopy.Layers[ii].Heights[x, y]);
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
    public override bool CellHasValue(byte cellX, byte cellY) => true;

    /// <summary>
    /// Sets all cell heights to null and clears the first pass and surveyed surface pass maps
    /// </summary>
    public override void Clear()
    {
      base.Clear();

      for (var i = 0; i < _numberOfHeightLayers; i++)
      {
        Array.Copy(ClientProgressiveHeightsLeafSubGrid.NullHeights, 0, Layers[i].Heights, 0, SubGridTreeConsts.SubGridTreeCellsPerSubGrid);
        Array.Copy(ClientProgressiveHeightsLeafSubGrid.NullTimes, 0, Layers[i].Times, 0, SubGridTreeConsts.SubGridTreeCellsPerSubGrid);
      }
    }

    public override void DumpToLog(ILogger log, string title)
    {
      log.LogDebug(title);
      log.LogDebug($"Number of layers: {Layers.Count}");

      for (var i = 0; i < _numberOfHeightLayers; i++)
      {
        log.LogDebug("Height layer {i}");

        var sb = new StringBuilder();

        for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
        {
          sb.Clear();

          for (var k = 0; k < SubGridTreeConsts.SubGridTreeDimension; k++)
          {
            sb.Append(Layers[i].Heights[j, k].ToString(CultureInfo.InvariantCulture));
            sb.Append(" ");
          }

          log.LogDebug($"Row: {j}: {sb}");
        }

        log.LogDebug("");
      }
    }

    /// <summary>
    /// Write the contents of the Items array using the supplied writer
    /// This is an unimplemented override; a generic BinaryReader based implementation is not provided.
    /// Override to implement if needed.
    /// </summary>
    public override void Write(BinaryWriter writer)
    {
      base.Write(writer);

      writer.Write(_numberOfHeightLayers);

      const int BUFFER_SIZE = SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(float);

      var buffer = GenericArrayPoolCacheHelper<byte>.Caches().Rent(BUFFER_SIZE);
      try
      {
        for (var i = 0; i < _numberOfHeightLayers; i++)
        {
          Buffer.BlockCopy(Layers[i].Heights, 0, buffer, 0, BUFFER_SIZE);
          writer.Write(buffer, 0, BUFFER_SIZE);
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
    public override void Read(BinaryReader reader)
    {
      base.Read(reader);

      NumberOfHeightLayers = reader.ReadInt32();

      const int BUFFER_SIZE = SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(float);

      var buffer = GenericArrayPoolCacheHelper<byte>.Caches().Rent(BUFFER_SIZE);
      try
      {
        for (var i = 0; i < _numberOfHeightLayers; i++)
        {
          reader.Read(buffer, 0, BUFFER_SIZE);
          Buffer.BlockCopy(buffer, 0, Layers[i].Heights, 0, BUFFER_SIZE);
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
    public override int IndicativeSizeInBytes()
    {
      return base.IndicativeSizeInBytes() +
             _numberOfHeightLayers * SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(float);
    }

    private void ReleaseHeightsRental()
    {
      for (var i = 0; i < _numberOfHeightLayers; i++)
      {
        var tmp = Layers[i].Heights;
        GenericTwoDArrayCacheHelper<float>.Caches().Return(ref tmp);
        var tmp2 = Layers[i].Times;
        GenericTwoDArrayCacheHelper<long>.Caches().Return(ref tmp2);
      }

      Layers = null;
    }

    public void Dispose()
    {
      ReleaseHeightsRental();
    }
  }
}
