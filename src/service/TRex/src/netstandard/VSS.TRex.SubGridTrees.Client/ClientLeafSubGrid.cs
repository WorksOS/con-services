using System;
using System.Collections;
using System.IO;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Utilities.Interfaces;
using VSS.TRex.Events.Models;
using VSS.TRex.Filters.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// ClientLeafSubGrid is a local base class for sub grid tree
  /// leaf sub grid derivatives. This class defines support for assigning filtered
  /// values to cells in the grid, and also adds a cache map to it. The cache
  /// map records which cells in the sub grid contain information that has been
  /// retrieved from the server.
  /// </summary>
  public abstract class ClientLeafSubGrid : SubGrid, IClientLeafSubGrid, IBinaryReaderWriter
  {
    /// <summary>
    /// Enumeration indicating type of grid data held in this client leaf sub grid
    /// </summary>
    protected GridDataType _gridDataType;

    /// <summary>
    /// Enumeration indicating type of grid data held in this client leaf sub grid
    /// </summary>
    public GridDataType GridDataType => _gridDataType;

    private double _cellSize;

    /// <summary>
    /// CellSize is a copy of the cell size from the parent sub grid. It is replicated here
    /// to remove SubGridTree binding in other processing contexts
    /// </summary>
    public double CellSize
    {
      get => _cellSize;
      set => _cellSize = value;
    }

    private int _indexOriginOffset;

    /// <summary>
    /// IndexOriginOffset is a copy of the IndexOriginOffset from the parent sub grid. It is replicated here
    ///to remove SubGridTree binding in other processing contexts
    /// </summary>
    public int IndexOriginOffset
    {
      get => _indexOriginOffset;
      set => _indexOriginOffset = value;
    }

    /// <summary>
    /// Is data extraction limited to the top identified layer of materials in each cell
    /// </summary>
    public bool TopLayerOnly { get; set; }

    public abstract void FillWithTestPattern();

    public abstract bool LeafContentEquals(IClientLeafSubGrid other);

    /// <summary>
    /// The requested display mode driving the request of these sub grids of data
    /// </summary>
    protected DisplayMode ProfileDisplayMode { get; set; }

    /// <summary>
    /// A map of flags indicating which grid data types are supported by the intermediary sub grid result cache
    /// </summary>
    public static readonly bool[] SupportsAssignationFromCachedPreProcessedClientSubGrid = // GridDataType
    {
      false, // All = $00000000;
      true, // CCV = $00000001;
      true, // Height = $00000002;
      false, // Latency = $00000003;
      true, // PassCount = $00000004;
      false, // Frequency = $00000005;
      false, // Amplitude = $00000006;
      false, // Moisture = $00000007;
      true, // Temperature = $00000008;
      false, // RMV = $00000009;
      true, // CCVPercent = $0000000B;
      false, // GPSMode = $0000000A;
      true, // SimpleVolumeOverlay = $0000000C;
      true, // HeightAndTime = $0000000D;

      // Note: Composite heights are used for profiling only 
      // These sub grids are very large and profiling heavily optimizes for specific cells so
      // don't cache these sub grids
      false, // CompositeHeights = $0000000E;
      true, // MDP = $0000000F;
      true, // MDPPercent = $00000010;
      false, // CellProfile = $00000011;
      false, // CellPasses = $00000012;
      true, // MachineSpeed = $00000013;
      true, // CCVPercentChange = $00000014;
      true, // MachineSpeedTarget = $00000015;
      true, // CCVPercentChangeIgnoredTopNullValue = $0000016
      true, // CCA = $0000017
      true, // CCAPercent = $0000018
      true, // Temperature details = $0000019
      true, // CutFill = 0x0000001A,
      true, // DesignHeight = 0x0000001B,

      // SurveyedSurfaceHeightAndTime is distinguished from HeightAndTime in that only surveyed surfaces are
      // used to construct this data. Differentiating the grid types allows coherent caching in a single spatial
      // general sub grid result cache along with HeightAndTime results that are derived from production data
      // and SurveyedSurfaceHeightAndTime results
      true, // SurveyedSurfaceHeightAndTime = 0x0000001C

      false // Progressing volumes = = 0x0000001D
    };

    /// <summary>
    /// Existence map of where we know Prod Data exists 
    /// </summary>
    public SubGridTreeBitmapSubGridBits ProdDataMap { get; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

    /// <summary>
    /// Existence map of cells matching current filter settings
    /// </summary>
    public SubGridTreeBitmapSubGridBits FilterMap { get; } = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

    /// <summary>
    /// World extent of the client leaf sub grid map of cells
    /// </summary>
    /// <returns></returns>
    public BoundingWorldExtent3D WorldExtents()
    {
      var wOx = (originX - _indexOriginOffset) * _cellSize;
      var wOy = (originY - _indexOriginOffset) * _cellSize;

      return new BoundingWorldExtent3D(wOx, wOy, wOx + SubGridTreeConsts.SubGridTreeDimension * _cellSize, wOy + SubGridTreeConsts.SubGridTreeDimension * _cellSize);
    }

    /// <summary>
    /// Constructor the the base client sub grid. This decorates the standard (owner, parent, level)
    /// constructor from the base with the cell size and index origin offset parameters from the sub grid tree
    /// this leaf is derived from.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="parent"></param>
    /// <param name="level"></param>
    /// <param name="cellSize"></param>
    /// <param name="indexOriginOffset"></param>
    protected ClientLeafSubGrid(ISubGridTree owner,
      ISubGrid parent,
      byte level,
      double cellSize,
      int indexOriginOffset) : base(owner, parent, level)
    {
      _cellSize = cellSize;
      _indexOriginOffset = indexOriginOffset;

      _gridDataType = GridDataType.All; // Default to 'all', descendant specialized classes will set appropriately

      TopLayerOnly = false;
      ProfileDisplayMode = DisplayMode.Height;
    }

    /// <summary>
    /// Assign the result of filtering a cell (based on filtering and other criteria) into a cell in this client leaf sub grid
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    /// <param name="context"></param>
    public abstract void AssignFilteredValue(byte cellX, byte cellY, FilteredValueAssignmentContext context);

    /// <summary>
    /// Determine if the value proposed for assignation to a cell in this client leaf sub grid is null with respect
    /// to the nullability criteria of that client leaf sub grid
    /// </summary>
    /// <param name="filteredValue"></param>
    /// <returns></returns>
    public abstract bool AssignableFilteredValueIsNull(ref FilteredPassData filteredValue);

    /// <summary>
    /// The set of population control flags this client wants enabled in the course of servicing requests
    /// </summary>
    public PopulationControlFlags EventPopulationFlags { get; set; } = PopulationControlFlags.None;

    public virtual bool WantsLiftProcessingResults() => false;

    /// <summary>
    /// Calculate the world origin coordinate location for this client leaf sub grid.
    /// This uses the local cell size and index origin offset information to perform the 
    /// calculation locally without the need for a reference sub grid tree.
    /// </summary>
    /// <param name="worldOriginX"></param>
    /// <param name="worldOriginY"></param>
    public override void CalculateWorldOrigin(out double worldOriginX,
      out double worldOriginY)
    {
      worldOriginX = (originX - _indexOriginOffset) * _cellSize;
      worldOriginY = (originY - _indexOriginOffset) * _cellSize;
    }

    /// <summary>
    /// Assign cell information from a previously cached result held in the general sub grid result cache
    /// using the supplied map to control which cells from the caches sub grid should be copied into this
    /// client leaf sub grid
    /// </summary>
    /// <param name="source"></param>
    public abstract void AssignFromCachedPreProcessedClientSubGrid(ISubGrid source);

    /// <summary>
    /// Assign cell information from a previously cached result held in the general sub grid result cache
    /// using the supplied map to control which cells from the caches sub grid should be copied into this
    /// client leaf sub grid
    /// </summary>
    /// <param name="source"></param>
    /// <param name="map"></param>
    public abstract void AssignFromCachedPreProcessedClientSubGrid(ISubGrid source, SubGridTreeBitmapSubGridBits map);

    /// <summary>
    /// Assigns the state of one client leaf sub grid to this client leaf sub grid
    /// Note: The cell values are explicitly NOT copied in this operation
    /// </summary>
    /// <param name="source"></param>
    public void Assign(IClientLeafSubGrid source)
    {
      level = source.Level;
      originX = source.OriginX;
      originY = source.OriginY;

      // Grid data type is never assigned from one client grid to another...
      //GridDataType = source.GridDataType;

      _cellSize = source.CellSize;
      _indexOriginOffset = source.IndexOriginOffset;
      ProdDataMap.Assign(source.ProdDataMap);
      FilterMap.Assign(source.FilterMap);
    }

    /// <summary>
    /// Dumps the contents of this client leaf sub grid into the log in a human readable form
    /// </summary>
    /// <param name="title"></param>
    public abstract void DumpToLog(string title);

    /// <summary>
    /// Write the contents of the Items array using the supplied writer
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="buffer"></param>
    public override void Write(BinaryWriter writer)
    {
      base.Write(writer);

      writer.Write((int) GridDataType);
      writer.Write(_cellSize);
      writer.Write(_indexOriginOffset);

      ProdDataMap.Write(writer);
      FilterMap.Write(writer);
    }

    /// <summary>
    /// Fill the items array by reading the binary representation using the provided reader. 
    /// </summary>
    /// <param name="reader"></param>
    public override void Read(BinaryReader reader)
    {
      base.Read(reader);

      if ((GridDataType) reader.ReadInt32() != GridDataType)
        throw new TRexSubGridIOException("GridDataType in stream does not match GridDataType of local sub grid instance");

      _cellSize = reader.ReadDouble();
      _indexOriginOffset = reader.ReadInt32();

      ProdDataMap.Read(reader);
      FilterMap.Read(reader);
    }

    /// <summary>
    /// Return an indicative size for memory consumption of this class to be used in cache tracking
    /// </summary>
    /// <returns></returns>

    public virtual int IndicativeSizeInBytes()
    {
      var filterMapSize = FilterMap?.IndicativeSizeInBytes() ?? 0;
      var prodDataMapSize = ProdDataMap?.IndicativeSizeInBytes() ?? 0;

      return filterMapSize + prodDataMapSize;
    }

    /// <summary>
    /// Facades the OriginX property of this sub grid for use in the spatial caching implementation
    /// </summary>
    public int CacheOriginX => originX;

    /// <summary>
    /// Facades the OriginY property of this sub grid for use in the spatial caching implementation
    /// </summary>
    public int CacheOriginY => originY;

    public virtual void DumpToLog()
    {
    }

    public virtual bool UpdateProcessingMapForSurveyedSurfaces(SubGridTreeBitmapSubGridBits processingMap, IList filteredSurveyedSurfaces, bool returnEarliestFilteredCellPass)
    {
      return false;
    }

    public virtual bool PerformHeightAnnotation(SubGridTreeBitmapSubGridBits processingMap, IList filteredSurveyedSurfaces, bool returnEarliestFilteredCellPass,
      IClientLeafSubGrid surfaceElevationsSource, Func<int, int, float, bool> elevationRangeFilter)
    {
      return false;
    }
  }
}
