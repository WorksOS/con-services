using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Designs;
using VSS.VisionLink.Raptor.Designs.Storage;
using VSS.VisionLink.Raptor.Executors.Tasks;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.Pipelines;
using VSS.VisionLink.Raptor.Services.Designs;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.SubGridTrees.Client;
using VSS.VisionLink.Raptor.Surfaces;
using VSS.VisionLink.Raptor.Types;
using VSS.VisionLink.Raptor.Volumes.Executors.Tasks;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.Volumes
{
    /// <summary>
    /// VolumesCalculatorBase provides a base class that may be extended/decorated
    /// to implement specific volume calculation engines that access production data and use
    /// it to derive volumes information.
    /// </summary>
    public abstract class VolumesCalculatorBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The Aggregator to use for calculation volumes statistics
        /// </summary>
        public SimpleVolumesCalculationsAggregator Aggregator { get; set; } = null;

        /// <summary>
        /// The Sitemodel from which the volume is being calculated
        /// </summary>
        public SiteModel SiteModel { get; set; } = null;

        /// <summary>
        ///  Default no-arg constructor
        /// </summary>
        public VolumesCalculatorBase()
        {
        }

        public VolumesCalculatorBase(SimpleVolumesCalculationsAggregator aggregator) : this()
        {
            Aggregator = aggregator;
        }

        /*
            private
              function CheckCellIsInSpatialConstraints(const CellX, CellY: Integer): Boolean;
              function GetCellSize: Double;
*/
        // protected BoundingWorldExtent3D DataExtents = BoundingWorldExtent3D.Inverted();

        protected BoundingWorldExtent3D Extents = BoundingWorldExtent3D.Inverted(); // No get;set; on purpose

        /// <summary>
        /// BaseFilter and TopFilter reference two sets of filter settings
        /// between which we may calculate volumes. At the current time, it is
        /// meaingful for a filter to have a spatial extent, and to denote an
        /// 'as-at' time only.
        /// </summary>
        public CombinedFilter BaseFilter { get; set; } = null;
        public CombinedFilter TopFilter { get; set; } = null;

        /// <summary>
        /// RefOriginal references a subset that may be used in the volumes calculations
        /// process. If set, it represents the original ground of the site
        /// </summary>
        public Design RefOriginal { get; set; } = null;

        /// <summary>
        /// RefDesign references a subset that may be used in the volumes calculations
        /// process. If set, it takes the place of the 'top' filter.
        /// </summary>
        public Design RefDesign { get; set; } = null;

        /// <summary>
        /// ActiveDesign is the design surface being used as the comparison surface in the
        /// surface to prodiuction data volume calculations. It is assigned from the FRefOriginal
        /// and FRefDesign surfaces depending on the volumes reporting type and configuration.
        /// </summary>
        public Design ActiveDesign { get; set; } = null;

        /// <summary>
        /// FromSelectionType and ToSelectionType describe how we mix the two filters
        /// (BaseFilter and TopFilter) and two reference designs (RefOriginal and
        /// RefDesign) together to derive the upper and lower 'surfaces' between which
        /// we compute summary or details production volumes
        /// </summary>
        public ProdReportSelectionType FromSelectionType { get; set; } = ProdReportSelectionType.None;

        /// <summary>
        /// FromSelectionType and ToSelectionType describe how we mix the two filters
        /// (BaseFilter and TopFilter) and two reference designs (RefOriginal and
        /// RefDesign) together to derive the upper and lower 'surfaces' between which
        /// we compute summary or details production volumes
        /// </summary>
        public ProdReportSelectionType ToSelectionType { get; set; }  = ProdReportSelectionType.None;

        /*
        // FAborted keeps track of whether we've been buchwhacked or not!
        protected FAborted : Boolean;

        // TopScanSubGrid is a loose subgrid - we don't actually assign it to a tree
        // at all, because its context is identical to the context of BaseSubGrid
        protected FTopScanSubGrid : TICClientSubGridTreeLeaf_Height;

        // FCoverageMap maps the area of cells that we have considered and successfully
        // computed volume information from
        protected FCoverageMap : TSubGridTreeBitMask;

        // FNoChangeMap maps the area of cells that we have considered and found to have
        // had no height change between to two surfaces considered
        protected FNoChangeMap : TSubGridTreeBitMask;                      
        */

        /// <summary>
        /// UseEarliestData governs whether we want the earlist or latest data from filtered
        /// ranges of cell passes in the base filtered surface.
        /// 
        /// </summary>
        public bool UseEarliestData { get; set; } = false;

        /*
         FLiftBuildSettings : TICLiftBuildSettings;

         {$IFDEF DEBUG}
         // Is used to map the extent the processing engine has covered in its processing
         FProcessingCoverageMap : TSubGridTreeBitMask;
         {$ENDIF}

         {$IFDEF LOGVOLCALCANALYSIS}
         FLogFile : TextFile;
         {$ENDIF}

         // FPipeLine is the subgrid pipeline used to drive the extraction of subgrid information
         // for the purposes of computing volumes information
         FReturnCode           : TICFSErrorStatus;
        */

        SubGridTreeSubGridExistenceBitMask ProdDataExistenceMap = null; //: TProductionDataExistanceMap;      //FPDExistenceMap : TSubGridTreeBitMask;
        SubGridTreeSubGridExistenceBitMask OverallExistenceMap = null;

        SubGridTreeSubGridExistenceBitMask DesignSubgridOverlayMap = null;

        public bool AbortedDueToTimeout { get; set; } = false;

        //        FEpochCount           : Integer;

        SurveyedSurfaces FilteredBaseSurveyedSurfaces = new SurveyedSurfaces();
        SurveyedSurfaces FilteredTopSurveyedSurfaces = new SurveyedSurfaces();

        public long RequestDescriptor { get; set; } = -1;

        public abstract bool ComputeVolumeInformation();

        private void ConfigurePipeline(SubGridPipelineAggregative<SubGridsRequestArgument, SimpleVolumesResponse> PipeLine,
                                       out BoundingIntegerExtent2D CellExtents)
        {
            CellExtents = BoundingIntegerExtent2D.Inverted();

            //PipeLine.TimeToLiveSeconds := VLPDSvcLocations.VLPDPSNode_VolumePipelineTTLSeconds;
            PipeLine.RequestDescriptor = RequestDescriptor;
            //PipeLine.ExternalDescriptor := FExternalDescriptor;

            PipeLine.DataModelID = SiteModel.ID;

            // Readd when logging available
            //SIGLogMessage.PublishNoODS(Self, Format('Volume calculation extents for DM=%d, Request=%d: %s', [FDataModelID, FRequestDescriptor, FExtents.AsText]), slmcDebug);
            PipeLine.WorldExtents.Assign(Extents);

            PipeLine.OverallExistenceMap = OverallExistenceMap;
            PipeLine.ProdDataExistenceMap = ProdDataExistenceMap;
            PipeLine.DesignSubgridOverlayMap = DesignSubgridOverlayMap;

            // PipeLine.LiftBuildSettings := FLiftBuildSettings;

            // Construct and assign the filter set into the pipeline
            FilterSet FilterSet = null;

            if (Aggregator.VolumeType == VolumeComputationType.Between2Filters)
            {
                FilterSet = new FilterSet(new CombinedFilter[] { BaseFilter, TopFilter });
            }
            else if (Aggregator.VolumeType == VolumeComputationType.BetweenDesignAndFilter)
            {
                FilterSet = new FilterSet(new CombinedFilter[] { TopFilter });
            }
            else
            {
                FilterSet = new FilterSet(new CombinedFilter[] { BaseFilter });
            };

            PipeLine.FilterSet = FilterSet;

/* TODO Ignite POC: These quantities are not required for the activity of the pipeline proper...
            if (ActiveDesign != null)
            {
                PipeLine.ReferenceDesign = ActiveDesign.DesignDescriptor;
            }
            PipeLine.ReferenceVolumeType = Aggregator.VolumeType;
*/

            PipeLine.GridDataType = GridDataType.Height;

            if (FilteredTopSurveyedSurfaces.Count > 0 || FilteredBaseSurveyedSurfaces.Count > 0)
            {
                PipeLine.IncludeSurveyedSurfaceInformation = true;
            }
        }

        public RequestErrorStatus ExecutePipeline()
        {
            PipelinedSubGridTask PipelinedTask = null;
            SubGridPipelineAggregative<SubGridsRequestArgument, SimpleVolumesResponse> PipeLine = null;

            BoundingIntegerExtent2D CellExtents;
            RequestErrorStatus Result = RequestErrorStatus.Unknown;

            bool PipelineAborted = false;
            // bool ShouldAbortDueToCompletedEventSet  = false;

            try
            {
                ProdDataExistenceMap = SiteModel.ExistanceMap; // ASNodeImplInstance.IOService.GetSubgridExistanceMap(FDataModelID, kSubGridTreeDimension * CellSize, FReturnCode);

                if (ProdDataExistenceMap == null)
                {
                    return RequestErrorStatus.FailedToRequestSubgridExistenceMap;
                }

                try
                {
                    if (ActiveDesign != null && (Aggregator.VolumeType == VolumeComputationType.BetweenFilterAndDesign || Aggregator.VolumeType == VolumeComputationType.BetweenDesignAndFilter))
                    {
                        if (ActiveDesign == null || ActiveDesign.DesignDescriptor.IsNull)
                        {
                            // TODO Readd when logging available
                            //SIGLogMessage.PublishNoODS(Self, Format('No design provided to prod data/design volumes calc for datamodel %d', [FDataModelID]), slmcError);
                            return RequestErrorStatus.NoDesignProvided;
                        }

                        DesignSubgridOverlayMap = ExistenceMaps.ExistenceMaps.GetSingleExistenceMap(SiteModel.ID, ExistenceMaps.Consts.EXISTANCE_MAP_DESIGN_DESCRIPTOR, ActiveDesign.ID);

                        if (DesignSubgridOverlayMap == null)
                        {
                            return RequestErrorStatus.NoDesignProvided;
                        }
                    }

                    OverallExistenceMap = new SubGridTreeSubGridExistenceBitMask();

                    // Work out the surveyed surfaces and coverage areas that need to be taken into account

                    SurveyedSurfaces SurveyedSurface = SiteModel.SurveyedSurfaces;

                    // See if we need to handle surveyed surface data for 'base'
                    // Filter out any surveyed surfaces which don't match current filter (if any) - realistically, this is time filters we're thinking of here
                    if (Aggregator.VolumeType == VolumeComputationType.Between2Filters || Aggregator.VolumeType == VolumeComputationType.BetweenFilterAndDesign)
                    {
                        if (!SurfaceFilterUtilities.ProcessSurveyedSurfacesForFilter(SiteModel.ID, SurveyedSurface, BaseFilter, FilteredTopSurveyedSurfaces, FilteredBaseSurveyedSurfaces, OverallExistenceMap))
                        {
                            return RequestErrorStatus.Unknown;
                        }
                    }

                    // See if we need to handle surveyed surface data for 'top'
                    // Filter out any surveyed surfaces which don't match current filter (if any) - realistically, this is time filters we're thinking of here
                    if (Aggregator.VolumeType == VolumeComputationType.Between2Filters || Aggregator.VolumeType == VolumeComputationType.BetweenDesignAndFilter)
                    {
                        if (!SurfaceFilterUtilities.ProcessSurveyedSurfacesForFilter(SiteModel.ID, SurveyedSurface, TopFilter, FilteredBaseSurveyedSurfaces, FilteredTopSurveyedSurfaces, OverallExistenceMap))
                        {
                            return RequestErrorStatus.Unknown;
                        }
                    }

                    // Add in the production data existance map to the computed surveyed surfaces existance maps
                    OverallExistenceMap.SetOp_OR(ProdDataExistenceMap);

                    // If necessary, impose spatial constraints from Lift filter design(s)
                    if (Aggregator.VolumeType == VolumeComputationType.Between2Filters || Aggregator.VolumeType == VolumeComputationType.BetweenFilterAndDesign)
                    {
                        if (!DesignFilterUtilities.ProcessDesignElevationsForFilter(SiteModel.ID, BaseFilter, OverallExistenceMap))
                        {
                            return RequestErrorStatus.Unknown;
                        }
                    }

                    if (Aggregator.VolumeType == VolumeComputationType.Between2Filters || Aggregator.VolumeType == VolumeComputationType.BetweenDesignAndFilter)
                    {
                        if (!DesignFilterUtilities.ProcessDesignElevationsForFilter(SiteModel.ID, TopFilter, OverallExistenceMap))
                        {
                            return RequestErrorStatus.Unknown;
                        }
                    }

                    PipelinedTask = new SimpleVolumesComputationTask(Aggregator);

                    try
                    {
                        PipeLine = new SubGridPipelineAggregative<SubGridsRequestArgument, SimpleVolumesResponse>(0, PipelinedTask);

                        PipelinedTask.PipeLine = PipeLine;

                        ConfigurePipeline(PipeLine, out CellExtents);

                        if (PipeLine.Initiate())
                        {
                            PipeLine.WaitForCompletion();
                        }

                        /*
                        while not FPipeLine.AllFinished and not FPipeLine.PipelineAborted do
                          begin
                            WaitResult := FPipeLine.CompleteEvent.WaitFor(5000);

                            if VLPDSvcLocations.Debug_EmitSubgridPipelineProgressLogging then
                              begin
                                if ((FEpochCount > 0) or (FPipeLine.SubmissionNode.TotalNumberOfSubgridsScanned > 0)) and
                                   ((FPipeLine.OperationNode.NumPendingResultsReceived > 0) or (FPipeLine.OperationNode.OustandingSubgridsToOperateOn > 0)) then
                                  SIGLogMessage.PublishNoODS(Self, Format('%s: Pipeline (request %d, model %d): #Progress# - Scanned = %d, Submitted = %d, Processed = %d (with %d pending and %d results outstanding)',
                                                                          [Self.ClassName,
                                                                           FRequestDescriptor, FPipeline.DataModelID,
                                                                           FPipeLine.SubmissionNode.TotalNumberOfSubgridsScanned,
                                                                           FPipeLine.SubmissionNode.TotalSumbittedSubgridRequests,
                                                                           FPipeLine.OperationNode.TotalOperatedOnSubgrids,
                                                                           FPipeLine.OperationNode.NumPendingResultsReceived,
                                                                           FPipeLine.OperationNode.OustandingSubgridsToOperateOn]), slmcDebug);
                              end;

                            if (WaitResult = wrSignaled) and not FPipeLine.AllFinished and not FPipeLine.PipelineAborted and not FPipeLine.Terminated then
                              begin
                                if ShouldAbortDueToCompletedEventSet then
                                  begin
                                    if (FPipeLine.OperationNode.NumPendingResultsReceived > 0) or (FPipeLine.OperationNode.OustandingSubgridsToOperateOn > 0) then
                                      SIGLogMessage.PublishNoODS(Self, Format('%s: Pipeline (request %d, model %d) being aborted as it''s completed event has remained set but still has work to do (%d outstanding subgrids, %d pending results to process) over a sleep epoch',
                                                                            [Self.ClassName,
                                                                             FRequestDescriptor, FPipeline.DataModelID,
                                                                             FPipeLine.OperationNode.OustandingSubgridsToOperateOn,
                                                                             FPipeLine.OperationNode.NumPendingResultsReceived]), slmcError);
                                    FPipeLine.Abort;
                                    ASNodeImplInstance.AsyncResponder.ASNodeResponseProcessor.PerformTaskCancellation(FPipelinedTask);
                                    Exit;
                                  end
                                else
                                  begin
                                    if (FPipeLine.OperationNode.NumPendingResultsReceived > 0) or (FPipeLine.OperationNode.OustandingSubgridsToOperateOn > 0) then
                                      SIGLogMessage.PublishNoODS(Self, Format('%s: Pipeline (request %d, model %d) has it''s completed event set but still has work to do (%d outstanding subgrids, %d pending results to process)',
                                                                            [Self.ClassName,
                                                                             FRequestDescriptor, FPipeline.DataModelID,
                                                                             FPipeLine.OperationNode.OustandingSubgridsToOperateOn,
                                                                             FPipeLine.OperationNode.NumPendingResultsReceived]), slmcDebug);
                                    Sleep(500);
                                    ShouldAbortDueToCompletedEventSet := True;
                                  end;
                              end;

                            if FPipeLine.TimeToLiveExpired then
                              begin
                                FAbortedDueToTimeout := True;
                                FPipeLine.Abort;
                                ASNodeImplInstance.AsyncResponder.ASNodeResponseProcessor.PerformTaskCancellation(FPipelinedTask);

                                // The pipeline has exceed its allotted time to complete. It will now
                                // be aborted and this request will be failed.
                                SIGLogMessage.PublishNoODS(Self, Format('%s: Pipeline (request %d) aborted due to time to live expiration (%d seconds)',
                                                                        [Self.ClassName, FRequestDescriptor, FPipeLine.TimeToLiveSeconds]), slmcError);
                                Exit;
                              end;
                         */

                        PipelineAborted = PipeLine.Aborted;

                        if (!PipeLine.Terminated && !PipeLine.Aborted)
                        {
                            Result = RequestErrorStatus.OK;
                        }
                    }
                    finally
                    {
                        if (AbortedDueToTimeout)
                        {
                            Result = RequestErrorStatus.AbortedDueToPipelineTimeout;
                        }
                        else
                        {
                            if (PipelinedTask.IsCancelled || PipelineAborted)
                            {
                                Result = RequestErrorStatus.RequestHasBeenCancelled;
                            }
                        }
                    }
                }
                catch (Exception E)
                {
                    Log.ErrorFormat("ExecutePipeline raised exception '{0}'", E);
                }

                return Result;
            }
            catch // (Exception E)
            {
                // Readd when logging available
                //SIGLogMessage.PublishNoODS(Self, Format('%s.Execute raised exception ''%s''', [Self.Classname, E.Message]), slmcException);
            }

            return RequestErrorStatus.Unknown;
        }

        /// <summary>
        /// VolumesCalculator extends VolumesCalculatorBase to include volume
        /// accumulation state tracking for simple volums calculations.
        /// </summary>
        public class VolumesCalculator : VolumesCalculatorBase
        {
            /// <summary>
            /// Default no-arg constructor
            /// </summary>
            public VolumesCalculator()
            {

            }

            private void ApplyFilterAndSubsetBoundariesToExtents()
            {
                /*
                 * if VLPDSvcLocations.Debug_ExtremeLogSwitchM then
                   begin
                     SIGLogMessage.Publish(Self, Format('TICVolumesCalculator DEBUG  BaseFilter Dates : %s  %s ',[DateTimeToStr(FBaseFilter.StartTime), DateTimeToStr(FBaseFilter.EndTime)]), slmcDebug);
                     SIGLogMessage.Publish(Self, Format('TICVolumesCalculator DEBUG  TopFilter Dates : %s  %s ',[DateTimeToStr(FTopFilter.StartTime), DateTimeToStr(FTopFilter.EndTime)]), slmcDebug);
                   end;
              */

                if (FromSelectionType == ProdReportSelectionType.Filter)
                {
                    Extents = BaseFilter.SpatialFilter.CalculateIntersectionWithExtents(Extents);
                }

                if (ToSelectionType == ProdReportSelectionType.Filter)
                {
                    Extents = TopFilter.SpatialFilter.CalculateIntersectionWithExtents(Extents);
                }
            }

            public override bool ComputeVolumeInformation()
            {
                if (Aggregator.VolumeType == VolumeComputationType.None)
                {
                    // TODO Readd when logging available
                    //SIGLogMessage.Publish(Self, 'No report type supplied to TICVolumesCalculator.ComputeVolumeInformation', slmcError);
                    return false;
                }

                if (FromSelectionType == ProdReportSelectionType.Surface)
                {
                    if (RefOriginal == null)
                    {
                        // TODO Readd when logging available
                        // SIGLogMessage.Publish(Self, 'No RefOriginal surface supplied', slmcError);
                        return false;
                    }
                }

                if (ToSelectionType == ProdReportSelectionType.Surface)
                {
                    if (RefDesign == null)
                    {
                        // TODO: Readd when logging available
                        //SIGLogMessage.Publish(Self, 'No RefDesign surface supplied', slmcError);
                        return false;
                    }
                }

                // Adjust the extents we have been given to encompass the spatial extent
                // of the supplied filters (if any);
                ApplyFilterAndSubsetBoundariesToExtents();

                BaseFilter.AttributeFilter.ReturnEarliestFilteredCellPass = UseEarliestData;

                // Configure our local spatial filter from the spatial constraints information in the base and top.

                /*
                 if (FFromSelectionType = prstFilter then FBaseSpatialFilter.Initialise(FBaseFilter);
                 if FToSelectionType = prstFilter then FTopSpatialFilter.Initialise(FTopFilter);
                */

                if (FromSelectionType == ProdReportSelectionType.Surface)
                {
                    ActiveDesign = RefOriginal;
                }
                else
                {
                    ActiveDesign = ToSelectionType == ProdReportSelectionType.Surface ? RefDesign : null;
                }

                // Assign the active design into the aggregator for use
                Aggregator.ActiveDesign = ActiveDesign;

                // Compute the volume as required
                return ExecutePipeline() == RequestErrorStatus.OK;
            }
        }
    }
}
