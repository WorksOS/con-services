using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.Utilities;

namespace VSS.TRex.Volumes
{
    /// <summary>
    /// Defines an aggregator that summaries simple volumes information for subgrids
    /// </summary>
    public class SimpleVolumesCalculationsAggregator : ISubGridRequestsAggregator, IAggregateWith<SimpleVolumesCalculationsAggregator>
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// Defines a subgrid full of null values to run through the volumes engine in cases when 
        /// one of the two subgrids is not available to allow for correctly tracking of statistics
        /// </summary>
        private static ClientHeightLeafSubGrid NullHeightSubgrid = new ClientHeightLeafSubGrid(null, null, 0, 0, 0);

        // CoverageMap maps the area of cells that we have considered and successfully
        // computed volume information from
        public SubGridTreeBitMask CoverageMap = new SubGridTreeBitMask();

        // NoChangeMap maps the area of cells that we have considered and found to have
        // had no height change between to two surfaces considered
        // SubGridTreeBitMask FNoChangeMap = new SubGridTreeBitMask();

        /// <summary>
        /// The design being used to compare heights derived from production data against to calculate per-cell volumes
        /// </summary>
        public IDesign ActiveDesign { get; set; }

        // References necessary for correct summarisation of aggregated state

        // public LiftBuildSettings        : TICLiftBuildSettings; = null;

        public Guid SiteModelID { get; set; } = Guid.Empty;

        public bool RequiresSerialisation { get; set; } = true;

        // The sum of the aggregated summarised information relating to volumes summary based reports

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

        //  TICVolumesCalculationsAggregateStateArray = Array of TICVolumesCalculationsAggregateState;

        public SimpleVolumesCalculationsAggregator()
        {
            // NOTE: This aggregator state is now single threaded in the context of processing subgrid
            // information into it as the processing threads access independent sub-state aggregators which
            // are aggregated together to form the final aggregation result. However, in contexts that do support
            // threaded access to this structure the FRequiresSerialisation flag should be set

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

        protected void ProcessVolumeInformationForSubgrid(ClientHeightLeafSubGrid BaseScanSubGrid,
                                                          ClientHeightLeafSubGrid TopScanSubGrid)
        {
            // DesignHeights represents all the valid spot elevations for the cells in the
            // subgrid being processed
            IClientHeightLeafSubGrid DesignHeights = null;
            DesignProfilerRequestResult ProfilerRequestResult = DesignProfilerRequestResult.UnknownError;

            // FCellArea is a handy place to store the cell area, rather than calculate it all the time (value wont change);
            double CellArea = CellSize * CellSize;

            // Query the patch of elevations from the surface model for this subgrid
            if (ActiveDesign?.GetDesignHeights(SiteModelID,
                                               BaseScanSubGrid.OriginAsCellAddress(),
                                               CellSize, out DesignHeights, out ProfilerRequestResult) == false)
            {
                if (ProfilerRequestResult != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
                {
                    Log.LogError($"Design profiler subgrid elevation request for {BaseScanSubGrid.OriginAsCellAddress()} failed with error {ProfilerRequestResult}");
                    return;
                }
            }

            SubGridTreeBitmapSubGridBits Bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

            //const bool StandardVolumeProcessing = true; // TODO: Should be -> (LiftBuildSettings.TargetLiftThickness == Consts.NullHeight || LiftBuildSettings.TargetLiftThickness <= 0)

            // If we are interested in standard volume processing use this cycle
            // Uncomment when StandardVolumeProcessing becomes... not constant
            // if (StandardVolumeProcessing)
            {
                CellsScanned += SubGridTreeConsts.SubGridTreeCellsPerSubgrid;

                for (int I = 0; I < SubGridTreeConsts.SubGridTreeDimension; I++)
                {
                    for (int J = 0; J < SubGridTreeConsts.SubGridTreeDimension; J++)
                    {
                        float TopZ;
                        float BaseZ = BaseScanSubGrid.Cells[I, J];

                        /* TODO - removed for Ignite POC until LiftBuildSettings is available
                        // If the user has configured a first pass thickness, then we need to subtract this height
                        // difference from the BaseZ retrieved from the current cell if this measured height was
                        // the first pass made in the cell.
                        if (LiftBuildSettings.FirstPassThickness > 0)
                        {
                            BaseZ -= LiftBuildSettings.FirstPassThickness;
                        }
                        */

                        if (VolumeType == VolumeComputationType.BetweenFilterAndDesign ||
                            VolumeType == VolumeComputationType.BetweenDesignAndFilter)
                        {
                            TopZ = DesignHeights?.Cells[I, J] ?? Consts.NullHeight;

                            if (VolumeType == VolumeComputationType.BetweenDesignAndFilter)
                                MinMax.Swap(ref BaseZ, ref TopZ);
                        }
                        else
                            TopZ = TopScanSubGrid.Cells[I, J];

                        switch (VolumeType)
                        {
                            case VolumeComputationType.None:
                                break;

                            case VolumeComputationType.AboveLevel:
                            {
                                if (BaseZ != Consts.NullHeight)
                                {
                                    CellsUsed++;
                                    if (BaseZ > BaseLevel)
                                        Volume += CellArea * (BaseZ - BaseLevel);
                                }
                                else
                                    CellsDiscarded++;

                                break;
                            }

                            case VolumeComputationType.Between2Levels:
                            {
                                if (BaseZ != Consts.NullHeight)
                                {
                                    CellsUsed++;

                                    if (BaseZ > BaseLevel)
                                        Volume += CellArea * (BaseZ < TopLevel ? (BaseZ - BaseLevel) : (TopLevel - BaseLevel));
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
                                if (BaseZ != Consts.NullHeight && TopZ != Consts.NullHeight)
                                {
                                    CellsUsed++;

                                    //  Note the fact we have processed this cell in the coverage map
                                    Bits.SetBit(I, J);

                                    // FCoverageMap.Cells[BaseScanSubGrid.OriginX + I, BaseScanSubGrid.OriginY + J] := True;

                                    bool CellUsedInVolumeCalc = (TopZ - BaseZ >= FillTolerance) || (BaseZ - TopZ >= CutTolerance);

                                    // Accumulate volumes
                                    if (CellUsedInVolumeCalc)
                                    {
                                        double VolumeDifference = CellArea * (TopZ - BaseZ);

                                        // Accumulate the 'surplus' volume. Ie: the simple summation of
                                        // all cuts and fills.
                                        Volume += VolumeDifference;

                                        // Accumulate the cuts and fills into discrete cut and fill quantities
                                        if (TopZ < BaseZ)
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

      // const bool TargetLiftThicknessCalculationsRequired = false; // TODO: Should be -> (LiftBuildSettings.TargetLiftThickness != Consts.NullHeight && LiftBuildSettings.TargetLiftThickness > 0)

      //If we are interested in thickness calculations do them
      /* todo Uncomment when the constant above becomes... not constant
      if (TargetLiftThicknessCalculationsRequired)
      {
          double BelowToleranceToCheck = LiftBuildSettings.TargetLiftThickness - LiftBuildSettings.BelowToleranceLiftThickness;
          double AboveToleranceToCheck = LiftBuildSettings.TargetLiftThickness + LiftBuildSettings.AboveToleranceLiftThickness;

          SubGridUtilities.SubGridDimensionalIterator((I, J) =>
          {
              BaseZ = BaseScanSubGrid.Cells[I, J];
              TopZ = TopScanSubGrid.Cells[I, J];

              if (BaseZ != Consts.NullHeight || TopZ != Consts.NullHeight)
                  CellsScanned++;

                  //Test if we don't have NULL values and carry on
              if (BaseZ != Consts.NullHeight && TopZ != Consts.NullHeight)
              {
                  Bits.SetBit(I, J);
                  double ElevationDiff = TopZ - BaseZ;

                  if (ElevationDiff <= AboveToleranceToCheck && ElevationDiff >= BelowToleranceToCheck)
                      CellsUsed++;
                  else
                      if (ElevationDiff > AboveToleranceToCheck)
                          CellsUsedFill++;
                      else
                          if (ElevationDiff < BelowToleranceToCheck)
                              CellsUsedCut++;
              }
              else
                  CellsDiscarded++;
          });
      }
      */

      // Record the bits for this subgrid in the coverage map by requesting the whole subgrid
      // of bits from the leaf level and setting it in one operation under an exclusive lock
      if (!Bits.IsEmpty())
            {
                if (RequiresSerialisation)
                    Monitor.Enter(CoverageMap);
                try
                {
                    ISubGrid CoverageMapSubgrid = CoverageMap.ConstructPathToCell(BaseScanSubGrid.OriginX, BaseScanSubGrid.OriginY, SubGridPathConstructionType.CreateLeaf);

                    if (CoverageMapSubgrid != null)
                    {
                        Debug.Assert(CoverageMapSubgrid is SubGridTreeLeafBitmapSubGrid, "CoverageMapSubgrid in TICVolumesCalculationsAggregateState.ProcessVolumeInformationForSubgrid is not a TSubGridTreeLeafBitmapSubGrid");
                        ((SubGridTreeLeafBitmapSubGrid)CoverageMapSubgrid).Bits = Bits;
                    }
                    else
                        Debug.Assert(false, "Failed to request CoverageMapSubgrid from FCoverageMap in TICVolumesCalculationsAggregateState.ProcessVolumeInformationForSubgrid");
                }
                finally
                {
                    if (RequiresSerialisation)
                        Monitor.Exit(CoverageMap);
                }
            }
        }

        /// <summary>
        /// Summarises the client height grids derived from subgrid processing into the running volumes aggregation state
        /// </summary>
        /// <param name="subGrids"></param>
        public void SummariseSubgridResult(IClientLeafSubGrid[][] subGrids)
        {
            if (RequiresSerialisation)
                Monitor.Enter(this);

            try
            {
                foreach (IClientLeafSubGrid[] subGridResult in subGrids)
                {
                    if (subGridResult == null)
                      continue;

                    // We have a subgrid from the Production Database. If we are processing volumes
                    // between two filters, then there will be a second subgrid in the sungrids array.
                    // By convention BaseSubgrid is always the first subgrid in the array,
                    // regardless of whether it really forms the 'top' or 'bottom' of the interval.

                    IClientLeafSubGrid TopSubGrid;
                    IClientLeafSubGrid BaseSubGrid = subGridResult[0];

                    if (BaseSubGrid == null)
                    {
                        Log.LogWarning("#W# .SummariseSubgridResult BaseSubGrid is null");
                        return;
                    }

                    if (subGrids.Length > 1)
                        TopSubGrid = subGridResult[1]; 
                    else
                        TopSubGrid = NullHeightSubgrid;

                    ProcessVolumeInformationForSubgrid(BaseSubGrid as ClientHeightLeafSubGrid, TopSubGrid as ClientHeightLeafSubGrid);
                }
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
            return $"VolumeType:{VolumeType}, Cellsize:{CellSize}, CoverageArea:{CoverageArea}, Bounding:{BoundingExtents}, " +
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
        /// Implement the subgrids request aggregator method to process subgrid results...
        /// </summary>
        /// <param name="subGrids"></param>
        public void ProcessSubgridResult(IClientLeafSubGrid[][] subGrids)
        {
            SummariseSubgridResult(subGrids);
        }
    }
}
