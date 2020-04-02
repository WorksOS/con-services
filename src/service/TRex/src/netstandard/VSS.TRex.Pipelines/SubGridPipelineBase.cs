using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.GridFabric.Models;
using VSS.TRex.Types;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Pipelines.Interfaces;
using VSS.TRex.Pipelines.Interfaces.Tasks;
using VSS.TRex.SubGrids.GridFabric.Arguments;
using VSS.TRex.SubGrids.GridFabric.Requests;
using VSS.TRex.SubGrids.Responses;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Pipelines
{
    /// <summary>
    /// Derived from SVO SubGridPipelineBase
    /// </summary>
    public abstract class SubGridPipelineBase<TSubGridsRequestArgument, TSubGridRequestsResponse, TSubGridRequestor> : ISubGridPipelineBase
        where TSubGridsRequestArgument : SubGridsRequestArgument, new()
        where TSubGridRequestsResponse : SubGridRequestsResponse, new()
        where TSubGridRequestor : SubGridRequestsBase<TSubGridsRequestArgument, TSubGridRequestsResponse>, IDisposable, new() 
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger Log = Logging.Logger.CreateLogger<SubGridPipelineBase<TSubGridsRequestArgument, TSubGridRequestsResponse, TSubGridRequestor>>();

        /// <summary>
        /// The event used to signal that the pipeline processing has completed, or aborted
        /// </summary>
        public SemaphoreSlim PipelineSignalEvent { get; } = new SemaphoreSlim(0, 1);

        /// <summary>
       /// Records how may sub grid results there are pending to be processed through the Task assigned to this pipeline.
        /// As each sub grid result is processed in the task, the task pings the pipeline to note another task has been 
        /// completed. Once this count reaches zero the pipeline is cleared to complete its processing.
        /// </summary>
        private long subGridsRemainingToProcess;
        public long SubGridsRemainingToProcess => subGridsRemainingToProcess;
        public long TotalSubGridsToProcess { get; private set; }

        public ITRexTask PipelineTask { get; set; }

        public bool Aborted { get; set; }

        public bool Terminated { get; set; }

        //public uint TimeToLiveSeconds = 0;
        //private DateTime TimeToLiveExpiryTime = DateTime.MaxValue;

        /// <summary>
        /// The request descriptor ID for this request
        /// </summary>
        public Guid RequestDescriptor { get; set; }

        // FMaximumOutstandingSubGridRequests : Integer;

        public Guid DataModelID { get; set; } = Guid.Empty;

        /// <summary>
        /// OverallExistenceMap is the map which describes the combination of Prod Data and Surveyed Surfaces
        /// </summary>
        public ISubGridTreeBitMask OverallExistenceMap { get; set; }

        /// <summary>
        /// ProdDataExistenceMap is the sub grid existence map for the data model referenced by FDataModelID
        /// </summary>
        public ISubGridTreeBitMask ProdDataExistenceMap { get; set; }

        /// <summary>
        /// Notes if the underlying query needs to include surveyed surface information in its results
        /// </summary>
        public bool IncludeSurveyedSurfaceInformation { get; set; } = true;

        /// <summary>
        /// DesignSubGridOverlayMap is the sub grid index for sub grids that cover the design being related to for cut/fill operations
        /// </summary>
        public ISubGridTreeBitMask DesignSubGridOverlayMap { get; set; }

        /// <summary>
        /// The set of filters to be made available to the sub grid processing for this request
        /// </summary>
        public IFilterSet FilterSet { get; set; }

        public int MaxNumberOfPassesToReturn { get; set; } = int.MaxValue;

        public DesignOffset ReferenceDesign { get; set; } = new DesignOffset();

        // public float FNoChangeVolumeTolerance;

        public AreaControlSet AreaControlSet { get; set; } = AreaControlSet.CreateAreaControlSet();

        private bool pipelineCompleted;

        public bool PipelineCompleted {
            get => pipelineCompleted;
            set
            {
                pipelineCompleted = value;

                // The pipeline has been signaled as complete so set its completion signal
                if (PipelineSignalEvent.CurrentCount == 0)
                    PipelineSignalEvent.Release();
            }
        }

        // LiftParams is a reference to a lift build settings object provided by the caller
        public ILiftParameters LiftParams { get; set; } = new LiftParameters();

        //  property Terminated : Boolean read FTerminated;
      
        /// <summary>
        /// The type of grid data to be selected from the data model
        /// </summary>
        public GridDataType GridDataType { get; set; } = GridDataType.All;

        // public BoundingIntegerExtent2D OverrideSpatialCellRestriction { get; set; } = BoundingIntegerExtent2D.Inverted();
     
        /// <summary>
        /// The request analyzer to be used to identify the set of sub grids required for the request.
        /// If no analyzer is supplied then a default analyzer will be created as need by the pipeline
        /// </summary>
        public IRequestAnalyser RequestAnalyser { get; set; }

        /// <summary>
        /// A lambda to provide custom initialization of specialist sub grids arguments used for different purposes
        /// </summary>
        public Action<TSubGridsRequestArgument> CustomArgumentInitializer { get; set; }

        private void AllSubgridsProcessed()
        {
            if (PipelineSignalEvent.CurrentCount == 0)
              PipelineSignalEvent.Release();
        }

        /// <summary>
        /// Advises that a group of sub grids has been processed and can be removed from the tally of
        /// sub grids awaiting results.
        /// This is typically used by aggregative queries where the cache compute cluster aggregates
        /// sub grid results within each partition and returns pre-aggregated results
        /// </summary>
        public void SubGridsProcessed(long numProcessed)
        {
            if (Interlocked.Add(ref subGridsRemainingToProcess, -numProcessed) <= 0)
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
        /// Orchestrates sending sub grid requests to the compute cluster and handling the result
        /// </summary>
        public bool Initiate()
        {
          // First analyze the request to determine the set of sub grids that will need to be requested
          if (RequestAnalyser.Execute())
          {
            subGridsRemainingToProcess = RequestAnalyser.TotalNumberOfSubGridsToRequest;

            Log.LogInformation($"Request analyzer counts {RequestAnalyser.TotalNumberOfSubGridsToRequest} sub grids to be requested, compared to {OverallExistenceMap.CountBits()} sub grids in production existence map");
            Log.LogInformation($"START: Request for {RequestAnalyser.TotalNumberOfSubGridsToRequest} sub grids");

            // Send the sub grid request mask to the grid fabric layer for processing
            if (RequestAnalyser.TotalNumberOfSubGridsToRequest > 0)
            {
              using var requestor = new TSubGridRequestor
              {
                TRexTask = PipelineTask,
                SiteModelID = DataModelID,
                RequestID = RequestDescriptor,
                TRexNodeId = PipelineTask.TRexNodeID,
                RequestedGridDataType = GridDataType,
                IncludeSurveyedSurfaceInformation = IncludeSurveyedSurfaceInformation,
                ProdDataMask = RequestAnalyser.ProdDataMask,
                SurveyedSurfaceOnlyMask = RequestAnalyser.SurveyedSurfaceOnlyMask,
                Filters = FilterSet,
                ReferenceDesign = ReferenceDesign,
                AreaControlSet = AreaControlSet,
                CustomArgumentInitializer = subGridArg => {}
              };

              var Response = requestor.Execute();
              if (Response.ResponseCode != SubGridRequestsResponseResult.OK)
              {
                Log.LogWarning($"Sub Grid Task failed with error {Response.ResponseCode}");
                return false;
              }

              Log.LogInformation($"COMPLETED: Request for {RequestAnalyser.TotalNumberOfSubGridsToRequest} sub grids");
              TotalSubGridsToProcess = RequestAnalyser.TotalNumberOfSubGridsToRequest;
              return true;
            }
            else
            {
              Log.LogInformation("SKIPPED: Requested no sub grids to process.");
              TotalSubGridsToProcess = 0;
              return true;
            }
          }

          Log.LogWarning($"RequestAnalyser failed execution - cannot process any sub grids (if any).");
          return false;
        }

        /// <summary>
        /// Waits until the set of requests injected into the pipeline have yielded all required results
        /// (passed into the relevant Task and signaled), or the pipeline timeout has expired
        /// </summary>
        public Task<bool> WaitForCompletion()
        {
          // If we have nothing to process, no point in waiting.
          if (TotalSubGridsToProcess == 0)
          {
            return Task.FromResult(true);
          }

          // Todo: Make the 2 minute limit configurable
          return PipelineSignalEvent.WaitAsync(120000); // Don't wait for more than two minutes...
        }
    }
}
