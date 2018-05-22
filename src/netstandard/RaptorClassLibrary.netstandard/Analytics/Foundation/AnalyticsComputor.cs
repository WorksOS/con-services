using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using VSS.TRex.Common;
using VSS.TRex.Executors.Tasks;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Interfaces;
using VSS.TRex.Pipelines;
using VSS.TRex.SiteModels;
using VSS.TRex.SubGridTrees;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics
{
    /// <summary>
    /// The base class the implements the analytics computation framework 
    /// </summary>
    public class AnalyticsComputor
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// The Aggregator to use for calculation of analytics
        /// </summary>
        public ISubGridRequestsAggregator Aggregator { get; set; }

        /// <summary>
        /// The Sitemodel from which the volume is being calculated
        /// </summary>
        public SiteModel SiteModel { get; set; }

        /// <summary>
        /// The cell size to be used in the calculation
        /// </summary>
        public double CellSize { get; set; } = Consts.NullDouble;

        /// <summary>
        /// Identifier for the design to be used as the basis for any required cut fill operations
        /// </summary>
        public Guid CutFillDesignID { get; set; } = Guid.Empty;

        /// <summary>
        /// The underlying grid data type required to satisfy the processing requirements of this analytics computor
        /// </summary>
        public GridDataType RequestedGridDataType { get; set; } = GridDataType.All;

        public BoundingWorldExtent3D Extents = BoundingWorldExtent3D.Inverted(); // No get;set; on purpose

        private SubGridTreeSubGridExistenceBitMask ProdDataExistenceMap;
        private SubGridTreeSubGridExistenceBitMask OverallExistenceMap;
        private SubGridTreeSubGridExistenceBitMask CutFillDesignExistenceMap;
        private SubGridPipelineAggregative<SubGridsRequestArgument, SubGridRequestsResponse> PipeLine;
        private AggregatedPipelinedSubGridTask PipelinedTask;

        /// <summary>
        ///  Default no-arg constructor
        /// </summary>
        public AnalyticsComputor()
        {
        }

        public long RequestDescriptor { get; set; } = -1;

        public FilterSet Filters { get; set; }

        public bool AbortedDueToTimeout { get; set; } = false;

        public bool IncludeSurveyedSurfaces { get; set; }

        private bool ConfigurePipeline(out BoundingIntegerExtent2D CellExtents)
        {
            CellExtents = BoundingIntegerExtent2D.Inverted();

            PipeLine.DataModelID = SiteModel.ID;
            PipeLine.RequestDescriptor = RequestDescriptor;
            PipeLine.IncludeSurveyedSurfaceInformation = IncludeSurveyedSurfaces;
            PipeLine.PipelineTask = PipelinedTask;
            PipeLine.GridDataType = RequestedGridDataType;
            PipeLine.CutFillDesignID = CutFillDesignID;

            Log.LogDebug($"Analytics computor extents for DM={SiteModel.ID}: {Extents}");

            PipeLine.WorldExtents.Assign(Extents);

            OverallExistenceMap = new SubGridTreeSubGridExistenceBitMask();
            OverallExistenceMap.SetOp_OR(ProdDataExistenceMap);

            // if CutFill request there will be a design assigned so get existance map to assign
            if (CutFillDesignID != Guid.Empty)
            {
                CutFillDesignExistenceMap = ExistenceMaps.ExistenceMaps.GetSingleExistenceMap(SiteModel.ID, ExistenceMaps.Consts.EXISTANCE_MAP_DESIGN_DESCRIPTOR, CutFillDesignID);

                if (CutFillDesignExistenceMap != null)
                {
                    PipeLine.DesignSubgridOverlayMap = CutFillDesignExistenceMap;
                }
                else
                {
                    Log.LogError($"Failed to request subgrid overlay index for design {CutFillDesignID} in datamodel {SiteModel.ID}");
                    return false;
                }
            }

          foreach (var filter in Filters.Filters)
            if (filter?.AttributeFilter != null)
            {
                if (filter.AttributeFilter.HasElevationRangeFilter && (filter.AttributeFilter.ElevationRangeDesignID != Guid.Empty))
                {
                    SubGridTreeSubGridExistenceBitMask LiftDesignSubgridOverlayMap = 
                      ExistenceMaps.ExistenceMaps.GetSingleExistenceMap(SiteModel.ID, ExistenceMaps.Consts.EXISTANCE_MAP_DESIGN_DESCRIPTOR, filter.AttributeFilter.ElevationRangeDesignID);

                    if (LiftDesignSubgridOverlayMap != null)
                        OverallExistenceMap.SetOp_AND(LiftDesignSubgridOverlayMap);
                }
            }

            PipeLine.OverallExistenceMap = OverallExistenceMap;
            PipeLine.ProdDataExistenceMap = ProdDataExistenceMap;

            // TODO Readd when lift build settings are supported
            // PipeLine.LiftBuildSettings = FLiftBuildSettings;

            PipeLine.FilterSet = Filters; //new FilterSet(new [] { Filter });

            return true;
        }

        private RequestErrorStatus ExecutePipeline()
        {
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
                    PipeLine = new SubGridPipelineAggregative<SubGridsRequestArgument, SubGridRequestsResponse>(/*0, */PipelinedTask);

                    PipelinedTask = new AggregatedPipelinedSubGridTask(Aggregator)
                    {
                        PipeLine = PipeLine
                    };

                    if (!ConfigurePipeline(out BoundingIntegerExtent2D _ /*CellExtents*/))
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
              Log.LogError($"ExecutePipeline raised exception: {E}");
            }

            return Result;
        }

        /// <summary>
        /// Primary method called to begin analytics computation
        /// </summary>
        /// <returns></returns>
        public bool ComputeAnalytics()
        {
            // TODO: add when lift build setting ssupported
            // FAggregateState.LiftBuildSettings := FLiftBuildSettings;
            Extents.SetMaximalCoverage();

            // Adjust the extents we have been given to encompass the spatial extent of the supplied filters (if any);
            Filters.ApplyFilterAndSubsetBoundariesToExtents(ref Extents);

            // Compute the report as required
            return ExecutePipeline() == RequestErrorStatus.OK;
        }
    }
}
