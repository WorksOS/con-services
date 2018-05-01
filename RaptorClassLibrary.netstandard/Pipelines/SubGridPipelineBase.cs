using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using VSS.VisionLink.Raptor.Executors.Tasks;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.GridFabric.Responses;
using VSS.VisionLink.Raptor.GridFabric.Types;
using VSS.VisionLink.Raptor.Pipelines.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Pipelines
{
    /// <summary>
    /// Derived from TSVOICSubGridPipelineBase = class(TObject)
    /// </summary>
    public class SubGridPipelineBase<TSubGridsRequestArgument, TSubGridRequestsResponse, TSubGridRequestor> : ISubGridPipelineBase
        where TSubGridsRequestArgument : SubGridsRequestArgument, new()
        where TSubGridRequestsResponse : SubGridRequestsResponse, new()
        where TSubGridRequestor : SubGridRequestsBase<TSubGridsRequestArgument, TSubGridRequestsResponse>, new() 
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The event used to signal that the pipeline processing has completed, or aborted
        /// </summary>
        public AutoResetEvent PipelineSignalEvent { get; } = new AutoResetEvent(false);

        /// <summary>
        /// Records how may subgrid results there are pending to be processed through the Task assigned to this pipeline.
        /// As each subgrid result is processed in the task, the task pings the pipeline to note another task has been 
        /// completed. Once this count reaches zero the pipeline is cleared to complete its processing.
        /// </summary>
        private long SubgridsRemainingToProcess;

//        public int ID;
        public PipelinedSubGridTask PipelineTask;
        public bool Aborted { get; set; }

        public uint TimeToLiveSeconds = 0;
        private DateTime TimeToLiveExpiryTime = DateTime.MaxValue;

        // public FExternalDescriptor: TASNodeRequestDescriptor;

        // FMaximumOutstandingSubgridRequests : Integer;

        public long DataModelID { get; set; } = -1;

        /// <summary>
        /// OverallExistenceMap is the map which describes the combination of Prod Data and Surveyed Surfaces
        /// </summary>
        public SubGridTreeSubGridExistenceBitMask OverallExistenceMap { get; set; }

        /// <summary>
        /// ProdDataExistenceMap is the subgrid existence map for the data model referenced by FDataModelID
        /// </summary>
        public SubGridTreeSubGridExistenceBitMask ProdDataExistenceMap { get; set; }

        public bool IncludeSurveyedSurfaceInformation = true;

        /// <summary>
        /// DesignSubgridOverlayMap is the subgrid index for subgrids that cover the design being related to for cut/fill operations
        /// </summary>
        public SubGridTreeSubGridExistenceBitMask DesignSubgridOverlayMap { get; set; }

        /// <summary>
        /// The set of filters to be made available to the subgrid processing for this request
        /// </summary>
        public FilterSet FilterSet { get; set; }

        public int MaxNumberOfPassesToReturn = 0;

        public long CutFillDesignID { get; set; } = long.MinValue;

        // public         FNoChangeVolumeTolerance : Single;

        public AreaControlSet AreaControlSet;

        public long RequestDescriptor = -1;

        public bool Terminated = false;

        protected bool pipelineCompleted;

        public bool PipelineCompleted {
            get
            {
                return pipelineCompleted;
            }
            set
            {
                pipelineCompleted = value;

                // The pipeline has been signalled as complete so set its completion signal
                // Don't modify AllFinished as all results may not have been received/processed before completion
                PipelineSignalEvent.Set();
            }
        }

        // FLiftBuildSettings is a reference to a lift build settings object provided by the caller
        //  property LiftBuildSettings : TICLiftBuildSettings read FLiftBuildSettings write FLiftBuildSettings;
        //  property Terminated : Boolean read FTerminated;

        public GridDataType GridDataType = GridDataType.All;

        public BoundingWorldExtent3D WorldExtents = BoundingWorldExtent3D.Inverted();
        public BoundingIntegerExtent2D OverrideSpatialCellRestriction = BoundingIntegerExtent2D.Inverted();

        /// <summary>
        /// Have all subgrids in the request been returned and processed?
        /// </summary>
        public bool AllFinished;

        private void AllSubgridsProcessed()
        {
            AllFinished = true;
            PipelineSignalEvent.Set();
        }

        /// <summary>
        /// Advises that a single subgrid has been processed and can be removed from the tally of
        /// subgrids awaiting results. 
        /// This is typically used by progressive queries where a SubGridListener
        /// is reponsible for receiving and coordinating handling of subgrid results
        /// </summary>
        public void SubgridProcessed()
        {
            if (Interlocked.Decrement(ref SubgridsRemainingToProcess) <= 0)
            {
                AllSubgridsProcessed();
            }
        }

        /// <summary>
        /// Advises that a group of subgrids has been processed and can be removed from the tally of
        /// subgrids awaiting results.
        /// This is typically used by aggregative queries where the cache compute cluster aggregates
        /// subgrid results within each partition and returns pre-aggregated results
        /// </summary>
        public void SubgridsProcessed(long numProcessed)
        {
            if (Interlocked.Add(ref SubgridsRemainingToProcess, numProcessed) <= 0)
            {
                AllSubgridsProcessed();
            }
        }

        /// <summary>
        /// Constructor accepting an identifier for the pipeline and a task for the pipeline to operate with
        /// </summary>
        /// <param name="AID"></param>
        /// <param name="task"></param>
        public SubGridPipelineBase(int AID, PipelinedSubGridTask task)
        {
            PipelineTask = task;
            //ID = AID;

            // FMaxNumberOfPassesToReturn:= VLPDSvcLocations.VLPDPSNode_MaxCellPassIterationDepth_PassCountDetailAndSummary;

            // FNoChangeVolumeTolerance:= 0;

            // FPixelXWorldSize := 0.0;
            // FPixelYWorldSize := 0.0;

            AreaControlSet = AreaControlSet.Null();

            // FLiftBuildSettings:= Nil;

            // FTimeToLiveSeconds:= kDefaultSubgridPipelineTimeToLiveSeconds;

            // FMaximumOutstandingSubgridRequests:= VLPDSvcLocations.VLPDASNode_DefaultMaximumOutstandingSubgridRequestsInPipeline;
        }

        /// <summary>
        /// Signals to the running pipeline that its operation has been aborted
        /// </summary>
        public virtual void Abort()
        {
            Aborted = true;

            PipelineSignalEvent.Set();
        }

        // procedure Terminate; Virtual;
        // Function TimeToLiveExpired : Boolean;

        // AbortAndShutdown instructs the pipeline to abandon operations and abort itself.
        // It is used by the pipeline monitor shutdown pipelines when the service is
        // being restarted.
        // Procedure AbortAndShutdown;

        /// <summary>
        /// Orchestrates sending subgrid requests to the compute cluster and handling the result
        /// </summary>
        /// <returns></returns>
        public virtual bool Initiate()
        {            
            // First analyse the request to determine the set of subgrids that will need to be requested
            RequestAnalyser analyser = new RequestAnalyser(this, WorldExtents);
            if (!analyser.Execute())
            {
                // Leave gracefully...
                return false;
            }

            SubgridsRemainingToProcess = analyser.TotalNumberOfSubgridsAnalysed;

            Log.InfoFormat("Request analyser counts {0} subgrids to be requested, compared to {1} subgrids in production existance map", analyser.TotalNumberOfSubgridsAnalysed, OverallExistenceMap.CountBits());

            if (analyser.TotalNumberOfSubgridsAnalysed == 0)
            {
                // There are no subgrids to be requested, leave quietly
                Log.InfoFormat("No subgrids analysed from request to be submitted to processing engine");

                return false;
            }

            Log.Info($"START: Request for {analyser.TotalNumberOfSubgridsAnalysed} subgrids");

            // Send the subgrid request mask to the grid fabric layer for processing
            TSubGridRequestor gridFabricRequest = new TSubGridRequestor()
            {
                Task = PipelineTask,
                SiteModelID = DataModelID,
                RequestID = PipelineTask.RequestDescriptor,
                RaptorNodeID = PipelineTask.RaptorNodeID,
                RequestedGridDataType = GridDataType,
                IncludeSurveyedSurfaceInformation = IncludeSurveyedSurfaceInformation,
                ProdDataMask = analyser.ProdDataMask,
                SurveyedSurfaceOnlyMask = analyser.SurveydSurfaceOnlyMask,
                Filters = FilterSet,
                CutFillDesignID = CutFillDesignID
            };

            ICollection<TSubGridRequestsResponse> Responses = gridFabricRequest.Execute();

            Log.Info($"COMPLETED: Request for {analyser.TotalNumberOfSubgridsAnalysed } subgrids");

            return Responses.All(x => x.ResponseCode == SubGridRequestsResponseResult.OK);
        }

        /// <summary>
        /// Waits until the set of requests injected into the pipeline have yielded all required results
        /// (passed into the relevant Task and signalled), or the pipeline timeout has expired
        /// </summary>
        public void WaitForCompletion()
        {
            if (PipelineSignalEvent.WaitOne(30000)) // Don't wait for more than two minutes...
            {
                Log.Info($"WaitForCompletion received signal with wait handle: {PipelineSignalEvent.SafeWaitHandle.GetHashCode()}");
            }
            else
            {
                // No signal was received, the wait timed out...
                Log.Info($"WaitForCompletion timed out with wait handle: {PipelineSignalEvent.SafeWaitHandle.GetHashCode()} and {SubgridsRemainingToProcess} subgrids remaining to be processed");
            }
        }
    }
}
