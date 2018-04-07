using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VSS.VisionLink.Raptor.Events;
using VSS.VisionLink.Raptor.Interfaces;
using VSS.VisionLink.Raptor.Machines;
using VSS.VisionLink.Raptor.SiteModels;
using VSS.VisionLink.Raptor.Storage;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor.TAGFiles.Types;

namespace VSS.VisionLink.Raptor.TAGFiles.Classes.Integrator
{
    public class AggregatedDataIntegratorWorker
    {
        /// <summary>
        /// A queue of the tasks this worker will process into the Raptor data stores
        /// </summary>
        private ConcurrentQueue<AggregatedDataIntegratorTask> TasksToProcess;

        /// <summary>
        /// A bitmask sub grid tree that tracks all subgrids modified by the tasks this worker has processed
        /// </summary>
        private SubGridTreeSubGridExistenceBitMask WorkingModelUpdateMap;

        /// <summary>
        /// The mutable grid storage proxy
        /// </summary>
        private IStorageProxy storageProxy_Mutable;

        /// <summary>
        /// The immutable grid storage proxy
        /// </summary>
        private IStorageProxy storageProxy_Immutable;

        /// <summary>
        /// Worker constructor that obtains the necessary storage proxies
        /// </summary>
        public AggregatedDataIntegratorWorker()
        {
            storageProxy_Immutable = StorageProxy.RaptorInstance(StorageMutability.Immutable);
            storageProxy_Mutable = StorageProxy.RaptorInstance(StorageMutability.Mutable);
        }

        /// <summary>
        /// Worker constructor accepting the list of tasks for it to process
        /// </summary>
        /// <param name="tasksToProcess"></param>
        public AggregatedDataIntegratorWorker(ConcurrentQueue<AggregatedDataIntegratorTask> tasksToProcess) : this()
        {
            TasksToProcess = tasksToProcess;
        }

        /// <summary>
        /// Event that records a particular subgrid has been modified, identified by the address of a 
        /// cell within that subgrid
        /// </summary>
        /// <param name="CellX"></param>
        /// <param name="CellY"></param>
        private void SubgridHasChanged(uint CellX, uint CellY)
        {
            WorkingModelUpdateMap.SetCell(CellX >> SubGridTree.SubGridIndexBitsPerLevel,
                                          CellY >> SubGridTree.SubGridIndexBitsPerLevel, true);
        }

        /*
         *   Procedure HandleDecapsulation(AggregatedCellPasses : TICServerSubGridTree);cul
                begin
            // Decapsulate the cell passes so they are accessible
            if kEncapsulateIntermediaryTAGFileProcessingResults then
              begin
                DataIntegratorInstance.IncrementEncapsulationSizeAndCapacity(-AggregatedCellPasses.EncapsulatedSize,
                                                                             -AggregatedCellPasses.EncapsulatedCapacity);
                AggregatedCellPasses.Decapsulate;
              end;
          end;
        */

        public bool ProcessTask(List<AggregatedDataIntegratorTask> ProcessedTasks)
        {
            ProductionEventChanges SiteModelMachineTargetValues = null;

            bool AnyMachineEvents = false;
            bool AnyCellPasses = false;

            SiteModel SiteModelFromDM;
            Machine MachineFromDM;
            // string LastKnownDesignName;
            int Comparison;

            AggregatedDataIntegratorTask Task;
            AggregatedDataIntegratorTask TestTask;

            EventIntegrator eventIntegrator = new EventIntegrator();

            /* The task contains a set of machine events and cell passes that need to be integrated into the
              machine and sitemodel references in the task respectively.Machine events need to be integrated
              before the cell passes that reference them are integrated.

              All other tasks in the task list that contain aggregated machine events and cell passes
              are integrated together into the machine events and sitemodel in one operation prior to
              the modified information being committed to disk.

              A task is only said to be completed when all integrations and resulting updates are
              persisted to disk.*/

            ProcessedTasks.Clear();

            // Set capacity to maximum expected size to prevent List resizing while assembling tasks
            //TODO ... ProcessedTasks.Capacity = VLPDSvcLocations.VLPDTagProc_MaxMappedTAGFilesToProcessPerAggregationEpoch;

            try
            {
                try
                {
                    lock (TasksToProcess)
                    {
                        // ====== STAGE 0: DETERMINE THE SITEMODEL AND MACHINE TO PROCESS TAG FILES FOR FROM THE BASE TASK
                        if (!TasksToProcess.TryDequeue(out Task))
                        {
                            return true;
                        }

                        if (Task == null)
                        {
                            // There is nothing in the queue to work on so just return true
                            return true;
                        }

                        Task.StartProcessingTime = DateTime.Now;

                        ProcessedTasks.Add(Task); // Seed task is always a part of the processed tasks

                        // First check to see if this task has been catered to by previous task processing
                        AnyMachineEvents = Task.AggregatedMachineEvents != null;
                        AnyCellPasses = Task.AggregatedCellPasses != null;

                        if (!(AnyMachineEvents || AnyCellPasses))
                        {
                            // TODO add when logging available
                            //SIGLogMessage.PublishNoODS(Self, 'No machine event or cell passes in base task', slmcMessage); // Nothing to do
                            return true;
                        }

                        //  SIGLogMessage.PublishNoODS(Self, 'Aggregation Task Process --> Filter tasks to aggregate', slmcDebug); {SKIP}

                        // ====== STAGE 1: ASSEMBLE LIST OF TAG FILES TO AGGREGATE IN ONE OPERATION

                        // Populate the tasks to process list with the aggregations that will be
                        // processed at this time. These tasks are also removed from the main task
                        // list to allow the TAG file processors to prepare additional TAG files
                        // while this set is being integrated into the model.


                        if (TasksToProcess.Count > 0)
                        {
                            for (int I = 0; I < Math.Min(TasksToProcess.Count - 1, TasksToProcess.Count /*Removed for POC VLPDSvcLocations.VLPDTagProc_MaxMappedTAGFilesToProcessPerAggregationEpoch*/); I++)
                            {
                                if (TasksToProcess.TryPeek(out TestTask))
                                {
                                    if (TestTask.TargetSiteModelID == Task.TargetSiteModelID && TestTask.TargetMachineID == Task.TargetMachineID &&
                                      AnyCellPasses == (TestTask.AggregatedCellPasses != null) && AnyMachineEvents == (TestTask.AggregatedMachineEvents != null))
                                    {
                                        // Removed for Ignite POC
                                        //if (ProcessedTasks.Count < VLPDSvcLocations.VLPDTagProc_MaxMappedTAGFilesToProcessPerAggregationEpoch)
                                        //{
                                            TasksToProcess.TryDequeue(out TestTask);
                                            ProcessedTasks.Add(TestTask);
                                        //}
                                        //else
                                        //{
                                        //    break;
                                        //}
                                    }
                                }
                            }
                        }
                    }

                    // Decapsulate the cell passes so they are accessible. Perform this after assembling the tasks list to prevent
                    // depeletion of viable tasks by other aggregator workers between obtaining the first task from the list and completion
                    // of building the similar tasks into a group to be processed.
                    // TODO... HandleDecapsulation(Task.AggregatedCellPasses);

                    // TODO add when logging available
                    // SIGLogMessage.PublishNoODS(Self, Format('Aggregation Task Process --> Integrating %d TAG files for machine %d in project %d',            
                    //                                        [ProcessedTasks.Count, Task.TargetMachineID, Task.TargetSiteModelID]), slmcMessage);

                    // ====== STAGE 2: AGGREGATE ALL EVENTS AND CELL PASSES FROM ALL TAG FILES INTO THE FIRST ONE IN THE LIST

                    for (int I = 1; I < ProcessedTasks.Count; I++) // Zeroth item in the list is Task
                    {
                        AggregatedDataIntegratorTask processedTask = ProcessedTasks[I];

                        // 'Include' the extents etc of each sitemodel being merged into 'task' into its extents and design change events
                        Task.TargetSiteModel.Include(processedTask.TargetSiteModel);

                        // Integrate the machine events
                        if (AnyMachineEvents && (processedTask.AggregatedMachineEvents != null))
                        {
                            eventIntegrator.IntegrateMachineEvents(processedTask.AggregatedMachineEvents, Task.AggregatedMachineEvents, true, false);
                            processedTask.AggregatedMachineEvents = null; // FreeAndNil(AggregatedMachineEvents);
                        }

                        //    SIGLogMessage.PublishNoODS(Self, Format('Aggregation Task Process --> Integrate %d cell pass trees', [ProcessedTasks.Count]), slmcDebug);

                        // Integrate the cell passes from all cell pass aggregators containing cell passes for this machine and sitemodel
                        if (AnyCellPasses && processedTask.AggregatedCellPasses != null)
                        {
                            // Decapsulate the cell passes so they are accessible
                            // TODO...  HandleDecapsulation(AggregatedCellPasses);

                            SubGridIntegrator subGridIntegrator = new SubGridIntegrator(processedTask.AggregatedCellPasses, null /* ProcessedTasks[I].TargetSiteModel*/, Task.AggregatedCellPasses, null);
                            subGridIntegrator.IntegrateSubGridTree(//ProcessedTasks[I].AggregatedCellPasses,
                                                                   //null,
                                                                   //Task.AggregatedCellPasses,
                                                                   SubGridTreeIntegrationMode.UsingInMemoryTarget,
                                                                   SubgridHasChanged);

                            //Update current DateTime with the lates on
                            if (processedTask.TargetMachine.LastKnownPositionTimeStamp.CompareTo(Task.TargetMachine.LastKnownPositionTimeStamp) == -1)
                            {
                                Task.TargetMachine.LastKnownPositionTimeStamp = processedTask.TargetMachine.LastKnownPositionTimeStamp;
                                Task.TargetMachine.LastKnownX = processedTask.TargetMachine.LastKnownX;
                                Task.TargetMachine.LastKnownY = processedTask.TargetMachine.LastKnownY;
                            }

                            processedTask.AggregatedCellPasses = null; // FreeAndNil(AggregatedCellPasses);
                        }
                    }

                    // Integrate the items present in the 'TargetSiteModel' into the real sitemodel
                    // read from the datamodel file itself, then synchronously write it to the DataModel
                    // avoiding the use of the deferred persistor.
                    SiteModelFromDM = SiteModels.SiteModels.Instance(StorageMutability.Mutable).GetSiteModel(Task.TargetSiteModelID);

                    if (SiteModelFromDM == null)
                    {
                        // TODO readd when logging available
                        //SIGLogMessage.PublishNoODS(Self, Format('Unable to lock SiteModel %d from the data model file', [Task.TargetSiteModelID]), slmcWarning);
                        return false;
                    }

                    lock (SiteModelFromDM)
                    {
                        // 'Include' the extents etc of the 'task' each sitemodel being merged into the persistent database
                        SiteModelFromDM.Include(Task.TargetSiteModel);

                        // Need to locate or create a matching machine in the site model.
                        MachineFromDM = SiteModelFromDM.Machines.Locate(Task.TargetMachineID, Task.TargetMachine.IsJohnDoeMachine);

                        if (MachineFromDM == null)
                        {
                            //   with Task.TargetMachine do
                            MachineFromDM = SiteModelFromDM.Machines.CreateNew(Task.TargetMachine.Name,
                                Task.TargetMachine.MachineHardwareID,
                                Task.TargetMachine.MachineType,
                                Task.TargetMachine.DeviceType,
                                Task.TargetMachine.IsJohnDoeMachine,
                                Task.TargetMachineID);
                            MachineFromDM.Assign(Task.TargetMachine);
                        }

                        // Bug 23038: Update the internal name of the machine with the machine name from the TAG file
                        if (Task.TargetMachine.Name != "" && MachineFromDM.Name != Task.TargetMachine.Name)
                        {
                            MachineFromDM.Name = Task.TargetMachine.Name;
                        }

                        // Bug 23039: Update the internal type of the machine with the machine type from the TAG file
                        // if the existing internal machine type is zero then
                        if (Task.TargetMachine.MachineType != 0 && MachineFromDM.MachineType == 0)
                        {
                            MachineFromDM.MachineType = Task.TargetMachine.MachineType;
                        }

                        if (AnyMachineEvents)
                        {
                            // If the machine target values can't be found then create them
                            SiteModelMachineTargetValues = SiteModelFromDM.MachinesTargetValues[MachineFromDM.ID];

                            if (SiteModelMachineTargetValues == null)
                            {
                                SiteModelFromDM.MachinesTargetValues.Add(new ProductionEventChanges(SiteModelFromDM, MachineFromDM.ID));
                                //SiteModelFromDM.MachinesTargetValues.CreateNewMachineTargetValues(MachineFromDM, MachineFromDM.ID);
                            }

                            // Check to see the machine target values were created correctly
                            SiteModelMachineTargetValues = SiteModelFromDM.MachinesTargetValues[MachineFromDM.ID];

                            // The events for this machine have not yet been read from the persistent store
                            // TODO: There is no check to see if they have already been loaded...
                            if (!SiteModelMachineTargetValues.LoadEventsForMachine(storageProxy_Mutable))
                            {
                                return false;
                            }
                        }
                    }

                    // ====== STAGE 3: INTEGRATE THE AGGREGATED EVENTS INTO THE PRIMARY LIVE DATABASE

                    if (AnyMachineEvents)
                    {
                        // Perform machine event integration outside of the SiteModel write access interlock as the
                        // individual event lists have independent exclusive locks event integration uses.
                        eventIntegrator.IntegrateMachineEvents(Task.AggregatedMachineEvents, SiteModelMachineTargetValues, true, true);

                        // Integrate the machine events into the main site model. This requires the
                        // sitemodel interlock as aspects of the sitemodel state (machine) are being changed.
                        lock (SiteModelFromDM)
                        {
                            if (SiteModelMachineTargetValues != null)
                            {
                                //Update machine last known value (events) from integrated model before saving
                                Comparison = MachineFromDM.LastKnownPositionTimeStamp.CompareTo(Task.TargetMachine.LastKnownPositionTimeStamp);
                                if (Comparison < 1)
                                {
                                    /* TODO...
                                    if (SiteModelMachineTargetValues.EventDesignNames.Count > 0)
                                    {
                                        SiteModelFromDM.GetDesignName((SiteModelMachineTargetValues.TargetValueChanges.EventDesignNames.Items[SiteModelMachineTargetValues.TargetValueChanges.EventDesignNames.Count - 1] as TICEventDesignNameValueChange).EventDesignNameID, LastKnownDesignName);
                                        MachineFromDM.LastKnownDesignName = LastKnownDesignName;
                                    }
                                    else
                                    {
                                        MachineFromDM.LastKnownDesignName = "";
                                    }
                                    */

                                    if (SiteModelMachineTargetValues.LayerIDStateEvents.Count > 0)
                                    {
                                        MachineFromDM.LastKnownLayerId = SiteModelMachineTargetValues.LayerIDStateEvents.Last().State;
                                    }
                                    else
                                    {
                                        MachineFromDM.LastKnownLayerId = 0;
                                    }

                                    MachineFromDM.LastKnownPositionTimeStamp = Task.TargetMachine.LastKnownPositionTimeStamp;
                                    MachineFromDM.LastKnownX = Task.TargetMachine.LastKnownX;
                                    MachineFromDM.LastKnownY = Task.TargetMachine.LastKnownY;
                                }
                            }
                            else
                            {
                                // TODO add when logging available
                                //SIGLogMessage.PublishNoODS(Self, 'SiteModelMachineTargetValues not located in aggregate machine events integrator', slmcError);
                                return false;
                            }
                        }
                    }

                    // Free the aggregated machine events as they are no longer needed. Don't do this under a SiteModel write access lock
                    // to prevent blocking of other aggreation threads while this occurs.
                    if (SiteModelMachineTargetValues != null)
                    {
                        Task.AggregatedMachineEvents = null; // FreeAndNil(Task.AggregatedMachineEvents);
                    }

                    // Use the synchronous command to save the machine events to the persistent store into the deferred (asynchronous model)
                    SiteModelMachineTargetValues.SaveMachineEventsToPersistentStore(storageProxy_Mutable);

                    // ====== STAGE 3: INTEGRATE THE AGGREGATED CELL PASSES INTO THE PRIMARY LIVE DATABASE
                    if (AnyCellPasses)
                    {
                        try
                        {
                            // This is a dirty map for the leaf subgrids and is stored as a bitmap grid
                            // with one level fewer that the subgrid tree it is representing, and
                            // with cells the size of the leaf subgrids themselves. As the cell coordinates
                            // we have been given are with respect to the subgrid, we must transform them
                            // into coordinates relavant to the dirty bitmap subgrid tree.

                            WorkingModelUpdateMap = new SubGridTreeSubGridExistenceBitMask
                            {
                                CellSize = SubGridTree.SubGridTreeDimension * SiteModelFromDM.Grid.CellSize,
                                ID = SiteModelFromDM.ID
                            };

                            // Integrate the cell pass data into the main sitemodel and commit each subgrid as it is updated
                            SubGridIntegrator subGridIntegrator = new SubGridIntegrator(Task.AggregatedCellPasses, SiteModelFromDM, SiteModelFromDM.Grid, storageProxy_Mutable);

                            if (!subGridIntegrator.IntegrateSubGridTree(//Task.AggregatedCellPasses,
                                                                        //SiteModelFromDM,
                                                                        //SiteModelFromDM.Grid,
                                                                        SubGridTreeIntegrationMode.SaveToPersistentStore,
                                                                        // DataPersistorInstance,
                                                                        SubgridHasChanged))
                            {
                                return false;
                            }

                            // Transfer the working sitemodel update map to the Task to allow the task finalizer to
                            // synchronise completion of this work unit in terms of persistence of
                            // all the changes to disk with the notification to the wider Raptor stack
                            // that a set of subgrids have been changed
                            Task.SetAggregateModifiedSubgrids(ref WorkingModelUpdateMap);
                        }
                        finally
                        {
                            Task.AggregatedCellPasses = null; // FreeAndNil(Task.AggregatedCellPasses);
                            WorkingModelUpdateMap = null; // FreeAndNil(FWorkingModelUpdateMap);
                        }
                    }

                    // Use the synchonous command to save the site model information to the persistent store into the deferred (asynchronous model)
                    SiteModelFromDM.SaveToPersistentStore();
                }
                finally
                {
                    if (!(AnyMachineEvents || AnyCellPasses))
                    {
                        // TODO add when logging available
                        // SIGLogMessage.PublishNoODS(Self, Format('Suspicious task with no cell passes or machine events in Sitemodel %d', [Task.TargetSiteModelID]), slmcWarning); { SKIP}
                    }

                    // TODO add when logging available
                    // SIGLogMessage.PublishNoODS(Self, Format('Aggregation Task Process --> Completed integrating %d TAG files for machine %d in project %d',
                    //                                        [ProcessedTasks.Count, Task.TargetMachineID, Task.TargetSiteModelID]), slmcMessage);
                }
            }
            catch // (Exception E)
            {
                // TODO add when logging available
                // SIGLogMessage.PublishNoODS(Self, Format('Exception in %s.ProcessTask: %s', [Self.ClassName, E.Message]), slmcException);
                return false;
            }

            return true;
        }
    }
}
