using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Executors.Tasks;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Pipelines
{
    public class SubGridPipelineBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Records how may subgrid results there are pending to be processed through the Task assigned to this pipeline.
        /// As each subgrid result is processed in the task, the task pings the pipeline to note another task has been 
        /// completed. Once this count reaches zero the pipeline is cleared to complete its processing.
        /// </summary>
        private long SubgridsRemainingToProcess = 0;

        // Derived from TSVOICSubGridPipelineBase = class(TObject)

        public int ID = 0;
        public PipelinedSubGridTask PipelineTask = null;
        public bool PipelineAborted = false;

        public UInt32 TimeToLiveSeconds = 0;
        DateTime TimeToLiveExpiryTime = DateTime.MaxValue;

        // public FExternalDescriptor: TASNodeRequestDescriptor;

        //      FMaximumOutstandingSubgridRequests : Integer;

        private bool pipelineCompleted = false;
        protected void SetPipelineCompleted(bool value) => pipelineCompleted = value;

        // FSubmissionNode deals with submitting the requests for subgrids to the
        // production server via the asynchronous subgrid request PS API
        //              FSubmissionNode : TSVOICSubGridSubmissionThread;

        // FOperationNode performs some client defined operation upon the subgrids
        // fetched from the PS
        //              FOperationNode : TSVOICPipelineResultOperationThreadBase;

        // FLiftBuildSettings is a reference to a lift build settings object
        // provided by the caller
        //       FLiftBuildSettings : TICLiftBuildSettings;

        public long DataModelID = -1;

        //   public           FIP : DWord;   // IP Address where subgrid results should be sent
        //   public           FPort : Word;  //  IP port number where subgrid results should be sent
        //   public           FResponsePort : Word;  //  IP port number where subgrid responses should be sent

        // FOverallExistenceMap is the map which describes the combination of Prod Data and Surveyed Surfaces
        // FProdDataExistenceMap is the subgrid existence map for the data model referenced
        // by FDataModelID
        public SubGridTreeBitMask OverallExistenceMap = null;
        public SubGridTreeBitMask ProdDataExistenceMap = null;

        public bool IncludeSurveyedSurfaceInformation = true;

        // FDesignSubgridOverlayMap is the subgrid index for subgrids that cover the
        // design being related to for cut/fill operations
        public SubGridTreeBitMask DesignSubgridOverlayMap = null;

        public FilterSet FilterSet = null;
        public int MaxNumberOfPassesToReturn = 0;

        //      public         FReferenceDesign : TVLPDDesignDescriptor;
        //      public         FReferenceVolumeType : TComputeICVolumesType;
        //      public         FNoChangeVolumeTolerance : Single;

        public AreaControlSet AreaControlSet;

        //              FCompleteEvent : TSimpleEvent;

        public long RequestDescriptor = -1;

        public bool Terminated = false;

        //              property SubmissionNode : TSVOICSubGridSubmissionThread read FSubmissionNode;
        //              property OperationNode : TSVOICPipelineResultOperationThreadBase read FOperationNode;

        public bool PipelineCompleted { get { return pipelineCompleted; } set { SetPipelineCompleted(value); } }

        //              property LiftBuildSettings : TICLiftBuildSettings read FLiftBuildSettings write FLiftBuildSettings;
        //              property CompleteEvent : TSimpleEvent read FCompleteEvent;
        //              property Terminated : Boolean read FTerminated;

        public GridDataType GridDataType = GridDataType.All;

        public BoundingWorldExtent3D WorldExtents = BoundingWorldExtent3D.Inverted();
        public BoundingIntegerExtent2D OverrideSpatialCellRestriction = BoundingIntegerExtent2D.Inverted();

        public bool AllFinished = false;

        public void SubgridProcessed()
        {
            if (System.Threading.Interlocked.Decrement(ref SubgridsRemainingToProcess) <= 0)
            {
                AllFinished = true;
            }
        }

        public SubGridPipelineBase(int AID, PipelinedSubGridTask task)
        {
            PipelineTask = task;
            ID = AID;
            // FCompleteEvent:= TSimpleEvent.Create;

            //            FIP:= 0;
            //            FPort:= 0;
            //            FResponsePort:= 0;

            //      FMaxNumberOfPassesToReturn:= VLPDSvcLocations.VLPDPSNode_MaxCellPassIterationDepth_PassCountDetailAndSummary;

            //            ReferenceDesign.Clear;
            //          FReferenceVolumeType:= ic_cvtNone;
            //        FNoChangeVolumeTolerance:= 0;

            // FPixelXWorldSize := 0.0;
            // FPixelYWorldSize := 0.0;
            AreaControlSet = AreaControlSet.Null();

            //    FLiftBuildSettings:= Nil;

            //            FTimeToLiveSeconds:= kDefaultSubgridPipelineTimeToLiveSeconds;

            //            FMaximumOutstandingSubgridRequests:= VLPDSvcLocations.VLPDASNode_DefaultMaximumOutstandingSubgridRequestsInPipeline;

            //            FCompleteEvent.ResetEvent;

            //            if Assigned(FSubmissionNode) then
            //    FSubmissionNode.Clean;
            //            if Assigned(FOperationNode) then
            //    FOperationNode.Clean;

            // TSVOICPipelineMonitor.Instance.RegisterPipeline(Self);
        }

        //        Destructor Destroy; Override;

        //              procedure Initiate; Virtual;
        public virtual void Abort() => PipelineAborted = true;

        //              procedure Terminate; Virtual;
        //              Function TimeToLiveExpired : Boolean;

        // AbortAndShutdown instructs the pipeline to abandon operations and abort itself.
        // It is used by the pipeline monitor shutdown pipelines when the service is
        // being restarted.
        //              Procedure AbortAndShutdown;

        public void Initiate()
        {
            // First analyse the request to determine the set of subgrids that will need to be requested
            RequestAnalyser analyser = new RequestAnalyser(this, WorldExtents);
            if (!analyser.Execute())
            {
                // Leave gracefully...
                return;
            }

            SubgridsRemainingToProcess = analyser.TotalNumberOfSubgridsAnalysed;

            Log.InfoFormat("Request analyser counts {0} subgrids to be requested, compared to {1} subgrids in production existance map", analyser.TotalNumberOfSubgridsAnalysed, ProdDataExistenceMap.CountBits());

            if (analyser.TotalNumberOfSubgridsAnalysed == 0)
            {
                // There are no subgrids to be requested, leave quietly
                return;
            }

            // Send the subgrid request mask to the grid fabric layer for processing
            SubGridRequests gridFabricRequest = new SubGridRequests(PipelineTask, DataModelID, PipelineTask.RequestDescriptor, PipelineTask.RaptorNodeID, GridDataType, analyser.Mask, FilterSet);

            try
            {
                gridFabricRequest.Execute();
            }
            catch (Exception E)
            {
                throw;
            }
        }
    }
}
