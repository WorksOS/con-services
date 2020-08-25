using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;

namespace VSS.TRex.Volumes
{
  /// <summary>
  /// Defines an aggregator that summaries simple volumes information for sub grids
  /// </summary>
  public class SimpleVolumesCalculationsAggregator : ISubGridRequestsAggregator, IAggregateWith<SimpleVolumesCalculationsAggregator>, IDisposable
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<SimpleVolumesCalculationsAggregator>();

    /// <summary>
    /// Defines a sub grid full of null values to run through the volumes engine in cases when 
    /// one of the two sub grids is not available to allow for correctly tracking of statistics
    /// </summary>
    private static readonly ClientHeightLeafSubGrid _nullHeightSubGrid = new ClientHeightLeafSubGrid(null, null, 0, 0, 0);

    private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

    // CoverageMap maps the area of cells that we have considered and successfully
    // computed volume information from
    public readonly SubGridTreeBitMask CoverageMap = new SubGridTreeBitMask();

    // NoChangeMap maps the area of cells that we have considered and found to have
    // had no height change between to two surfaces considered
    // SubGridTreeBitMask FNoChangeMap = new SubGridTreeBitMask();

    /// <summary>
    /// The design being used to compare heights derived from production data against to calculate per-cell volumes.
    /// Also contains the offset for a reference surface.
    /// </summary>
    public IDesignWrapper ActiveDesign { get; set; }

    // References necessary for correct summarization of aggregated state
    public ILiftParameters LiftParams { get; set; } = new LiftParameters();

    public Guid SiteModelID { get; set; } = Guid.Empty;

    // The sum of the aggregated summarized information relating to volumes summary based reports

    // CellsUsed records how many cells were used in the volume calculation
    public long CellsUsed { get; set; }
    public long CellsUsedCut { get; set; }
    public long CellsUsedFill { get; set; }

    // FCellsScanned records the total number of cells that were considered by
    // the engine. This includes cells outside of reference design fence boundaries
    // and cells where both base and top values may have been null.
    public long CellsScanned { get; set; }

    // FCellsDiscarded records how many cells were discarded because filtered value was null
    public long CellsDiscarded { get; set; }

    public double CellSize { get; set; }
    public VolumeComputationType VolumeType { get; set; } = VolumeComputationType.None;

    // Volume is the calculated volume determined by simple difference between
    // cells. It does not take into account cut/fill differences (see FCut|FillVolume)
    // This volume is the sole output for operations that apply levels to the surfaces
    public double Volume { get; set; }

    // CutFillVolume is the calculated volume of material that has been 'cut' and 'filled' when the
    // base surface is compared to the top surface. ie: If the top surface is below
    // the base surface at a point then that point is in 'cut'.
    public CutFillVolume CutFillVolume = new CutFillVolume(0, 0);

    public DesignDescriptor DesignDescriptor = DesignDescriptor.Null(); // no {get;set;} intentionally
    private bool _disposedValue;

    public double TopLevel { get; set; } = 0;
    public double BaseLevel { get; set; } = 0;
    public double CoverageArea { get; set; }
    public double CutArea { get; set; }
    public double FillArea { get; set; }
    public double TotalArea { get; set; }
    public BoundingWorldExtent3D BoundingExtents { get; set; } = BoundingWorldExtent3D.Inverted();

    // CutTolerance determines the tolerance (in meters) that the 'From' surface
    // needs to be above the 'To' surface before the two surfaces are not
    // considered to be equivalent, or 'on-grade', and hence there is material still remaining to
    // be cut
    public double CutTolerance { get; set; } = VolumesConsts.DEFAULT_CELL_VOLUME_CUT_TOLERANCE;

    // FillTolerance determines the tolerance (in meters) that the 'To' surface
    // needs to be above the 'From' surface before the two surfaces are not
    // considered to be equivalent, or 'on-grade', and hence there is material still remaining to
    // be cut
    public double FillTolerance { get; set; } = VolumesConsts.DEFAULT_CELL_VOLUME_FILL_TOLERANCE;

    public SimpleVolumesCalculationsAggregator() { }

    public void Finalise()
    {
      CoverageArea = CellsUsed * CellSize * CellSize;
      CutArea = CellsUsedCut * CellSize * CellSize;
      FillArea = CellsUsedFill * CellSize * CellSize;
      TotalArea = CellsScanned * CellSize * CellSize;
      BoundingExtents = CoverageMap.ComputeCellsWorldExtents();
    }

    protected void ProcessVolumeInformationForSubGrid(ClientHeightLeafSubGrid baseScanSubGrid,
                                                      ClientHeightLeafSubGrid topScanSubGrid)
    {
      // DesignHeights represents all the valid spot elevations for the cells in the
      // sub grid being processed
      (IClientHeightLeafSubGrid designHeights, DesignProfilerRequestResult profilerRequestResult) getDesignHeightsResult = (null, DesignProfilerRequestResult.UnknownError);

      // FCellArea is a handy place to store the cell area, rather than calculate it all the time (value wont change);
      var cellArea = CellSize * CellSize;

      // Query the patch of elevations from the surface model for this sub grid
      if (ActiveDesign != null)
      {
        getDesignHeightsResult = ActiveDesign.Design.GetDesignHeightsViaLocalCompute(SiteModelID, ActiveDesign.Offset, baseScanSubGrid.OriginAsCellAddress(), CellSize);

        if (getDesignHeightsResult.profilerRequestResult != DesignProfilerRequestResult.OK &&
            getDesignHeightsResult.profilerRequestResult != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
        {
          _log.LogError($"Design profiler sub grid elevation request for {baseScanSubGrid.OriginAsCellAddress()} failed with error {getDesignHeightsResult.profilerRequestResult}");
          return;
        }
      }

      var bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

      // ReSharper disable once CompareOfFloatsByEqualityOperator
      var standardVolumeProcessing = LiftParams.TargetLiftThickness == Consts.NullHeight || LiftParams.TargetLiftThickness <= 0;

      var localCellsScanned = 0;
      var localCellsUsed = 0;
      var localCellsDiscarded = 0;
      var localCellsUsedCut = 0;
      var localCellsUsedFill = 0;
      var localVolume = 0.0d;
      var localCutFillVolume = new CutFillVolume(0, 0);

      // If we are interested in standard volume processing use this cycle
      if (standardVolumeProcessing)
      {
        localCellsScanned += SubGridTreeConsts.SubGridTreeCellsPerSubGrid;

        for (var i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
        {
          for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
          {
            float topZ;
            var baseZ = baseScanSubGrid.Cells[i, j];

            // If the user has configured a first pass thickness, then we need to subtract this height
            // difference from the BaseZ retrieved from the current cell if this measured height was
            // the first pass made in the cell.
            if (LiftParams.FirstPassThickness > 0)
            {
              baseZ -= LiftParams.FirstPassThickness;
            }

            if (VolumeType == VolumeComputationType.BetweenFilterAndDesign ||
                VolumeType == VolumeComputationType.BetweenDesignAndFilter)
            {
              topZ = getDesignHeightsResult.designHeights?.Cells[i, j] ?? Consts.NullHeight;

              if (VolumeType == VolumeComputationType.BetweenDesignAndFilter)
                MinMax.Swap(ref baseZ, ref topZ);
            }
            else
              topZ = topScanSubGrid.Cells[i, j];

            switch (VolumeType)
            {
              case VolumeComputationType.None:
                break;

              case VolumeComputationType.AboveLevel:
                {
                  // ReSharper disable once CompareOfFloatsByEqualityOperator
                  if (baseZ != Consts.NullHeight)
                  {
                    localCellsUsed++;
                    if (baseZ > BaseLevel)
                      localVolume += cellArea * (baseZ - BaseLevel);
                  }
                  else
                    localCellsDiscarded++;

                  break;
                }

              case VolumeComputationType.Between2Levels:
                {
                  // ReSharper disable once CompareOfFloatsByEqualityOperator
                  if (baseZ != Consts.NullHeight)
                  {
                    localCellsUsed++;

                    if (baseZ > BaseLevel)
                      localVolume += cellArea * (baseZ < TopLevel ? (baseZ - BaseLevel) : (TopLevel - BaseLevel));
                  }
                  else
                    localCellsDiscarded++;

                  break;
                }

              case VolumeComputationType.AboveFilter:
              case VolumeComputationType.Between2Filters:
              case VolumeComputationType.BetweenFilterAndDesign:
              case VolumeComputationType.BetweenDesignAndFilter:
                {
                  // ReSharper disable once CompareOfFloatsByEqualityOperator
                  if (baseZ != Consts.NullHeight &&
                      // ReSharper disable once CompareOfFloatsByEqualityOperator
                      topZ != Consts.NullHeight)
                  {
                    localCellsUsed++;

                    //  Note the fact we have processed this cell in the coverage map
                    bits.SetBit(i, j);

                    var cellUsedInVolumeCalc = (topZ - baseZ >= FillTolerance) || (baseZ - topZ >= CutTolerance);

                    // Accumulate volumes
                    if (cellUsedInVolumeCalc)
                    {
                      var volumeDifference = cellArea * (topZ - baseZ);

                      // Accumulate the 'surplus' volume. Ie: the simple summation of
                      // all cuts and fills.
                      localVolume += volumeDifference;

                      // Accumulate the cuts and fills into discrete cut and fill quantities
                      if (topZ < baseZ)
                      {
                        localCellsUsedCut++;
                        localCutFillVolume.AddCutVolume(Math.Abs(volumeDifference));
                      }
                      else
                      {
                        localCellsUsedFill++;
                        localCutFillVolume.AddFillVolume(Math.Abs(volumeDifference));
                      }
                    }
                    else
                    {
                      // Note the fact there was no volume change in this cell
                      // NoChangeMap.Cells[BaseScanSubGrid.OriginX + I, BaseScanSubGrid.OriginY + J] := True;
                    }
                  }
                  else
                  {
                    localCellsDiscarded++;
                  }
                }
                break;

              default:
                _log.LogError($"Unknown volume type {VolumeType} in ProcessVolumeInformationForSubGrid()");
                break;
            }
          }
        }
      }

      // ReSharper disable once CompareOfFloatsByEqualityOperator
      var targetLiftThicknessCalculationsRequired = LiftParams.TargetLiftThickness != Consts.NullHeight && LiftParams.TargetLiftThickness > 0;

      //If we are interested in thickness calculations do them
      if (targetLiftThicknessCalculationsRequired)
      {
        var belowToleranceToCheck = LiftParams.TargetLiftThickness - LiftParams.BelowToleranceLiftThickness;
        var aboveToleranceToCheck = LiftParams.TargetLiftThickness + LiftParams.AboveToleranceLiftThickness;

        SubGridUtilities.SubGridDimensionalIterator((i, j) =>
        {
          var baseZ = baseScanSubGrid.Cells[i, j];
          var topZ = topScanSubGrid.Cells[i, j];

          // ReSharper disable once CompareOfFloatsByEqualityOperator
          if (baseZ != Consts.NullHeight ||
              // ReSharper disable once CompareOfFloatsByEqualityOperator
              topZ != Consts.NullHeight)
          {
            localCellsScanned++;
          }

          //Test if we don't have NULL values and carry on
          // ReSharper disable once CompareOfFloatsByEqualityOperator
          if (baseZ != Consts.NullHeight &&
              // ReSharper disable once CompareOfFloatsByEqualityOperator
              topZ != Consts.NullHeight)
          {
            bits.SetBit(i, j);
            double elevationDiff = topZ - baseZ;

            if (elevationDiff <= aboveToleranceToCheck && elevationDiff >= belowToleranceToCheck)
              localCellsUsed++;
            else if (elevationDiff > aboveToleranceToCheck)
              localCellsUsedFill++;
            else if (elevationDiff < belowToleranceToCheck)
              localCellsUsedCut++;
          }
          else
          {
            localCellsDiscarded++;
          }
        });
      }

      // Update the quantities in the aggregator proper
      // Note: the lock is not asynchronous as this will be highly non-contended
      _lock.Wait();
      try
      {
        CellsScanned += localCellsScanned;
        CellsUsed += localCellsUsed;
        CellsDiscarded += localCellsDiscarded;
        CellsUsedCut += localCellsUsedCut;
        CellsUsedFill += localCellsUsedFill;
        Volume += localVolume;
        CutFillVolume.AddCutFillVolume(localCutFillVolume.CutVolume, localCutFillVolume.FillVolume);

        // Record the bits for this sub grid in the coverage map by requesting the whole sub grid
        // of bits from the leaf level and setting it in one operation under an exclusive lock
        if (!bits.IsEmpty())
        {
          var coverageMapSubGrid = CoverageMap.ConstructPathToCell(baseScanSubGrid.OriginX, baseScanSubGrid.OriginY, SubGridPathConstructionType.CreateLeaf);
          ((SubGridTreeLeafBitmapSubGrid)coverageMapSubGrid).Bits = bits;
        }
      }
      finally
      {
        _lock.Release();
      }
    }

    /// <summary>
    /// Summarizes the client height grids derived from sub grid processing into the running volumes aggregation state
    /// </summary>
    public void SummarizeSubGridResultAsync(IClientLeafSubGrid[][] subGrids)
    {
      //var taskList = new List<Task>(subGrids.Length);

      foreach (var subGridResult in subGrids)
      {
        if (subGridResult != null)
        {
          // We have a sub grid from the Production Database. If we are processing volumes
          // between two filters, then there will be a second sub grid in the sub grids array.
          // By convention BaseSubGrid is always the first sub grid in the array,
          // regardless of whether it really forms the 'top' or 'bottom' of the interval.

          var baseSubGrid = subGridResult[0];

          if (baseSubGrid == null)
          {
            _log.LogWarning("#W# SummarizeSubGridResult BaseSubGrid is null");
          }
          else
          {
            var topSubGrid = subGridResult.Length > 1 ? subGridResult[1] : _nullHeightSubGrid;

            ProcessVolumeInformationForSubGrid(baseSubGrid as ClientHeightLeafSubGrid, topSubGrid as ClientHeightLeafSubGrid);
            //taskList.Add(ProcessVolumeInformationForSubGrid(baseSubGrid as ClientHeightLeafSubGrid, topSubGrid as ClientHeightLeafSubGrid));
          }
        }
      }

      /*
      try
      {
        await Task.WhenAll(taskList);
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception: SimpleVolumesCalculationsAggregator: WhenAll() failed");
      }
      */
    }

    /// <summary>
    /// Provides a human readable form of the aggregator state
    /// </summary>
    public override string ToString()
    {
      return $"VolumeType:{VolumeType}, CellSize:{CellSize}, CoverageArea:{CoverageArea}, Bounding:{BoundingExtents}, " +
          $"Volume:{Volume}, Cut:{CutFillVolume.CutVolume}, Fill:{CutFillVolume.FillVolume}, " +
          $"Cells Used/Discarded/Scanned:{CellsUsed}/{CellsDiscarded}/{CellsScanned}, ReferenceDesign:{DesignDescriptor}";
    }

    /// <summary>
    /// Combine this aggregator with another simple volumes aggregator and store the result in this aggregator
    /// </summary>
    public SimpleVolumesCalculationsAggregator AggregateWith(SimpleVolumesCalculationsAggregator other)
    {
      _lock.Wait();
      try
      {
        //  SIGLogMessage.PublishNoODS(Self, Format('Aggregating From:%s', [Source.ToString]));
        //  SIGLogMessage.PublishNoODS(Self, Format('Into:%s', [ToString]));

        CellsUsed += other.CellsUsed;
        CellsUsedCut += other.CellsUsedCut;
        CellsUsedFill += other.CellsUsedFill;
        CellsScanned += other.CellsScanned;
        CellsDiscarded += other.CellsDiscarded;

        CoverageArea += other.CoverageArea;
        CutArea += other.CutArea;
        FillArea += other.FillArea;
        TotalArea += other.TotalArea;
        BoundingExtents.Include(other.BoundingExtents);

        Volume += other.Volume;
        CutFillVolume.AddCutFillVolume(other.CutFillVolume.CutVolume, other.CutFillVolume.FillVolume);

        return this;
      }
      finally
      {
        _lock.Release();
      }
    }

    /// <summary>
    /// Implement the sub grids request aggregator method to process sub grid results...
    /// </summary
    public void ProcessSubGridResult(IClientLeafSubGrid[][] subGrids)
    {
      SummarizeSubGridResultAsync(subGrids); //.WaitAndUnwrapException();
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          _lock?.Dispose();
          _lock = null;
        }

        _disposedValue = true;
      }
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
    }
  }
}
