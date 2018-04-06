using System.Collections.Concurrent;
using System.Linq;
using VSS.VisionLink.Raptor.Events;
using VSS.VisionLink.Raptor.Machines;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.SubGridTrees.Server;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.Integrator
{
    public class AggregatedDataIntegrator
    {
        /// <summary>
        /// TasksToProcess is the list of tasks the processor is working through.
        /// </summary>
        public ConcurrentQueue<AggregatedDataIntegratorTask> TasksToProcess { get; set; } = new ConcurrentQueue<AggregatedDataIntegratorTask>();

        //     FPendingFilesInterlock : TCriticalSection;

        // FProcessEvent is used to wake up this processing thread when somthing arrives
        // into the FFilesToProcess list
        //      FProcessEvent : TSimpleEvent;

        // FShuttingDown is a signal to the processor to clean up and prepare for
        // shutdown
        //      FShuttingDown : Boolean;

        // FShutdownReadyEvent is used to signal the wider production server that
        // the snippet processor is ready to shutdown.
        //      FShutdownReadyEvent : TSimpleEvent;

        //      FPendingFilesToBeProcessed : TStringList;
        private int PendingFilesToBeProcessedCount;

        private int OutstandingCellPasses;
        private long TotalCellPassesProcessed;

        //      FNumberOfTasksBeingProcessed : Integer;
        //      FRemainingNumberOfTasksBeingProcessed : Integer;

        //      FEncapsulatedSubgridsSize : Int64;
        //      FEncapsulatedSubgridsCapacity : Int64;

        //      FWorkers : Array of TSVOICAggregatedDataIntegratorWorkerThread;

        //    public
        //      property ShutdownReadyEvent : TSimpleEvent read FShutdownReadyEvent;

        // AddTaskToProcessList adds a task to the processing queue for the task
        // processor. This is a thread safe call, multiple threads may safely add
        // tasks to the list in a concurrent fashion if required.
        // Each task added to the process list represents a tag file that has been
        // processed
        public bool AddTaskToProcessList(SiteModel siteModel,
                                      Machine machine,
                                      ServerSubGridTree aggregatedCellPasses,
                                      int aggregatedCellPassCount,
                                      ProductionEventChanges aggregatedMachineEvents /*,
                                    const ATaskFinalizer : TAggregationTaskFinalizer*/)
        {
            /*
              // First encapsulate the subgrid tree. Performing this first means that
              // we don't block the aggregation process while encapsulation is taking place
                if kEncapsulateIntermediaryTAGFileProcessingResults then
                begin
                  AAggregatedCellPasses.Encapsulate;
                        IncrementEncapsulationSizeAndCapacity(AAggregatedCellPasses.EncapsulatedSize,
                                                              AAggregatedCellPasses.EncapsulatedCapacity);
                 end;
            */
            // Then add the new event

            AggregatedDataIntegratorTask NewTask = new AggregatedDataIntegratorTask()
            {
                TargetSiteModel = siteModel,
                TargetSiteModelID = siteModel.ID,
                TargetMachine = machine,
                TargetMachineID = machine.ID,
                AggregatedCellPasses = aggregatedCellPasses,
                AggregatedMachineEvents = aggregatedMachineEvents,
                //Finalizer = ATaskFinalizer,
                AggregatedCellPassCount = aggregatedCellPassCount
            };

            //NewTeask.Finalizer.FinalizedTask = NewTask,

            IncrementOutstandingCellPasses(aggregatedCellPassCount);
            System.Threading.Interlocked.Add(ref TotalCellPassesProcessed, aggregatedCellPassCount);

            TasksToProcess.Enqueue(NewTask);

            /*
            if VLPDSvcLocations.VLPDTagProc_PerformIsFileInPendingCheckOnTAGFileSubmission then
    if Assigned(ATaskFinalizer) then
      begin
        FPendingFilesInterlock.Acquire;
            try
          FPendingFilesToBeProcessed.Add(ATaskFinalizer.FileName);
        finally
          FPendingFilesInterlock.Release;
            end;
            end;
*/
            System.Threading.Interlocked.Increment(ref PendingFilesToBeProcessedCount);

            // FProcessEvent.SetEvent;

            return true;
        }

        // RemoveTaskFromProcessList does the opposite of AddTaskToProcessList.
        // This is a thread safe call, multiple threads may safely add
        // tasks to the list in a concurrent fashion if required. A return result of false indicates the task was not present in the
        // tasks to process list.
        //      function RemoveTaskFromProcessList(const Task: TSVOICAggregatedDataIntegratorTask): Boolean;

        //      Procedure RemoveFileFromPendingList(const AFileName : TFileName);

        //      Function IsFileInPendingList(const AFileName : TFileName) : Boolean;

        // CountOfTasksToProcess returns the number of tasks remaining in the
        // tasks to process list. This is a thread safe call, multiple threads may safely add
        // files to the list in a concurrent fashion if required.
        public int CountOfTasksToProcess => TasksToProcess.Count;

        public AggregatedDataIntegrator()
        {

        }

        //        Function SystemMonitorString : String;

        public void GetStatistics(out int outstandingCellPasses,
                                  out long totalCellPassesProcessed,
                                  out int pendingFilesToBeProcessed)
        {
            outstandingCellPasses = OutstandingCellPasses;
            totalCellPassesProcessed = TotalCellPassesProcessed;
            pendingFilesToBeProcessed = PendingFilesToBeProcessedCount;
        }

        // CanAcceptMoreAggregatedCellPasses keeps track of whether the buffer of cell
        // passes currently pending integration into the database can accept more cell passes
        public bool CanAcceptMoreAggregatedCellPasses => true;

        // Procedure GetPendingFileList(const FileList : TStringList);

        public void IncrementOutstandingCellPasses(int Increment) => System.Threading.Interlocked.Add(ref OutstandingCellPasses, Increment);

        //  Procedure IncrementEncapsulationSizeAndCapacity(Const SizeIncrement, CapacityIncrement : Int64);
    }
}
