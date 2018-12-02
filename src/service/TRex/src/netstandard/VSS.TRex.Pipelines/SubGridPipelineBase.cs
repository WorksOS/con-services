using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using VSS.TRex.GridFabric.Models;
using VSS.TRex.Types;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.GridFabric.Responses;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SubGrids.GridFabric.Requests;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Pipelines
{
    /// <summary>
    /// Derived from SVO SubGridPipelineBase
    /// </summary>
    public class SubGridPipelineBase<TSubGridsRequestArgument, TSubGridRequestsResponse, TSubGridRequestor> : ISubGridPipelineBase
        where TSubGridsRequestArgument : SubGridsRequestArgument, new()
        where TSubGridRequestsResponse : SubGridRequestsResponse, new()
        where TSubGridRequestor : SubGridRequestsBase<TSubGridsRequestArgument, TSubGridRequestsResponse>, new() 
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// The event used to signal that the pipeline processing has completed, or aborted
        /// </summary>
        public SemaphoreSlim PipelineSignalEvent { get; } = new SemaphoreSlim(0, 1);

        /// <summary>
       /// Records how may subgrid results there are pending to be processed through the Task assigned to this pipeline.
        /// As each subgrid result is processed in the task, the task pings the pipeline to note another task has been 
        /// completed. Once this count reaches zero the pipeline is cleared to complete its processing.
        /// </summary>
        private long subgridsRemainingToProcess = 0;
        public long SubgridsRemainingToProcess => subgridsRemainingToProcess;

        public ITRexTask PipelineTask { get; set; }

        public bool Aborted { get; set; }

        public bool Terminated { get; set; }

        public uint TimeToLiveSeconds = 0;
        private DateTime TimeToLiveExpiryTime = DateTime.MaxValue;

        /// <summary>
        /// The request descriptor ID for this request
        /// </summary>
        public Guid RequestDescriptor { get; set; }

        // FMaximumOutstandingSubgridRequests : Integer;

        public Guid DataModelID { get; set; } = Guid.Empty;

        /// <summary>
        /// OverallExistenceMap is the map which describes the combination of Prod Data and Surveyed Surfaces
        /// </summary>
        public ISubGridTreeBitMask OverallExistenceMap { get; set; }

        /// <summary>
        /// ProdDataExistenceMap is the subgrid existence map for the data model referenced by FDataModelID
        /// </summary>
        public ISubGridTreeBitMask ProdDataExistenceMap { get; set; }

        /// <summary>
        /// Notes if the underlying query needs to include surveyed surface information in its results
        /// </summary>
        public bool IncludeSurveyedSurfaceInformation { get; set; } = true;

        /// <summary>
        /// DesignSubgridOverlayMap is the subgrid index for subgrids that cover the design being related to for cut/fill operations
        /// </summary>
        public ISubGridTreeBitMask DesignSubgridOverlayMap { get; set; }

        /// <summary>
        /// The set of filters to be made available to the subgrid processing for this request
        /// </summary>
        public IFilterSet FilterSet { get; set; }

        public int MaxNumberOfPassesToReturn = 0;

        public Guid ReferenceDesignID { get; set; } = Guid.Empty;

        // public float FNoChangeVolumeTolerance;

        public AreaControlSet AreaControlSet { get; set;  }

        private bool pipelineCompleted;

        public bool PipelineCompleted {
            get => pipelineCompleted;
            set
            {
                pipelineCompleted = value;

                // The pipeline has been signaled as complete so set its completion signal
                // Don't modify AllFinished as all results may not have been received/processed before completion
                if (PipelineSignalEvent.CurrentCount == 0)
                    PipelineSignalEvent.Release();
            }
        }

        // FLiftBuildSettings is a reference to a lift build settings object provided by the caller
        //  property LiftBuildSettings : TICLiftBuildSettings read FLiftBuildSettings write FLiftBuildSettings;
        //  property Terminated : Boolean read FTerminated;
      
        /// <summary>
        /// The type of grid data to be selected from the data model
        /// </summary>
        public GridDataType GridDataType { get; set; } = GridDataType.All;

        // public BoundingIntegerExtent2D OverrideSpatialCellRestriction { get; set; } = BoundingIntegerExtent2D.Inverted();
     
        /// <summary>
        /// Have all subgrids in the request been returned and processed?
        /// </summary>
         private bool AllFinished;

        /// <summary>
        /// The request analyzer to be used to identify the set of subgrids required for the request.
        /// If no analyzer is supplied then a default analyzer will be created as need by the pipeline
        /// </summary>
        public IRequestAnalyser RequestAnalyser { get; set; }

        private void AllSubgridsProcessed()
        {
            AllFinished = true;
            if (PipelineSignalEvent.CurrentCount == 0)
              PipelineSignalEvent.Release();
        }

        /// <summary>
        /// Advises that a single subgrid has been processed and can be removed from the tally of
        /// subgrids awaiting results. 
        /// This is typically used by progressive queries where a SubGridListener
        /// is responsible for receiving and coordinating handling of subgrid results
        /// </summary>
        public void SubgridProcessed()
        {
            if (Interlocked.Decrement(ref subgridsRemainingToProcess) <= 0)
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
            if (Interlocked.Add(ref subgridsRemainingToProcess, -numProcessed) <= 0)
            {
                AllSubgridsProcessed();
            }
        }

        public SubGridPipelineBase()
        {
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
        /// Constructor accepting an identifier for the pipeline and a task for the pipeline to operate with
        /// </summary>
        /// <param name="task"></param>
        public SubGridPipelineBase(ITRexTask task) : this()
        {
            PipelineTask = task;
        }

        /// <summary>
        /// Signals to the running pipeline that its operation has been aborted
        /// </summary>
        public virtual void Abort()
        {
            Aborted = true;

            if (PipelineSignalEvent.CurrentCount == 0)
              PipelineSignalEvent.Release();
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
        public bool Initiate()
        {
            // First analyze the request to determine the set of subgrids that will need to be requested
            if (!RequestAnalyser.Execute())
            {
                // Leave gracefully...
                return false;
            }

            subgridsRemainingToProcess = RequestAnalyser.TotalNumberOfSubgridsToRequest;

            Log.LogInformation($"Request analyzer counts {RequestAnalyser.TotalNumberOfSubgridsToRequest} subgrids to be requested, compared to {OverallExistenceMap.CountBits()} subgrids in production existence map");

            if (RequestAnalyser.TotalNumberOfSubgridsToRequest == 0)
            {
                // There are no subgrids to be requested, leave quietly
                Log.LogInformation("No subgrids analyzed from request to be submitted to processing engine");

                return false;
            }

            Log.LogInformation($"START: Request for {RequestAnalyser.TotalNumberOfSubgridsToRequest} subgrids");

            // Send the subgrid request mask to the grid fabric layer for processing
            var requestor = new TSubGridRequestor
            {
                TRexTask = PipelineTask,
                SiteModelID = DataModelID,
                RequestID = RequestDescriptor,
                TRexNodeId = PipelineTask.TRexNodeID,
                RequestedGridDataType = GridDataType,
                IncludeSurveyedSurfaceInformation = IncludeSurveyedSurfaceInformation,
                ProdDataMask = RequestAnalyser.ProdDataMask,
                SurveyedSurfaceOnlyMask = RequestAnalyser.SurveydSurfaceOnlyMask,
                Filters = FilterSet,
                ReferenceDesignID = ReferenceDesignID
            };

            var Response = requestor.Execute();

            Log.LogInformation($"COMPLETED: Request for {RequestAnalyser.TotalNumberOfSubgridsToRequest} subgrids");

            return Response.ResponseCode == SubGridRequestsResponseResult.OK;
        }

        /// <summary>
        /// Waits until the set of requests injected into the pipeline have yielded all required results
        /// (passed into the relevant Task and signaled), or the pipeline timeout has expired
        /// </summary>
        public Task<bool> WaitForCompletion()
        {
          // Todo: Make the 2 minute limit configurable
          return PipelineSignalEvent.WaitAsync(120000); // Don't wait for more than two minutes...
        }
    }
}
