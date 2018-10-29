using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.TAGFiles.Models;
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

    private bool _adviseOtherServicesOfDataModelChanges = Consts.kAdviseOtherServicesOfDataModelChangesDefault;

    private int _maxMappedTagFilesToProcessPerAggregationEpoch = Consts.kMaxMappedTagFilesToProcessPerAggregationEpochDefault;
    
    private void ReadEnvironmentVariables()
    {
      var config = DIContext.Obtain<IConfigurationStore>();
      var configResultBool = config.GetValueBool("ADVISEOTHERSERVICES_OFMODELCHANGES");
      if (configResultBool != null)
      {
        _adviseOtherServicesOfDataModelChanges = configResultBool.Value;
      }
      var configResultInt = config.GetValueInt("MAXMAPPEDTAGFILES_TOPROCESSPERAGGREGATIONEPOCH");
      if (configResultInt > -1)
      {
        _maxMappedTagFilesToProcessPerAggregationEpoch = configResultInt;
      }
    }

    /// <summary>
    /// Worker constructor that obtains the necessary storage proxies
    /// </summary>
    public AggregatedDataIntegratorWorker()
    {
      ReadEnvironmentVariables();
    }

    /// <summary>
    /// Worker constructor accepting the list of tasks for it to process.
    /// The tasks in the tasksToProcess list contain TAG files relating to a single machine's activities
    /// within a single project
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
      ProcessedTasks.Capacity = _maxMappedTagFilesToProcessPerAggregationEpoch;

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
          // and also explicitly provides the transactional storage proxy being used for processing the
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
          bool AnyMachineEvents = Task.AggregatedMachineEvents != null;
          bool AnyCellPasses = Task.AggregatedCellPasses != null;

          if (!(AnyMachineEvents || AnyCellPasses))
          {
            Log.LogWarning($"Suspicious task with no cell passes or machine events in Sitemodel {Task.TargetSiteModelID}");

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
              if (ProcessedTasks.Count < _maxMappedTagFilesToProcessPerAggregationEpoch)
              {
                if (TasksToProcess.TryDequeue(out AggregatedDataIntegratorTask task))
                  ProcessedTasks.Add(task);
              }
              else
              {
                break;
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
            eventIntegrator.IntegrateMachineEvents(processedTask.AggregatedMachineEvents, Task.AggregatedMachineEvents, false, processedTask.TargetSiteModel, Task.TargetSiteModel);

            //Log.LogDebug($"Aggregation Task Process --> Integrate {ProcessedTasks.Count} cell pass trees");

            // Integrate the cell passes from all cell pass aggregators 
            SubGridIntegrator subGridIntegrator = new SubGridIntegrator(processedTask.AggregatedCellPasses, null, Task.AggregatedCellPasses, null);
            subGridIntegrator.IntegrateSubGridTree(SubGridTreeIntegrationMode.UsingInMemoryTarget, SubgridHasChanged);

            //Update current DateTime with the latest one
            if (processedTask.TargetMachine.LastKnownPositionTimeStamp.CompareTo(Task.TargetMachine.LastKnownPositionTimeStamp) == -1)
            {
              Task.TargetMachine.LastKnownPositionTimeStamp = processedTask.TargetMachine.LastKnownPositionTimeStamp;
              Task.TargetMachine.LastKnownX = processedTask.TargetMachine.LastKnownX;
              Task.TargetMachine.LastKnownY = processedTask.TargetMachine.LastKnownY;
            }

            if (Task.TargetMachine.MachineHardwareID == "")
              Task.TargetMachine.MachineHardwareID = processedTask.TargetMachine.MachineHardwareID;

            if (Task.TargetMachine.MachineType == 0)
              Task.TargetMachine.MachineType = processedTask.TargetMachine.MachineType;

            processedTask.AggregatedCellPasses = null;
          }

          // Integrate the items present in the 'TargetSiteModel' into the real sitemodel
          // read from the datamodel file itself, then synchronously write it to the DataModel

          IMachine MachineFromDM;
          IProductionEventLists SiteModelMachineTargetValues;
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

            // If the machine target values can't be found then create them
            SiteModelMachineTargetValues = SiteModelFromDM.MachinesTargetValues[MachineFromDM.InternalSiteModelMachineIndex];

            if (SiteModelMachineTargetValues == null)
            {
              SiteModelFromDM.MachinesTargetValues.Add(new ProductionEventLists(SiteModelFromDM, MachineFromDM.InternalSiteModelMachineIndex));
            }

            // Check to see the machine target values were created correctly
            SiteModelMachineTargetValues = SiteModelFromDM.MachinesTargetValues[MachineFromDM.InternalSiteModelMachineIndex];
          }

          // ====== STAGE 3: INTEGRATE THE AGGREGATED EVENTS INTO THE PRIMARY LIVE DATABASE

          // Perform machine event integration outside of the SiteModel write access interlock as the
          // individual event lists have independent exclusive locks event integration uses.
          eventIntegrator.IntegrateMachineEvents(Task.AggregatedMachineEvents, SiteModelMachineTargetValues, true, Task.TargetSiteModel, SiteModelFromDM);

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
                MachineFromDM.LastKnownDesignName = SiteModelFromDM.SiteModelMachineDesigns[SiteModelMachineTargetValues.MachineDesignNameIDStateEvents.LastStateValue()].Name;

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

          // Use the synchronous command to save the machine events to the persistent store into the deferred (asynchronous model)
          SiteModelMachineTargetValues.SaveMachineEventsToPersistentStore(storageProxy_Mutable);

          // ====== STAGE 4: INTEGRATE THE AGGREGATED CELL PASSES INTO THE PRIMARY LIVE DATABASE
          try
          {
            // This is a dirty map for the leaf subgrids and is stored as a bitmap grid
            // with one level fewer that the subgrid tree it is representing, and
            // with cells the size of the leaf subgrids themselves. As the cell coordinates
            // we have been given are with respect to the subgrid, we must transform them
            // into coordinates relevant to the dirty bitmap subgrid tree.

            WorkingModelUpdateMap = new SubGridTreeSubGridExistenceBitMask
            {
              CellSize = SubGridTreeConsts.SubGridTreeDimension * SiteModelFromDM.Grid.CellSize,
              ID = SiteModelFromDM.ID
            };

            // Integrate the cell pass data into the main sitemodel and commit each subgrid as it is updated
            // ... first reliable the passes with the machine ID
            Task.AggregatedCellPasses?.ScanAllSubGrids(leaf =>
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

            // Use the synchronous command to save the site model information to the persistent store into the deferred (asynchronous model)
            SiteModelFromDM.SaveToPersistentStore(storageProxy_Mutable);

            // ====== Stage 5 : Commit all prepared data to the transactional storage proxy
            // All operations within the transaction to integrate the changes into the live model have completed successfully.
            // Now commit those changes as a block.

            var startTime = DateTime.Now;
            Log.LogInformation("Starting storage proxy Commit()");
            storageProxy_Mutable.Commit(out int numDeleted, out int numUpdated, out long numBytesWritten);
            Log.LogInformation($"Completed storage proxy Commit(), duration = {DateTime.Now - startTime}, requiring {numDeleted} deletions, {numUpdated} updates with {numBytesWritten} bytes written");

            // Advise the segment retirement manager of any segments/subgrids that needs to be retired as as result of this integration

            if (subGridIntegrator.InvalidatedSpatialStreams.Count > 0)
            {
              // Stamp all the invalidated spatial streams with the project ID
              foreach (var key in subGridIntegrator.InvalidatedSpatialStreams)
                key.ProjectUID = SiteModelFromDM.ID;

              try
              {
                ISegmentRetirementQueue retirementQueue = DIContext.Obtain<ISegmentRetirementQueue>();

                if (retirementQueue == null)
                {
                  Log.LogCritical("No registered segment retirement queue in DI context");
                  Debug.Assert(false, "No registered segment retirement queue in DI context");
                }

                DateTime insertUTC = DateTime.Now;

                retirementQueue.Add(new SegmentRetirementQueueKey
                  {
                    ProjectUID = SiteModelFromDM.ID,
                    InsertUTCAsLong = insertUTC.Ticks
                  },
                  new SegmentRetirementQueueItem
                  {
                    InsertUTCAsLong = insertUTC.Ticks,
                    ProjectUID = SiteModelFromDM.ID,
                    SegmentKeys = subGridIntegrator.InvalidatedSpatialStreams.ToArray()
                  });
              }
              catch (Exception e)
              {
                Log.LogCritical($"Unable to add segment invalidation list to segment retirement queue due to exception: {e}");
                Log.LogCritical("The following segments will NOT be retired as a result:");
                foreach (var invalidatedItem in subGridIntegrator.InvalidatedSpatialStreams)
                  Log.LogCritical($"{invalidatedItem}");
              }
            }
          }
          finally
          {
            Task.AggregatedCellPasses = null;
            WorkingModelUpdateMap = null;
          }

          if (_adviseOtherServicesOfDataModelChanges)
          {
            // Notify the site model in all contents in the grid that it's attributes have changed
            Log.LogInformation($"Notifying site model attributes changed for {SiteModelFromDM.ID}");

            // Notify the immutable grid listeners that attributes of this sitemodel have changed.
            var sender = DIContext.Obtain<ISiteModelAttributesChangedEventSender>();
            sender.ModelAttributesChanged(SiteModelNotificationEventGridMutability.NotifyImmutable, SiteModelFromDM.ID,
              existenceMapChanged: true, machinesChanged: true, machineTargetValuesChanged: true, machineDesignsModified: true);
          }

          // Update the metadata for the site model
          Log.LogInformation($"Updating site model metadata for {SiteModelFromDM.ID}");
          DIContext.Obtain<ISiteModelMetadataManager>().Update
          (siteModelID: SiteModelFromDM.ID, lastModifiedDate: DateTime.Now, siteModelExtent: SiteModelFromDM.SiteModelExtent,
            machineCount: SiteModelFromDM.Machines.Count);
        }
        finally
        {
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
