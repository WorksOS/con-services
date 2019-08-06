using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx.Synchronous;
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
  public class SimpleVolumesCalculationsAggregator : ISubGridRequestsAggregator, IAggregateWith<SimpleVolumesCalculationsAggregator>
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SimpleVolumesCalculationsAggregator>();

    /// <summary>
    /// Defines a sub grid full of null values to run through the volumes engine in cases when 
    /// one of the two sub grids is not available to allow for correctly tracking of statistics
    /// </summary>
    private static readonly ClientHeightLeafSubGrid NullHeightSubGrid = new ClientHeightLeafSubGrid(null, null, 0, 0, 0);

    // CoverageMap maps the area of cells that we have considered and successfully
    // computed volume information from
    public readonly SubGridTreeBitMask CoverageMap = new SubGridTreeBitMask();

    // NoChangeMap maps the area of cells that we have considered and found to have
    // had no height change between to two surfaces considered
    // SubGridTreeBitMask FNoChangeMap = new SubGridTreeBitMask();

    /// <summary>
    /// The design being used to compare heights derived from production data against to calculate per-cell volumes
    /// </summary>
    public IDesign ActiveDesign { get; set; }
    /// <summary>
    /// The offset if the design is a reference surface
    /// </summary>
    public double ActiveDesignOffset;

    // References necessary for correct summarization of aggregated state
    public ILiftParameters LiftParams { get; set; } = new LiftParameters();

    public Guid SiteModelID { get; set; } = Guid.Empty;

    public bool RequiresSerialisation { get; set; } = true;

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

    public SimpleVolumesCalculationsAggregator()
    {
      // NOTE: This aggregator state is now single threaded in the context of processing sub grid
      // information into it as the processing threads access independent sub-state aggregators which
      // are aggregated together to form the final aggregation result. However, in contexts that do support
      // threaded access to this structure the RequiresSerialisation flag should be set

      // if Assigned(Source) then
      //    Initialise(Source);
    }

    public void Finalise()
    {
      CoverageArea = CellsUsed * CellSize * CellSize;
      CutArea = CellsUsedCut * CellSize * CellSize;
      FillArea = CellsUsedFill * CellSize * CellSize;
      TotalArea = CellsScanned * CellSize * CellSize;
      BoundingExtents = CoverageMap.ComputeCellsWorldExtents();
    }

    /*
     * procedure TICVolumesCalculationsAggregateState.Initialise(const Source : TICVolumesCalculationsAggregateState);
        begin
      if Assigned(Source) then
        begin
          FCellSize := Source.CellSize;
          FDesignDescriptor := Source.DesignDescriptor;
          FVolumeType := Source.VolumeType;
          FBaseLevel := Source.BaseLevel;
          FTopLevel := Source.TopLevel;

          LiftBuildSettings := Source.LiftBuildSettings;
          SiteModelID := Source.SiteModelID;
          DesignProfilerService := Source.DesignProfilerService;

          CutTolerance := Source.CutTolerance;
          FillTolerance := Source.FillTolerance;
        end;

    //  FNoChangeMap := TSubGridTreeBitMask.Create(kSubGridTreeLevels, CellSize);

      FNullHeightSubgrid := TICClientSubGridTreeLeaf_Height.Create(Nil, Nil, kSubGridTreeLevels, CellSize, 0); //cell size of datamodel in question
      FNullHeightSubgrid.Clear;
    end;
    */

    protected async Task ProcessVolumeInformationForSubgrid(ClientHeightLeafSubGrid BaseScanSubGrid,
                                                      ClientHeightLeafSubGrid TopScanSubGrid)
    {
      // DesignHeights represents all the valid spot elevations for the cells in the
      // sub grid being processed
      (IClientHeightLeafSubGrid designHeights, DesignProfilerRequestResult profilerRequestResult) getDesignHeightsResult = (null, DesignProfilerRequestResult.UnknownError);

      // FCellArea is a handy place to store the cell area, rather than calculate it all the time (value wont change);
      var cellArea = CellSize * CellSize;

      // Query the patch of elevations from the surface model for this sub grid
      if (ActiveDesign != null)
      {
        getDesignHeightsResult = await ActiveDesign.GetDesignHeights(SiteModelID, ActiveDesignOffset, BaseScanSubGrid.OriginAsCellAddress(), CellSize);

        if (getDesignHeightsResult.profilerRequestResult != DesignProfilerRequestResult.OK &&
            getDesignHeightsResult.profilerRequestResult != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
        {
          Log.LogError($"Design profiler sub grid elevation request for {BaseScanSubGrid.OriginAsCellAddress()} failed with error {getDesignHeightsResult.profilerRequestResult}");
          return;
        }
      }

      var bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

      var StandardVolumeProcessing = LiftParams.TargetLiftThickness == Consts.NullHeight || LiftParams.TargetLiftThickness <= 0;

      // If we are interested in standard volume processing use this cycle
      if (StandardVolumeProcessing)
      {
        CellsScanned += SubGridTreeConsts.SubGridTreeCellsPerSubGrid;

        for (var i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
        {
          for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
          {
            float topZ;
            float baseZ = BaseScanSubGrid.Cells[i, j];

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
              topZ = TopScanSubGrid.Cells[i, j];

            switch (VolumeType)
            {
              case VolumeComputationType.None:
                break;

              case VolumeComputationType.AboveLevel:
                {
                  if (baseZ != Consts.NullHeight)
                  {
                    CellsUsed++;
                    if (baseZ > BaseLevel)
                      Volume += cellArea * (baseZ - BaseLevel);
                  }
                  else
                    CellsDiscarded++;

                  break;
                }

              case VolumeComputationType.Between2Levels:
                {
                  if (baseZ != Consts.NullHeight)
                  {
                    CellsUsed++;

                    if (baseZ > BaseLevel)
                      Volume += cellArea * (baseZ < TopLevel ? (baseZ - BaseLevel) : (TopLevel - BaseLevel));
                  }
                  else
                    CellsDiscarded++;

                  break;
                }

              case VolumeComputationType.AboveFilter:
              case VolumeComputationType.Between2Filters:
              case VolumeComputationType.BetweenFilterAndDesign:
              case VolumeComputationType.BetweenDesignAndFilter:
                {
                  if (baseZ != Consts.NullHeight && topZ != Consts.NullHeight)
                  {
                    CellsUsed++;

                    //  Note the fact we have processed this cell in the coverage map
                    bits.SetBit(i, j);

                    // FCoverageMap.Cells[BaseScanSubGrid.OriginX + I, BaseScanSubGrid.OriginY + J] := True;

                    bool CellUsedInVolumeCalc = (topZ - baseZ >= FillTolerance) || (baseZ - topZ >= CutTolerance);

                    // Accumulate volumes
                    if (CellUsedInVolumeCalc)
                    {
                      double VolumeDifference = cellArea * (topZ - baseZ);

                      // Accumulate the 'surplus' volume. Ie: the simple summation of
                      // all cuts and fills.
                      Volume += VolumeDifference;

                      // Accumulate the cuts and fills into discrete cut and fill quantities
                      if (topZ < baseZ)
                      {
                        CellsUsedCut++;
                        CutFillVolume.AddCutVolume(Math.Abs(VolumeDifference));
                      }
                      else
                      {
                        CellsUsedFill++;
                        CutFillVolume.AddFillVolume(Math.Abs(VolumeDifference));
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
                    CellsDiscarded++;
                  }
                }
                break;

              default:
                Log.LogError($"Unknown volume type {VolumeType} in ProcessVolumeInformationForSubgrid()");
                break;
            }
          }
        }
      }

      var targetLiftThicknessCalculationsRequired = LiftParams.TargetLiftThickness != Consts.NullHeight && LiftParams.TargetLiftThickness > 0;

      //If we are interested in thickness calculations do them
      if (targetLiftThicknessCalculationsRequired)
      {
          double belowToleranceToCheck = LiftParams.TargetLiftThickness - LiftParams.BelowToleranceLiftThickness;
          double aboveToleranceToCheck = LiftParams.TargetLiftThickness + LiftParams.AboveToleranceLiftThickness;

          SubGridUtilities.SubGridDimensionalIterator((I, J) =>
          {
              var baseZ = BaseScanSubGrid.Cells[I, J];
              var topZ = TopScanSubGrid.Cells[I, J];

              if (baseZ != Consts.NullHeight || topZ != Consts.NullHeight)
                  CellsScanned++;

                  //Test if we don't have NULL values and carry on
              if (baseZ != Consts.NullHeight && topZ != Consts.NullHeight)
              {
                  bits.SetBit(I, J);
                  double ElevationDiff = topZ - baseZ;

                  if (ElevationDiff <= aboveToleranceToCheck && ElevationDiff >= belowToleranceToCheck)
                      CellsUsed++;
                  else
                      if (ElevationDiff > aboveToleranceToCheck)
                          CellsUsedFill++;
                      else
                          if (ElevationDiff < belowToleranceToCheck)
                              CellsUsedCut++;
              }
              else
                  CellsDiscarded++;
          });
      }

      // Record the bits for this sub grid in the coverage map by requesting the whole sub grid
      // of bits from the leaf level and setting it in one operation under an exclusive lock
      if (!bits.IsEmpty())
      {
        if (RequiresSerialisation)
          Monitor.Enter(CoverageMap);
        try
        {
          ISubGrid CoverageMapSubgrid = CoverageMap.ConstructPathToCell(BaseScanSubGrid.OriginX, BaseScanSubGrid.OriginY, SubGridPathConstructionType.CreateLeaf);
          ((SubGridTreeLeafBitmapSubGrid)CoverageMapSubgrid).Bits = bits;
        }
        finally
        {
          if (RequiresSerialisation)
            Monitor.Exit(CoverageMap);
        }
      }
    }

    /// <summary>
    /// Summarises the client height grids derived from sub grid processing into the running volumes aggregation state
    /// </summary>
    /// <param name="subGrids"></param>
    public async Task SummariseSubgridResultAsync(IClientLeafSubGrid[][] subGrids)
    {
      if (RequiresSerialisation)
        Monitor.Enter(this);

      try
      {
        var taskList = new List<Task>(subGrids.Length);

        foreach (IClientLeafSubGrid[] subGridResult in subGrids)
        {
          if (subGridResult != null)
          {
            // We have a sub grid from the Production Database. If we are processing volumes
            // between two filters, then there will be a second sub grid in the sub grids array.
            // By convention BaseSubGrid is always the first sub grid in the array,
            // regardless of whether it really forms the 'top' or 'bottom' of the interval.

            var baseSubGrid = subGridResult[0];

            if (baseSubGrid == null)
              Log.LogWarning("#W# SummariseSubGridResult BaseSubGrid is null");
            else
            {
              var topSubGrid = subGridResult.Length > 1 ? subGridResult[1] : NullHeightSubGrid;

              taskList.Add(ProcessVolumeInformationForSubgrid(baseSubGrid as ClientHeightLeafSubGrid, topSubGrid as ClientHeightLeafSubGrid));
            }
          }
        }

        await Task.WhenAll(taskList);
      }
      finally
      {
        if (RequiresSerialisation)
          Monitor.Exit(this);
      }
    }

    /// <summary>
    /// Provides a human readable form of the aggregator state
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return $"VolumeType:{VolumeType}, CellSize:{CellSize}, CoverageArea:{CoverageArea}, Bounding:{BoundingExtents}, " +
          $"Volume:{Volume}, Cut:{CutFillVolume.CutVolume}, Fill:{CutFillVolume.FillVolume}, " +
          $"Cells Used/Discarded/Scanned:{CellsUsed}/{CellsDiscarded}/{CellsScanned}, ReferenceDesign:{DesignDescriptor}";
    }

    /// <summary>
    /// Combine this aggregator with another simple volumes aggregator and store the result in this aggregator
    /// </summary>
    /// <param name="other"></param>
    public SimpleVolumesCalculationsAggregator AggregateWith(SimpleVolumesCalculationsAggregator other)
    {
      if (RequiresSerialisation)
      {
        //  TMonitor.Enter(Self);
      }
      try
      {
        //  SIGLogMessage.PublishNoODS(Self, Format('Aggregating From:%s', [Source.ToString]), slmcDebug);
        //  SIGLogMessage.PublishNoODS(Self, Format('Into:%s', [ToString]), slmcDebug);

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
        if (RequiresSerialisation)
        {
          //   TMonitor.Exit(Self);
        }
      }
    }

    /// <summary>
    /// Implement the sub grids request aggregator method to process sub grid results...
    /// </summary>
    /// <param name="subGrids"></param>
    public void ProcessSubGridResult(IClientLeafSubGrid[][] subGrids)
    {
      SummariseSubgridResultAsync(subGrids).WaitAndUnwrapException();
    }
  }
}
