using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.Filters.Models;
using VSS.TRex.IO.Helpers;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// The content of each cell in a height client leaf sub grid. Each cell stores an elevation only.
  /// </summary>
  public class ClientProgressiveHeightsLeafSubGrid : ClientLeafSubGrid, IDisposable
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ClientHeightLeafSubGrid>();

    public List<float[,]> Heights { get; set; }

    private int _numberOfProgressions;

    public int NumberOfProgressions
    {
      get => _numberOfProgressions;
      set
      {
        _numberOfProgressions = value;
        Heights = new List<float[,]>(_numberOfProgressions);
        for (var i = 0; i < _numberOfProgressions; i++)
        {
          Heights[i] = GenericTwoDArrayCacheHelper<float>.Caches().Rent();
        }

        Clear();
      }
    }

    private void Initialise()
    {
      _gridDataType = TRex.Types.GridDataType.Height;
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

    /// <summary>
    /// Assign filtered height value from a filtered pass to a cell
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <param name="Context"></param>
    public override void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext Context)
    {
      throw new NotImplementedException();

//      Cells[cellX, cellY] = Context.FilteredValue.FilteredPassData.FilteredPass.Height;
    }

    /// <summary>
    /// Fills the contents of the client leaf sub grid with a known, non-null test pattern of values
    /// </summary>
    public override void FillWithTestPattern()
    {
      //throw new NotImplementedException();
    }

    /// <summary>
    /// Determines if the leaf content of this sub grid is equal to 'other'
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool LeafContentEquals(IClientLeafSubGrid other)
    {
      return true;
      // throw new NotImplementedException();

//      bool result = true;

//      IGenericClientLeafSubGrid<float> _other = (IGenericClientLeafSubGrid<float>)other;
//      ForEach((x, y) => result &= Cells[x, y] == _other.Cells[x, y]);

//      return result;
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
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <returns></returns>
    public override bool CellHasValue(byte cellX, byte cellY) => true; //Cells[cellX, cellY] != Consts.NullHeight;


    /// <summary>
    /// Sets all cell heights to null and clears the first pass and surveyed surface pass maps
    /// </summary>
    public override void Clear()
    {
      base.Clear();

      for (var i = 0; i < _numberOfProgressions; i++)
      {
        Array.Copy(ClientHeightLeafSubGrid.NullCells, 0, Heights[i], 0, SubGridTreeConsts.SubGridTreeCellsPerSubGrid);
      }
    }

    public override void DumpToLog(string title)
    {
      throw new NotImplementedException();
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

    //  throw new NotImplementedException();
/*
      const int BUFFER_SIZE = SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(float);

      var buffer = GenericArrayPoolCacheHelper<byte>.Caches().Rent(BUFFER_SIZE);
      try
      {
        Buffer.BlockCopy(Cells, 0, buffer, 0, BUFFER_SIZE);
        writer.Write(buffer, 0, BUFFER_SIZE);
      }
      finally
      {
        GenericArrayPoolCacheHelper<byte>.Caches().Return(ref buffer);
      }
      */
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

     // throw new NotImplementedException();
      /*
      const int BUFFER_SIZE = SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(float);

      var buffer = GenericArrayPoolCacheHelper<byte>.Caches().Rent(BUFFER_SIZE);
      try
      {
        reader.Read(buffer, 0, BUFFER_SIZE);
        Buffer.BlockCopy(buffer, 0, Cells, 0, BUFFER_SIZE);
      }
      finally
      {
        GenericArrayPoolCacheHelper<byte>.Caches().Return(ref buffer);
      }
      */
    }

    /// <summary>
    /// Return an indicative size for memory consumption of this class to be used in cache tracking
    /// </summary>
    /// <returns></returns>
    public override int IndicativeSizeInBytes()
    {
      return base.IndicativeSizeInBytes() +
             _numberOfProgressions * SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(float);
    }

    private void ReleaseHeightsRental()
    {
      for (var i = 0; i < _numberOfProgressions; i++)
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
