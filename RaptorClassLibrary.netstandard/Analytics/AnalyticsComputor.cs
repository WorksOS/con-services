using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Common;
using VSS.VisionLink.Raptor.Executors.Tasks;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.GridFabric.Responses;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.Pipelines;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Analytics
{
    /// <summary>
    /// The base class the implements the analytics computation framework 
    /// </summary>
    public class AnalyticsComputor<TArgument, SubGridsRequestResponse>
        where TArgument : BaseApplicationServiceRequestArgument
        where SubGridsRequestResponse : class, new()
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The Aggregator to use for calculation of analytics
        /// </summary>
        public ISubGridRequestsAggregator Aggregator { get; set; } = null;

        /// <summary>
        /// The Sitemodel from which the volume is being calculated
        /// </summary>
        public SiteModel SiteModel { get; set; } = null;

        /// <summary>
        /// The cell size to be used in the calculation
        /// </summary>
        public double CellSize { get; set; } = Consts.NullDouble;

        private SubGridTreeSubGridExistenceBitMask ProdDataExistenceMap = null;
        private SubGridTreeSubGridExistenceBitMask OverallExistenceMap = null;
        private SubGridTreeSubGridExistenceBitMask CutFillDesignExistenceMap = null;

        /// <summary>
        /// Identifier for the design to be used as the basis for any required cut fill operations
        /// </summary>
        public long CutFillDesignID { get; set; } = long.MinValue;

        /// <summary>
        /// The underlying grid data type required to satisfy the processing requirements of this analytics computor
        /// </summary>
        public GridDataType RequestedGridDataType { get; set; } = GridDataType.All;

        /// <summary>
        ///  Default no-arg constructor
        /// </summary>
        public AnalyticsComputor()
        {
        }

        protected BoundingWorldExtent3D Extents = BoundingWorldExtent3D.Inverted(); // No get;set; on purpose

        public long RequestDescriptor { get; set; } = -1;

        public CombinedFilter Filter { get; set; } = null;

        public bool AbortedDueToTimeout { get; set; } = false;

        public bool IncludeSurveyedSurfaces { get; set; } = false;

        protected void ApplyFilterAndSubsetBoundariesToExtents()
        {
            if (Filter != null)
                Extents = Filter.SpatialFilter.CalculateIntersectionWithExtents(Extents);
        }

        private bool ConfigurePipeline(SubGridPipelineAggregative<SubGridsRequestArgument, SubGridRequestsResponse> PipeLine,
                                       AggregatedPipelinedSubGridTask PipelinedTask,
                                       out BoundingIntegerExtent2D CellExtents)
        {
            CellExtents = BoundingIntegerExtent2D.Inverted();

            PipeLine.DataModelID = SiteModel.ID;
            PipeLine.RequestDescriptor = RequestDescriptor;
            PipeLine.IncludeSurveyedSurfaceInformation = IncludeSurveyedSurfaces;
            PipeLine.PipelineTask = PipelinedTask;
            PipeLine.GridDataType = RequestedGridDataType;

            Log.Debug($"Analytics computor extents for DM={SiteModel.ID}: {Extents}");

            PipeLine.WorldExtents.Assign(Extents);

            OverallExistenceMap = new SubGridTreeSubGridExistenceBitMask();
            OverallExistenceMap.SetOp_OR(ProdDataExistenceMap);

            // if CutFill request there will be a design assigned so get existance map to assign
            if (CutFillDesignID != long.MinValue)
            {
                CutFillDesignExistenceMap = ExistenceMaps.ExistenceMaps.GetSingleExistenceMap(SiteModel.ID, ExistenceMaps.Consts.EXISTANCE_MAP_DESIGN_DESCRIPTOR, CutFillDesignID);

                if (CutFillDesignExistenceMap != null)
                {
                    PipeLine.DesignSubgridOverlayMap = CutFillDesignExistenceMap;
                }
                else
                {
                    // TODO Readd when logging available...
                    // SIGLogMessage.PublishNoODS(Self, Format('Failed to request subgrid overlay index for design %s in datamodel %d (error %s)',
                    //                            [FAggregateState.CutFillSettings.DesignDescriptor.ToString, SiteModel.ID, DesignProfilerErrorStatusName(DesignProfilerResult)]), slmcError);
                    return false;
                }
            }

            if (Filter != null && Filter.AttributeFilter != null)
            {
                if (Filter.AttributeFilter.HasElevationRangeFilter && (Filter.AttributeFilter.ElevationRangeDesignID != long.MinValue))
                {
                    SubGridTreeSubGridExistenceBitMask LiftDesignSubgridOverlayMap = ExistenceMaps.ExistenceMaps.GetSingleExistenceMap(SiteModel.ID, ExistenceMaps.Consts.EXISTANCE_MAP_DESIGN_DESCRIPTOR, Filter.AttributeFilter.ElevationRangeDesignID);

                    if (LiftDesignSubgridOverlayMap != null)
                        OverallExistenceMap.SetOp_AND(LiftDesignSubgridOverlayMap);
                }
            }

            PipeLine.OverallExistenceMap = OverallExistenceMap;
            PipeLine.ProdDataExistenceMap = ProdDataExistenceMap;

            // TODO Readd when lift build settings are supported
            // PipeLine.LiftBuildSettings = FLiftBuildSettings;

            PipeLine.FilterSet = new FilterSet(new CombinedFilter[] { Filter });

            return true;
        }

        public RequestErrorStatus ExecutePipeline()
        {
            AggregatedPipelinedSubGridTask PipelinedTask = null;
            RequestErrorStatus Result = RequestErrorStatus.Unknown;
            bool PipelineAborted = false;
            // WaitResult            : TWaitResult;
            // bool ShouldAbortDueToCompletedEventSet  = false;

            try
            {
                // Retrieve the existence map for the datamodel
                ProdDataExistenceMap = SiteModel.ExistanceMap;

                if (ProdDataExistenceMap == null)
                    return RequestErrorStatus.FailedToRequestSubgridExistenceMap;

                try
                {
                    // PipeLine is the subgrid pipeline used to drive the extraction of subgrid information
                    // for the purposes of computing volumes information
                    SubGridPipelineAggregative<SubGridsRequestArgument, SubGridRequestsResponse> PipeLine = new SubGridPipelineAggregative<SubGridsRequestArgument, SubGridRequestsResponse>(0, PipelinedTask);

                    PipelinedTask = new AggregatedPipelinedSubGridTask(Aggregator)
                    {
                        PipeLine = PipeLine
                    };

                    if (!ConfigurePipeline(PipeLine, PipelinedTask, out BoundingIntegerExtent2D CellExtents))
                    {
                        // TODO: Set some kind of failure mode into result
                        return RequestErrorStatus.FailedToConfigureInternalPipeline;
                    }

                    // Start the pipeline processing it's work and wait for it to complete
                    if (PipeLine.Initiate())
                    {
                        PipeLine.WaitForCompletion();
                    }

                    /* TODO ????
                              FEpochCount := 0;
                              while not FPipeLine.AllFinished and not FPipeline.PipelineAborted do
                                begin
                                  WaitResult := FPipeLine.CompleteEvent.WaitFor(5000);

                                  if VLPDSvcLocations.Debug_EmitSubgridPipelineProgressLogging then
                                    begin
                                      if ((FEpochCount > 0) or(FPipeLine.SubmissionNode.TotalNumberOfSubgridsScanned > 0)) and
                                        ((FPipeLine.OperationNode.NumPendingResultsReceived >0) or(FPipeLine.OperationNode.OustandingSubgridsToOperateOn > 0)) then
                                         SIGLogMessage.PublishNoODS(Self, Format('%s: Pipeline (request %d, model %d): #Progress# - Scanned = %d, Submitted = %d, Processed = %d (with %d pending and %d results outstanding)',
                                                                              [Self.ClassName,
                                                                                 FRequestDescriptor, FPipeLine.DataModelID,
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
                                          if (FPipeLine.OperationNode.NumPendingResultsReceived >0) or(FPipeLine.OperationNode.OustandingSubgridsToOperateOn > 0) then
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
                                          if (FPipeLine.OperationNode.NumPendingResultsReceived >0) or(FPipeLine.OperationNode.OustandingSubgridsToOperateOn > 0) then
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
                    //              Inc(FEpochCount);
                    */

                    PipelineAborted = PipeLine.Aborted;

                    if (!PipeLine.Terminated && !PipeLine.Aborted)
                        Result = RequestErrorStatus.OK;
                }
                finally
                {
                    if (AbortedDueToTimeout)
                        Result = RequestErrorStatus.AbortedDueToPipelineTimeout;
                    else
                        if (PipelinedTask.IsCancelled || PipelineAborted)
                        Result = RequestErrorStatus.RequestHasBeenCancelled;
                }
            }
            catch (Exception E)
            {
                Log.ErrorFormat("ExecutePipeline raised exception '{0}'", E);
            }

            return Result;
        }

        public bool ComputeAnalytics()
        {
            // TODO: add when lift build setting ssupported
            // FAggregateState.LiftBuildSettings := FLiftBuildSettings;
            Extents.SetMaximalCoverage();

            // Adjust the extents we have been given to encompass the spatial extent of the supplied filters (if any);
            ApplyFilterAndSubsetBoundariesToExtents();

            // Compute the report as required
            return ExecutePipeline() == RequestErrorStatus.OK;
        }
    }
}
