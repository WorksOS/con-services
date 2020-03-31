using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.Filters.Models;
using VSS.TRex.IO.Helpers;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SurveyedSurfaces.Interfaces;

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
      long nullValue = 0; //DateTime.MinValue.Ticks;

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
    public override void Write(BinaryWriter writer)
    {
      base.Write(writer);

      const int bufferSize = SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(long);

      var buffer = GenericArrayPoolCacheHelper<byte>.Caches().Rent(bufferSize);
      try
      {
        Buffer.BlockCopy(Times, 0, buffer, 0, bufferSize);
        writer.Write(buffer, 0, bufferSize);
      }
      finally
      {
        GenericArrayPoolCacheHelper<byte>.Caches().Return(ref buffer);
      }
    }

    /// <summary>
    /// Fill the items array by reading the binary representation using the provided reader. 
    /// </summary>
    /// <param name="reader"></param>
    public override void Read(BinaryReader reader)
    {
      base.Read(reader);

      const int bufferSize = SubGridTreeConsts.SubGridTreeCellsPerSubGrid * sizeof(long);

      var buffer = GenericArrayPoolCacheHelper<byte>.Caches().Rent(bufferSize);
      try
      {
        reader.Read(buffer, 0, bufferSize);
        Buffer.BlockCopy(buffer, 0, Times, 0, bufferSize);
      }
      finally
      {
        GenericArrayPoolCacheHelper<byte>.Caches().Return(ref buffer);
      }
    }

    /// <summary>
    /// Assign cell information from a previously cached result held in the general sub grid result cache
    /// using the supplied map to control which cells from the caches sub grid should be copied into this
    /// client leaf sub grid
    /// </summary>
    /// <param name="source"></param>
    public override void AssignFromCachedPreProcessedClientSubGrid(ISubGrid source)
    {
      base.AssignFromCachedPreProcessedClientSubGrid(source);
      Array.Copy(((ClientHeightAndTimeLeafSubGrid)source).Times, Times, SubGridTreeConsts.CellsPerSubGrid);

      //SurveyedSurfaceMap.Assign(((ClientHeightAndTimeLeafSubGrid)source).SurveyedSurfaceMap);
    }


    /// <summary>
    /// Assign cell information from a previously cached result held in the general sub grid result cache
    /// using the supplied map to control which cells from the cached sub grid should be copied into this
    /// client leaf sub grid
    /// </summary>
    /// <param name="source"></param>
    /// <param name="map"></param>
    public override void AssignFromCachedPreProcessedClientSubGrid(ISubGrid source, SubGridTreeBitmapSubGridBits map)
    {
      base.AssignFromCachedPreProcessedClientSubGrid(source, map);

      // Copy all of the times as the nullity (or not) of the elevation is the determinator of a value being present
      Array.Copy(((ClientHeightAndTimeLeafSubGrid)source).Times, Times, SubGridTreeConsts.CellsPerSubGrid);

      //SurveyedSurfaceMap.Assign(((ClientHeightLeafSubGrid)source).SurveyedSurfaceMap);
      //SurveyedSurfaceMap.AndWith(map);
    }

    public override bool UpdateProcessingMapForSurveyedSurfaces(SubGridTreeBitmapSubGridBits processingMap, IList filteredSurveyedSurfaces, bool returnEarliestFilteredCellPass)
    {
      if (!(filteredSurveyedSurfaces is ISurveyedSurfaces surveyedSurfaces))
      {
        return false;
      }

      processingMap.Assign(FilterMap);

      // If we're interested in a particular cell, but we don't have any surveyed surfaces later (or earlier)
      // than the cell production data pass time (depending on PassFilter.ReturnEarliestFilteredCellPass)
      // then there's no point in asking the Design Profiler service for an elevation

      processingMap.ForEachSetBit((x, y) =>
      {
        if (Cells[x, y] != Consts.NullHeight &&
            !(returnEarliestFilteredCellPass ? surveyedSurfaces.HasSurfaceEarlierThan(Times[x, y]) : surveyedSurfaces.HasSurfaceLaterThan(Times[x, y])))
          processingMap.ClearBit(x, y);
      });

      return true;
    }

    public bool PerformHeightAnnotation(SubGridTreeBitmapSubGridBits processingMap, IList filteredSurveyedSurfaces, bool returnEarliestFilteredCellPass,
      IClientLeafSubGrid surfaceElevationsSource, Func<int, int, float, bool> elevationRangeFilterLambda)
    {
      if (!(surfaceElevationsSource is ClientHeightAndTimeLeafSubGrid surfaceElevations))
      {
        return false;
      }

      // For all cells we wanted to request a surveyed surface elevation for,
      // update the cell elevation if a non null surveyed surface of appropriate time was computed
      // Note: The surveyed surface will return all cells in the requested sub grid, not just the ones indicated in the processing map
      // IE: It is unsafe to test for null top indicate not-filtered, use the processing map iterators to cover only those cells required
      processingMap.ForEachSetBit((x, y) =>
      {
        var surveyedSurfaceCellHeight = surfaceElevations.Cells[x, y];

        if (surveyedSurfaceCellHeight == Consts.NullHeight)
        {
          return;
        }

        // If we got back a surveyed surface elevation...
        var surveyedSurfaceCellTime = surfaceElevations.Times[x, y];
        var prodHeight = Cells[x, y];
        var prodTime = Times[x, y];

        // Determine if the elevation from the surveyed surface data is required based on the production data elevation being null, and
        // the relative age of the measured surveyed surface elevation compared with a non-null production data height
        if (!(prodHeight == Consts.NullHeight || (returnEarliestFilteredCellPass ? surveyedSurfaceCellTime < prodTime : surveyedSurfaceCellTime > prodTime)))
        {
          // We didn't get a surveyed surface elevation, so clear the bit in the processing map to indicate there is no surveyed surface information present for it
          processingMap.ClearBit(x, y);
          return;
        }

        // Check if there is an elevation range filter in effect and whether the surveyed surface elevation data matches it
        if (elevationRangeFilterLambda != null)
        {
          if (!(elevationRangeFilterLambda(x, y, surveyedSurfaceCellHeight)))
          {
            // We didn't get a surveyed surface elevation, so clear the bit in the processing map to indicate there is no surveyed surface information present for it
            processingMap.ClearBit(x, y);
            return;
          }
        }

        Cells[x, y] = surveyedSurfaceCellHeight;
        Times[x, y] = surveyedSurfaceCellTime;
      });

      //        if (ClientGrid_is_TICClientSubGridTreeLeaf_HeightAndTime)
      //          ClientGridAsHeightAndTime.SurveyedSurfaceMap.Assign(ProcessingMap);
      return true;
    }
  }
}
