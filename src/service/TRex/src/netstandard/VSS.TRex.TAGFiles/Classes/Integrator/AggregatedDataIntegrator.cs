using System;
using System.Collections.Concurrent;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.TAGFiles.Classes.Integrator
{
    public class AggregatedDataIntegrator
    {
        /// <summary>
        /// TasksToProcess is the list of tasks the processor is working through.
        /// </summary>
        public ConcurrentQueue<AggregatedDataIntegratorTask> TasksToProcess { get; } = new ConcurrentQueue<AggregatedDataIntegratorTask>();

        // FProcessEvent is used to wake up this processing thread when something arrives
        // into the FFilesToProcess list
        //      FProcessEvent : TSimpleEvent;

        // FShuttingDown is a signal to the processor to clean up and prepare for
        // shutdown
        //      FShuttingDown : Boolean;

        // FShutdownReadyEvent is used to signal the wider production server that
        // the snippet processor is ready to shutdown.
        //      FShutdownReadyEvent : TSimpleEvent;

        private int _pendingFilesToBeProcessedCount;

        private int _outstandingCellPasses;
        private long _totalCellPassesProcessed;

        //      FNumberOfTasksBeingProcessed : Integer;
        //      FRemainingNumberOfTasksBeingProcessed : Integer;

        //      FWorkers : Array of AggregatedDataIntegratorWorkerThread;

        //    public
        //      property ShutdownReadyEvent : TSimpleEvent read FShutdownReadyEvent;

        // AddTaskToProcessList adds a task to the processing queue for the task
        // processor. This is a thread safe call, multiple threads may safely add
        // tasks to the list in a concurrent fashion if required.
        // Each task added to the process list represents a tag file that has been
        // processed
        public void AddTaskToProcessList(ISiteModel transientSiteModel,
                                         Guid persistentSiteModelId,
                                         IMachinesList transientMachines,
                                         IServerSubGridTree aggregatedCellPasses,
                                         int aggregatedCellPassCount,
                                         IMachinesProductionEventLists aggregatedMachineEvents)
        {
            var newTask = new AggregatedDataIntegratorTask
            {
                IntermediaryTargetSiteModel = transientSiteModel,
                PersistedTargetSiteModelID = persistentSiteModelId,
                IntermediaryTargetMachines = transientMachines,
                AggregatedCellPasses = aggregatedCellPasses,
                AggregatedMachineEvents = aggregatedMachineEvents,
                AggregatedCellPassCount = aggregatedCellPassCount
            };

            IncrementOutstandingCellPasses(aggregatedCellPassCount);
            System.Threading.Interlocked.Add(ref _totalCellPassesProcessed, aggregatedCellPassCount);

            TasksToProcess.Enqueue(newTask);

            System.Threading.Interlocked.Increment(ref _pendingFilesToBeProcessedCount);

            // FProcessEvent.SetEvent;
        }

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
            outstandingCellPasses = _outstandingCellPasses;
            totalCellPassesProcessed = _totalCellPassesProcessed;
            pendingFilesToBeProcessed = _pendingFilesToBeProcessedCount;
        }

        // CanAcceptMoreAggregatedCellPasses keeps track of whether the buffer of cell
        // passes currently pending integration into the database can accept more cell passes
        public bool CanAcceptMoreAggregatedCellPasses => true;

        public void IncrementOutstandingCellPasses(int increment) => System.Threading.Interlocked.Add(ref _outstandingCellPasses, increment);
    }
}
