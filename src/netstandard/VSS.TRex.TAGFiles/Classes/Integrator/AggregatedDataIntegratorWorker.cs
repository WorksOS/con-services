using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.Integrator
{
    public class AggregatedDataIntegratorWorker
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// A queue of the tasks this worker will process into the TRex data stores
        /// </summary>
        private ConcurrentQueue<AggregatedDataIntegratorTask> TasksToProcess;

        /// <summary>
        /// A bitmask sub grid tree that tracks all subgrids modified by the tasks this worker has processed
        /// </summary>
        private ISubGridTreeBitMask WorkingModelUpdateMap;

        /// <summary>
        /// The mutable grid storage proxy
        /// </summary>
        private IStorageProxy storageProxy_Mutable = DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage();

        /// <summary>
        /// Worker constructor that obtains the necessary storage proxies
        /// </summary>
        public AggregatedDataIntegratorWorker()
        {
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
            WorkingModelUpdateMap.SetCell(CellX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
                                          CellY >> SubGridTreeConsts.SubGridIndexBitsPerLevel, true);
        }

        /// <summary>
        /// Processes all available tasks in the TasksToProcess list up to the maximum number the worker will accept 
        /// for any single epoch of processing TAG files.
        /// </summary>
        /// <param name="ProcessedTasks"></param>
        /// <returns></returns>
        public bool ProcessTask(List<AggregatedDataIntegratorTask> ProcessedTasks)
        {
            IProductionEventLists SiteModelMachineTargetValues = null;

            bool AnyMachineEvents = false;
            bool AnyCellPasses = false;

            EventIntegrator eventIntegrator = new EventIntegrator();

            /* The task contains a set of machine events and cell passes that need to be integrated into the
              machine and sitemodel references in the task respectively. Machine events need to be integrated
              before the cell passes that reference them are integrated.

              All other tasks in the task list that contain aggregated machine events and cell passes
              are integrated together into the machine events and sitemodel in one operation prior to
              the modified information being committed to disk.

              A task is only said to be completed when all integrations and resulting updates are
              persisted to disk.*/

            ProcessedTasks.Clear();

            // Set capacity to maximum expected size to prevent List resizing while assembling tasks
            ProcessedTasks.Capacity = TRexConfig.MaxMappedTAGFilesToProcessPerAggregationEpoch;

            try
            {
                AggregatedDataIntegratorTask Task = null;
                try
                {
                    if (!TasksToProcess.TryDequeue(out Task))
                    {
                        return true;
                    }

                    if (Task == null)
                    {
                        // There is nothing in the queue to work on so just return true
                        return true;
                    }

                    storageProxy_Mutable.Clear();

                    // Note: This request for the SiteModel specifically asks for the mutable grid Sitemodel,
                    // and also explicitly provides the transactional storage proxy being used for processig the
                    // data from TAG files into the model
                    ISiteModel SiteModelFromDM = DIContext.Obtain<ISiteModels>().GetSiteModel(storageProxy_Mutable, Task.TargetSiteModelID, true);

                    if (SiteModelFromDM == null)
                    {
                        Log.LogError($"Unable to lock SiteModel {Task.TargetSiteModelID} from the data model file");
                        return false;
                    }

                    Task.StartProcessingTime = DateTime.Now;

                    ProcessedTasks.Add(Task); // Seed task is always a part of the processed tasks

                    // First check to see if this task has been catered to by previous task processing
                    AnyMachineEvents = Task.AggregatedMachineEvents != null;
                    AnyCellPasses = Task.AggregatedCellPasses != null;

                    if (!(AnyMachineEvents || AnyCellPasses))
                    {
                        Log.LogInformation("No machine event or cell passes in base task"); // Nothing to do
                        return true;
                    }

                    Log.LogInformation("Aggregation Task Process --> Filter tasks to aggregate");

                    // ====== STAGE 1: ASSEMBLE LIST OF TAG FILES TO AGGREGATE IN ONE OPERATION

                    // Populate the tasks to process list with the aggregations that will be
                    // processed at this time. These tasks are also removed from the main task
                    // list to allow the TAG file processors to prepare additional TAG files
                    // while this set is being integrated into the model.                    

                    if (TasksToProcess.Count > 0)
                    {
                        for (int I = 0; I < TasksToProcess.Count; I++)
                        {
                            if (TasksToProcess.TryDequeue(out AggregatedDataIntegratorTask TestTask) &&
                                AnyCellPasses == (TestTask.AggregatedCellPasses != null) &&
                                AnyMachineEvents == (TestTask.AggregatedMachineEvents != null))
                            {
                                if (ProcessedTasks.Count < TRexConfig.MaxMappedTAGFilesToProcessPerAggregationEpoch)
                                { 
                                    if (TasksToProcess.TryDequeue(out TestTask))
                                        ProcessedTasks.Add(TestTask);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }

                    Log.LogInformation($"Aggregation Task Process --> Integrating {ProcessedTasks.Count} TAG files for machine {Task.TargetMachineID} in project {Task.TargetSiteModelID}");

                    // ====== STAGE 2: AGGREGATE ALL EVENTS AND CELL PASSES FROM ALL TAG FILES INTO THE FIRST ONE IN THE LIST

                    for (int I = 1; I < ProcessedTasks.Count; I++) // Zeroth item in the list is Task
                    {
                        AggregatedDataIntegratorTask processedTask = ProcessedTasks[I];

                        // 'Include' the extents etc of each sitemodel being merged into 'task' into its extents and design change events
                        Task.TargetSiteModel.Include(processedTask.TargetSiteModel);

                        // Integrate the machine events
                        if (AnyMachineEvents && (processedTask.AggregatedMachineEvents != null))
                        {
                            eventIntegrator.IntegrateMachineEvents(processedTask.AggregatedMachineEvents, Task.AggregatedMachineEvents, false);
                            processedTask.AggregatedMachineEvents = null; 
                        }

                        //Log.LogDebug($"Aggregation Task Process --> Integrate {ProcessedTasks.Count} cell pass trees");

                        // Integrate the cell passes from all cell pass aggregators containing cell passes for this machine and sitemodel
                        if (AnyCellPasses && processedTask.AggregatedCellPasses != null)
                        {
                            SubGridIntegrator subGridIntegrator = new SubGridIntegrator(processedTask.AggregatedCellPasses, null, Task.AggregatedCellPasses, null);
                            subGridIntegrator.IntegrateSubGridTree(SubGridTreeIntegrationMode.UsingInMemoryTarget, SubgridHasChanged);

                            //Update current DateTime with the lates on
                            if (processedTask.TargetMachine.LastKnownPositionTimeStamp.CompareTo(Task.TargetMachine.LastKnownPositionTimeStamp) == -1)
                            {
                                Task.TargetMachine.LastKnownPositionTimeStamp = processedTask.TargetMachine.LastKnownPositionTimeStamp;
                                Task.TargetMachine.LastKnownX = processedTask.TargetMachine.LastKnownX;
                                Task.TargetMachine.LastKnownY = processedTask.TargetMachine.LastKnownY;
                            }

                            processedTask.AggregatedCellPasses = null;
                        }
                    }

                    // Integrate the items present in the 'TargetSiteModel' into the real sitemodel
                    // read from the datamodel file itself, then synchronously write it to the DataModel
                    // avoiding the use of the deferred persistor.

                    IMachine MachineFromDM;

                    lock (SiteModelFromDM)
                    {
                        // 'Include' the extents etc of the 'task' each sitemodel being merged into the persistent database
                        SiteModelFromDM.Include(Task.TargetSiteModel);

                        // Need to locate or create a matching machine in the site model.
                        MachineFromDM = SiteModelFromDM.Machines.Locate(Task.TargetMachineID, Task.TargetMachine.IsJohnDoeMachine);

                        if (MachineFromDM == null)
                        {
                            MachineFromDM = SiteModelFromDM.Machines.CreateNew(Task.TargetMachine.Name,
                                Task.TargetMachine.MachineHardwareID,
                                Task.TargetMachine.MachineType,
                                Task.TargetMachine.DeviceType,
                                Task.TargetMachine.IsJohnDoeMachine,
                                Task.TargetMachineID);
                            MachineFromDM.Assign(Task.TargetMachine);
                        }

                        // Update the internal name of the machine with the machine name from the TAG file
                        if (Task.TargetMachine.Name != "" && MachineFromDM.Name != Task.TargetMachine.Name)
                        {
                            MachineFromDM.Name = Task.TargetMachine.Name;
                        }

                        // Update the internal type of the machine with the machine type from the TAG file
                        // if the existing internal machine type is zero then
                        if (Task.TargetMachine.MachineType != 0 && MachineFromDM.MachineType == 0)
                        {
                            MachineFromDM.MachineType = Task.TargetMachine.MachineType;
                        }

                        if (AnyMachineEvents)
                        {
                            // If the machine target values can't be found then create them
                            SiteModelMachineTargetValues = SiteModelFromDM.MachinesTargetValues[MachineFromDM.InternalSiteModelMachineIndex];

                            if (SiteModelMachineTargetValues == null)
                            {
                                SiteModelFromDM.MachinesTargetValues.Add(new ProductionEventLists (SiteModelFromDM, MachineFromDM.InternalSiteModelMachineIndex));
                            }

                            // Check to see the machine target values were created correctly
                            SiteModelMachineTargetValues = SiteModelFromDM.MachinesTargetValues[MachineFromDM.InternalSiteModelMachineIndex];
                        }
                    }

                    // ====== STAGE 3: INTEGRATE THE AGGREGATED EVENTS INTO THE PRIMARY LIVE DATABASE

                    if (AnyMachineEvents)
                    {
                        // Perform machine event integration outside of the SiteModel write access interlock as the
                        // individual event lists have independent exclusive locks event integration uses.
                        eventIntegrator.IntegrateMachineEvents(Task.AggregatedMachineEvents, SiteModelMachineTargetValues, true);

                        // Integrate the machine events into the main site model. This requires the
                        // sitemodel interlock as aspects of the sitemodel state (machine) are being changed.
                        lock (SiteModelFromDM)
                        {
                            if (SiteModelMachineTargetValues != null)
                            {
                                //Update machine last known value (events) from integrated model before saving
                                int Comparison = MachineFromDM.LastKnownPositionTimeStamp.CompareTo(Task.TargetMachine.LastKnownPositionTimeStamp);
                                if (Comparison < 1)
                                {
                                    // TODO: Convert design name list and id event list structure from Raptor
                                    // MachineFromDM.LastKnownDesignName = SiteModelMachineTargetValues.DesignNameStateEvents.LastOrDefault().State;
                                    MachineFromDM.LastKnownDesignName = "";

                                    /* TODO as above
                                    if (SiteModelMachineTargetValues.DesignNameStateEvents.Count > 0)
                                    {
                                        SiteModelFromDM.SiteModelMachineTargetValues.DesignNameStateEvents.Items[SiteModelMachineTargetValues.TargetValueChanges.EventDesignNames.Count - 1] as TICEventDesignNameValueChange).EventDesignNameID, out string LastKnownDesignName);
                                        MachineFromDM.LastKnownDesignName = LastKnownDesignName;
                                    }
                                    */

                                    if (SiteModelMachineTargetValues.LayerIDStateEvents.Count() > 0)
                                    {
                                        MachineFromDM.LastKnownLayerId = SiteModelMachineTargetValues.LayerIDStateEvents.LastStateValue();
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
                                Log.LogError("SiteModelMachineTargetValues not located in aggregate machine events integrator");
                                return false;
                            }
                        }
                    }

                    // Free the aggregated machine events as they are no longer needed. Don't do this under a SiteModel write access lock
                    // to prevent blocking of other aggreation threads while this occurs.
                    if (SiteModelMachineTargetValues != null)
                    {
                        Task.AggregatedMachineEvents = null; 
                    }

                    // Use the synchronous command to save the machine events to the persistent store into the deferred (asynchronous model)
                    SiteModelMachineTargetValues.SaveMachineEventsToPersistentStore(storageProxy_Mutable);

                    // ====== STAGE 4: INTEGRATE THE AGGREGATED CELL PASSES INTO THE PRIMARY LIVE DATABASE
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
                                CellSize = SubGridTreeConsts.SubGridTreeDimension * SiteModelFromDM.Grid.CellSize,
                                ID = SiteModelFromDM.ID
                            };

                            // Integrate the cell pass data into the main sitemodel and commit each subgrid as it is updated
                            // ... first relable the passes with the machine ID
                            Task.AggregatedCellPasses.ScanAllSubGrids(leaf =>
                            {
                              ServerSubGridTreeLeaf serverLeaf = (ServerSubGridTreeLeaf) leaf;
                    
                              foreach (var segment in serverLeaf.Cells.PassesData.Items)
                              {
                                SubGridUtilities.SubGridDimensionalIterator((x, y) =>
                                {
                                  uint passCount = segment.PassesData.PassCount(x, y);
                                  for (int i = 0; i < passCount; i++)
                                    segment.PassesData.SetInternalMachineID(x, y, i, MachineFromDM.InternalSiteModelMachineIndex);
                                });
                              }
                              return true;
                            });

                            // ... then integrate them
                            SubGridIntegrator subGridIntegrator = new SubGridIntegrator(Task.AggregatedCellPasses, SiteModelFromDM, SiteModelFromDM.Grid, storageProxy_Mutable);
                            if (!subGridIntegrator.IntegrateSubGridTree(SubGridTreeIntegrationMode.SaveToPersistentStore, SubgridHasChanged))
                            {
                                return false;
                            }

                            // Transfer the working sitemodel update map to the Task to allow the task finalizer to
                            // synchronise completion of this work unit in terms of persistence of
                            // all the changes to disk with the notification to the wider TRex stack
                            // that a set of subgrids have been changed
                            Task.SetAggregateModifiedSubgrids(ref WorkingModelUpdateMap);
                        }
                        finally
                        {
                            Task.AggregatedCellPasses = null;
                            WorkingModelUpdateMap = null;
                        }
                    }

                    // Use the synchonous command to save the site model information to the persistent store into the deferred (asynchronous model)
                    SiteModelFromDM.SaveToPersistentStore(storageProxy_Mutable);

                    // ====== Stage 5 : Commit all prepared data to the transactional storage proxy
                    // All operations within the transaction to integrate the changes into the live model have completed successfully.
                    // Now commit those changes as a block.
                    storageProxy_Mutable.Commit();
                }
                finally
                {
                    if (!(AnyMachineEvents || AnyCellPasses))
                    {
                        Log.LogWarning($"Suspicious task with no cell passes or machine events in Sitemodel {Task?.TargetSiteModelID}");
                    }

                    Log.LogInformation($"Aggregation Task Process --> Completed integrating {ProcessedTasks.Count} TAG files for machine {Task?.TargetMachineID} in project {Task?.TargetSiteModelID}");
                }
            }
            catch (Exception E)
            {
                Log.LogError($"Exception in ProcessTask: {E}");
                return false;
            }

            return true;
        }
    }
}
