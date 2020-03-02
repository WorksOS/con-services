using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.TAGFiles.Types;

namespace VSS.TRex.TAGFiles.Classes.Integrator
{
  public class AggregatedDataIntegratorWorker
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<AggregatedDataIntegratorWorker>();

    /// <summary>
    /// A queue of the tasks this worker will process into the TRex data stores
    /// </summary>
    private readonly ConcurrentQueue<AggregatedDataIntegratorTask> _tasksToProcess;

    /// <summary>
    /// A bitmask sub grid tree that tracks all sub grids modified by the tasks this worker has processed
    /// </summary>
    private ISubGridTreeBitMask _workingModelUpdateMap;

    /// <summary>
    /// The mutable grid storage proxy
    /// </summary>
    private readonly IStorageProxy _storageProxyMutable = DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage();

    private static readonly bool AdviseOtherServicesOfDataModelChanges = DIContext.Obtain<IConfigurationStore>().GetValueBool("ADVISEOTHERSERVICES_OFMODELCHANGES", Consts.ADVISEOTHERSERVICES_OFMODELCHANGES);

    private static readonly int DefaultMaxMappedTagFilesToProcessPerAggregationEpoch = DIContext.Obtain<IConfigurationStore>().GetValueInt("MAX_MAPPED_TAG_FILES_TO_PROCESS_PER_AGGREGATION_EPOCH", Consts.MAX_MAPPED_TAG_FILES_TO_PROCESS_PER_AGGREGATION_EPOCH);

    public int MaxMappedTagFilesToProcessPerAggregationEpoch { get; set; } = DefaultMaxMappedTagFilesToProcessPerAggregationEpoch;

    private Guid SiteModelID { get; }

    private AggregatedDataIntegratorWorker(Guid siteModelId)
    {
      SiteModelID = siteModelId;
    }

    /// <summary>
    /// Worker constructor accepting the list of tasks for it to process.
    /// The tasks in the tasksToProcess list contain TAG files relating to a single machine's activities
    /// within a single project
    /// </summary>
    /// <param name="tasksToProcess"></param>
    /// <param name="siteModelId"></param>
    public AggregatedDataIntegratorWorker(ConcurrentQueue<AggregatedDataIntegratorTask> tasksToProcess,
      Guid siteModelId) : this(siteModelId)
    {
      _tasksToProcess = tasksToProcess;
    }

    /// <summary>
    /// Event that records a particular sub grid has been modified, identified by the address of a 
    /// cell within that sub grid
    /// </summary>
    /// <param name="cellX"></param>
    /// <param name="cellY"></param>
    private void SubGridHasChanged(int cellX, int cellY)
    {
      _workingModelUpdateMap.SetCell(cellX >> SubGridTreeConsts.SubGridIndexBitsPerLevel,
        cellY >> SubGridTreeConsts.SubGridIndexBitsPerLevel, true);
    }

    /// <summary>
    /// Processes all available tasks in the TasksToProcess list up to the maximum number the worker will accept 
    /// for any single epoch of processing TAG files.
    /// </summary>
    /// <param name="processedTasks"></param>
    /// <param name="numTagFilesRepresented"></param>
    /// <returns></returns>
    public bool ProcessTask(List<AggregatedDataIntegratorTask> processedTasks, int numTagFilesRepresented)
    {
      var eventIntegrator = new EventIntegrator();
      long totalPassCountInAggregation = 0;

      /* The task contains a set of machine events and cell passes that need to be integrated into the
        machine and site model references in the task respectively. Machine events need to be integrated
        before the cell passes that reference them are integrated.

        All other tasks in the task list that contain aggregated machine events and cell passes
        are integrated together into the machine events and site model in one operation prior to
        the modified information being committed to disk.

        A task is only said to be completed when all integrations and resulting updates are
        persisted to disk.*/

      processedTasks.Clear();

      // Set capacity to maximum expected size to prevent List resizing while assembling tasks
      processedTasks.Capacity = MaxMappedTagFilesToProcessPerAggregationEpoch;

      AggregatedDataIntegratorTask task = null;
      var sw = Stopwatch.StartNew();
      try
      {
        if (!_tasksToProcess.TryDequeue(out task) || task == null)
          return true; // There is nothing in the queue to work on so just return true

        Log.LogInformation("Aggregation Task Process: Clearing mutable storage proxy");

        _storageProxyMutable.Clear();

        // Note: This request for the SiteModel specifically asks for the mutable grid SiteModel,
        // and also explicitly provides the transactional storage proxy being used for processing the
        // data from TAG files into the model
        var siteModelFromDatamodel = DIContext.Obtain<ISiteModels>().GetSiteModel(task.PersistedTargetSiteModelID, true);

        if (siteModelFromDatamodel == null)
        {
          Log.LogError($"Unable to lock SiteModel {task.PersistedTargetSiteModelID} from the data model file");
          return false;
        }

        siteModelFromDatamodel.SetStorageRepresentationToSupply(StorageMutability.Mutable);

        task.StartProcessingTime = Consts.MIN_DATETIME_AS_UTC;

        processedTasks.Add(task); // Seed task is always a part of the processed tasks

        // First check to see if this task has been catered to by previous task processing
        var anyMachineEvents = task.AggregatedMachineEvents != null;
        var anyCellPasses = task.AggregatedCellPasses != null;

        if (!(anyMachineEvents || anyCellPasses))
        {
          Log.LogWarning($"Suspicious task with no cell passes or machine events in site model {task.PersistedTargetSiteModelID}");
          return true;
        }

        Log.LogInformation("Aggregation Task Process --> Filter tasks to aggregate");

        // ====== STAGE 1: ASSEMBLE LIST OF TAG FILES TO AGGREGATE IN ONE OPERATION

        // Populate the tasks to process list with the aggregations that will be
        // processed at this time. These tasks are also removed from the main task
        // list to allow the TAG file processors to prepare additional TAG files
        // while this set is being integrated into the model.                    

        while (processedTasks.Count < MaxMappedTagFilesToProcessPerAggregationEpoch &&
               _tasksToProcess.TryDequeue(out var taskToProcess))
        {
          processedTasks.Add(taskToProcess);
        }

        Log.LogInformation($"Aggregation Task Process --> Integrating {processedTasks.Count} TAG file processing tasks for machine {task.PersistedTargetMachineID} in project {task.PersistedTargetSiteModelID}");

        // ====== STAGE 2: AGGREGATE ALL EVENTS AND CELL PASSES FROM ALL TAG FILES INTO THE FIRST ONE IN THE LIST

        // Use the grouped sub grid tree integrator to assemble a single aggregate tree from the set of trees in the processed tasks

        Log.LogDebug($"Aggregation Task Process --> Integrate {processedTasks.Count} cell pass trees");

        IServerSubGridTree groupedAggregatedCellPasses;
        if (processedTasks.Count > 1)
        {
          var subGridTreeIntegrator = new GroupedSubGridTreeIntegrator
          {
            Trees = processedTasks
              .Where(t => t.AggregatedCellPassCount > 0)
              .Select(t => (t.AggregatedCellPasses,
                t.AggregatedMachineEvents.StartEndRecordedDataEvents.FirstStateDate(),
                t.AggregatedMachineEvents.StartEndRecordedDataEvents.LastStateDate()))
              .ToList()
          };

          // Assign the new grid into Task to represent the spatial aggregation of all of the tasks aggregated cell passes
          groupedAggregatedCellPasses = subGridTreeIntegrator.IntegrateSubGridTreeGroup();
        }
        else
        {
          groupedAggregatedCellPasses = task.AggregatedCellPasses;
        }

        #if CELLDEBUG
        groupedAggregatedCellPasses?.ScanAllSubGrids(leaf =>
        {
          foreach (var segment in ((ServerSubGridTreeLeaf)leaf).Cells.PassesData.Items)
          {
            foreach (var cell in segment.PassesData.GetState())
              cell.CheckPassesAreInCorrectTimeOrder("Cell passes not in correct order at point groupedAggregatedCellPasses is determined"); 
          }

          return true;
        });
        #endif

        Log.LogDebug("Aggregation Task Process --> Integrate machine events and other clean up cell pass trees");

        // Discard all the aggregated cell pass models for the tasks being processed as they have now been aggregated into
        // the model represented by groupedAggregatedCellPasses

        processedTasks.ForEach(x =>
        {
          if (x.AggregatedCellPasses != task.AggregatedCellPasses)
          {
            x.AggregatedCellPasses.Dispose();
            x.AggregatedCellPasses = null;
          }
        });

        // Iterate through the tasks to integrate the machine events and perform other clean up operations
        for (var I = 1; I < processedTasks.Count; I++) // Zeroth item in the list is Task
        {
          var processedTask = processedTasks[I];

          // 'Include' the extents etc of each site model being merged into 'task' into its extents and design change events
          task.IntermediaryTargetSiteModel.Include(processedTask.IntermediaryTargetSiteModel);

          // Integrate the machine events
          eventIntegrator.IntegrateMachineEvents(processedTask.AggregatedMachineEvents, task.AggregatedMachineEvents, false, 
            processedTask.IntermediaryTargetSiteModel, task.IntermediaryTargetSiteModel);

          //Update current DateTime with the latest one
          if (processedTask.IntermediaryTargetMachine.LastKnownPositionTimeStamp.CompareTo(task.IntermediaryTargetMachine.LastKnownPositionTimeStamp) == -1)
          {
            task.IntermediaryTargetMachine.LastKnownPositionTimeStamp = processedTask.IntermediaryTargetMachine.LastKnownPositionTimeStamp;
            task.IntermediaryTargetMachine.LastKnownX = processedTask.IntermediaryTargetMachine.LastKnownX;
            task.IntermediaryTargetMachine.LastKnownY = processedTask.IntermediaryTargetMachine.LastKnownY;
          }

          if (task.IntermediaryTargetMachine.MachineHardwareID == "")
            task.IntermediaryTargetMachine.MachineHardwareID = processedTask.IntermediaryTargetMachine.MachineHardwareID;

          if (task.IntermediaryTargetMachine.MachineType == 0)
            task.IntermediaryTargetMachine.MachineType = processedTask.IntermediaryTargetMachine.MachineType;
        }

        // Integrate the items present in the 'IntermediaryTargetSiteModel' into the real site model
        // read from the datamodel file itself, then synchronously write it to the DataModel

        Log.LogDebug("Aggregation Task Process --> Integrating aggregated cell passes into the live site model");

        IMachine machineFromDatamodel;
        IProductionEventLists siteModelMachineTargetValues;
        lock (siteModelFromDatamodel)
        {
          // 'Include' the extents etc of the 'task' each site model being merged into the persistent database
          siteModelFromDatamodel.Include(task.IntermediaryTargetSiteModel);

          // Need to locate or create a matching machine in the site model.
          machineFromDatamodel = siteModelFromDatamodel.Machines.Locate(task.PersistedTargetMachineID, task.IntermediaryTargetMachine.IsJohnDoeMachine);

          // Log.LogInformation($"Selecting machine: PersistedTargetMachineID={task.PersistedTargetMachineID}, IsJohnDoe?:{task.IntermediaryTargetMachine.IsJohnDoeMachine}, Result: {machineFromDatamodel}");

          if (machineFromDatamodel == null)
          {
            machineFromDatamodel = siteModelFromDatamodel.Machines.CreateNew(task.IntermediaryTargetMachine.Name,
              task.IntermediaryTargetMachine.MachineHardwareID,
              task.IntermediaryTargetMachine.MachineType,
              task.IntermediaryTargetMachine.DeviceType,
              task.IntermediaryTargetMachine.IsJohnDoeMachine,
              task.PersistedTargetMachineID);
            task.PersistedTargetMachineID = machineFromDatamodel.ID;
            task.IntermediaryTargetMachine.ID = machineFromDatamodel.ID;
            task.IntermediaryTargetMachine.InternalSiteModelMachineIndex = machineFromDatamodel.InternalSiteModelMachineIndex;
            machineFromDatamodel.Assign(task.IntermediaryTargetMachine);
          }

          // Update the internal name of the machine with the machine name from the TAG file
          if (task.IntermediaryTargetMachine.Name != "" && machineFromDatamodel.Name != task.IntermediaryTargetMachine.Name)
            machineFromDatamodel.Name = task.IntermediaryTargetMachine.Name;

          // Update the internal type of the machine with the machine type from the TAG file
          // if the existing internal machine type is zero then
          if (task.IntermediaryTargetMachine.MachineType != 0 && machineFromDatamodel.MachineType == 0)
            machineFromDatamodel.MachineType = task.IntermediaryTargetMachine.MachineType;

          // If the machine target values can't be found then create them
          siteModelMachineTargetValues = siteModelFromDatamodel.MachinesTargetValues[machineFromDatamodel.InternalSiteModelMachineIndex];

          if (siteModelMachineTargetValues == null)
            siteModelFromDatamodel.MachinesTargetValues.Add(new ProductionEventLists(siteModelFromDatamodel, machineFromDatamodel.InternalSiteModelMachineIndex));

          // Check to see the machine target values were created correctly
          siteModelMachineTargetValues = siteModelFromDatamodel.MachinesTargetValues[machineFromDatamodel.InternalSiteModelMachineIndex];
        }

        // ====== STAGE 3: INTEGRATE THE AGGREGATED EVENTS INTO THE PRIMARY LIVE DATABASE

        // Perform machine event integration outside of the SiteModel write access interlock as the
        // individual event lists have independent exclusive locks event integration uses.

        Log.LogDebug("Aggregation Task Process --> Integrating machine events into the live site model");

        eventIntegrator.IntegrateMachineEvents(task.AggregatedMachineEvents, siteModelMachineTargetValues, true, task.IntermediaryTargetSiteModel, siteModelFromDatamodel);

        // Integrate the machine events into the main site model. This requires the
        // site model interlock as aspects of the site model state (machine) are being changed.
        lock (siteModelFromDatamodel)
        {
          if (siteModelMachineTargetValues != null)
          {
            //Update machine last known value (events) from integrated model before saving
            var comparison = machineFromDatamodel.LastKnownPositionTimeStamp.CompareTo(task.IntermediaryTargetMachine.LastKnownPositionTimeStamp);
            if (comparison == -1)
            {
              machineFromDatamodel.LastKnownDesignName = siteModelFromDatamodel.SiteModelMachineDesigns[siteModelMachineTargetValues.MachineDesignNameIDStateEvents.LastStateValue()].Name;
              machineFromDatamodel.LastKnownLayerId = siteModelMachineTargetValues.LayerIDStateEvents.Count() > 0 ? siteModelMachineTargetValues.LayerIDStateEvents.LastStateValue() : (ushort) 0;
              machineFromDatamodel.LastKnownPositionTimeStamp = task.IntermediaryTargetMachine.LastKnownPositionTimeStamp;
              machineFromDatamodel.LastKnownX = task.IntermediaryTargetMachine.LastKnownX;
              machineFromDatamodel.LastKnownY = task.IntermediaryTargetMachine.LastKnownY;
            }
          }
          else
          {
            Log.LogError("SiteModelMachineTargetValues not located in aggregate machine events integrator");
            return false;
          }
        }

        // Use the synchronous command to save the machine events to the persistent store into the deferred (asynchronous model)
        siteModelMachineTargetValues.SaveMachineEventsToPersistentStore(_storageProxyMutable);

        // ====== STAGE 4: INTEGRATE THE AGGREGATED CELL PASSES INTO THE PRIMARY LIVE DATABASE
        try
        {
          Log.LogInformation($"Aggregation Task Process --> Labeling aggregated cell pass with correct machine ID for {siteModelFromDatamodel.ID}");

          // This is a dirty map for the leaf sub grids and is stored as a bitmap grid
          // with one level fewer that the sub grid tree it is representing, and
          // with cells the size of the leaf sub grids themselves. As the cell coordinates
          // we have been given are with respect to the sub grid, we must transform them
          // into coordinates relevant to the dirty bitmap sub grid tree.

          _workingModelUpdateMap = new SubGridTreeSubGridExistenceBitMask
          {
            CellSize = SubGridTreeConsts.SubGridTreeDimension * siteModelFromDatamodel.CellSize,
            ID = siteModelFromDatamodel.ID
          };

          // Integrate the cell pass data into the main site model and commit each sub grid as it is updated
          // ... first relabel the passes with the machine ID as it is set to null in the swathing engine
          groupedAggregatedCellPasses?.ScanAllSubGrids(leaf =>
          {
            var serverLeaf = (ServerSubGridTreeLeaf) leaf;

            foreach (var segment in serverLeaf.Cells.PassesData.Items)
            {
              segment.PassesData.SetAllInternalMachineIDs(machineFromDatamodel.InternalSiteModelMachineIndex, out var modifiedPassCount);
              totalPassCountInAggregation += modifiedPassCount;
            }

            return true;
          });

          // ... then integrate them
          var sw2 = Stopwatch.StartNew();
          Log.LogInformation($"Aggregation Task Process --> Integrating aggregated results for {totalPassCountInAggregation} cell passes from {numTagFilesRepresented} TAG files (spanning {groupedAggregatedCellPasses?.CountLeafSubGridsInMemory()} sub grids) into primary data model for {siteModelFromDatamodel.ID} spanning {siteModelFromDatamodel.ExistenceMap.CountBits()} sub grids");

          var subGridIntegrator = new SubGridIntegrator(groupedAggregatedCellPasses, siteModelFromDatamodel, siteModelFromDatamodel.Grid, _storageProxyMutable);
          if (!subGridIntegrator.IntegrateSubGridTree(SubGridTreeIntegrationMode.SaveToPersistentStore, SubGridHasChanged))
          {
            Log.LogError("Aggregation Task Process --> Aborting due to failure in integration process");
            return false;
          }

          Log.LogInformation($"Aggregation Task Process --> Completed integrating aggregated results into primary data model for {siteModelFromDatamodel.ID}, in elapsed time of {sw2.Elapsed}");

          TAGProcessingStatistics.IncrementTotalTAGFilesProcessedIntoModels(numTagFilesRepresented);
          TAGProcessingStatistics.IncrementTotalCellPassesAggregatedIntoModels(totalPassCountInAggregation);

          // Use the synchronous command to save the site model information to the persistent store into the deferred (asynchronous model)
          siteModelFromDatamodel.SaveToPersistentStoreForTAGFileIngest(_storageProxyMutable);

          // ====== Stage 5 : Commit all prepared data to the transactional storage proxy
          // All operations within the transaction to integrate the changes into the live model have completed successfully.
          // Now commit those changes as a block.

          var startTime = DateTime.UtcNow;
          Log.LogInformation("Starting storage proxy Commit()");
          _storageProxyMutable.Commit(out var numDeleted, out var numUpdated, out var numBytesWritten);
          Log.LogInformation($"Completed storage proxy Commit(), duration = {DateTime.UtcNow - startTime}, requiring {numDeleted} deletions, {numUpdated} updates with {numBytesWritten} bytes written");

          // Advise the segment retirement manager of any segments/sub grids that needs to be retired as as result of this integration
          Log.LogInformation($"Aggregation Task Process --> Updating segment retirement queue for {siteModelFromDatamodel.ID}");
          if (subGridIntegrator.InvalidatedSpatialStreams.Count > 0)
          {
            // Stamp all the invalidated spatial streams with the project ID
            foreach (var key in subGridIntegrator.InvalidatedSpatialStreams)
            {
              key.ProjectUID = siteModelFromDatamodel.ID;
            }

            try
            {
              var retirementQueue = DIContext.Obtain<ISegmentRetirementQueue>();

              if (retirementQueue == null)
              {
                throw new TRexTAGFileProcessingException("No registered segment retirement queue in DI context");
              }

              var insertUtc = DateTime.UtcNow;

              retirementQueue.Add(
                new SegmentRetirementQueueKey
                {
                  ProjectUID = siteModelFromDatamodel.ID,
                  InsertUTCAsLong = insertUtc.Ticks
                },
                new SegmentRetirementQueueItem
                {
                  InsertUTCAsLong = insertUtc.Ticks,
                  ProjectUID = siteModelFromDatamodel.ID,
                  SegmentKeys = subGridIntegrator.InvalidatedSpatialStreams.ToArray()
                });
            }
            catch (Exception e)
            {
              Log.LogCritical(e, "Unable to add segment invalidation list to segment retirement queue due to exception:");
              Log.LogCritical("The following segments will NOT be retired as a result:");
              foreach (var invalidatedItem in subGridIntegrator.InvalidatedSpatialStreams)
              {
                Log.LogCritical($"{invalidatedItem}");
              }
            }
          }

          if (AdviseOtherServicesOfDataModelChanges)
          {
            // Notify the site model in all contents in the grid that it's attributes have changed
            Log.LogInformation($"Aggregation Task Process --> Notifying site model attributes changed for {siteModelFromDatamodel.ID}");

            // Notify the immutable grid listeners that attributes of this site model have changed.
            var sender = DIContext.Obtain<ISiteModelAttributesChangedEventSender>();
            sender.ModelAttributesChanged
            (targetGrid: SiteModelNotificationEventGridMutability.NotifyImmutable,
              siteModelID: siteModelFromDatamodel.ID,
              existenceMapChanged: true,
              existenceMapChangeMask: _workingModelUpdateMap,
              machinesChanged: true,
              machineTargetValuesChanged: true,
              machineDesignsModified: true,
              proofingRunsModified: true);
          }

          // Update the metadata for the site model
          Log.LogInformation($"Aggregation Task Process --> Updating site model metadata for {siteModelFromDatamodel.ID}");
          DIContext.Obtain<ISiteModelMetadataManager>().Update
          (siteModelID: siteModelFromDatamodel.ID, lastModifiedDate: DateTime.UtcNow, siteModelExtent: siteModelFromDatamodel.SiteModelExtent,
            machineCount: siteModelFromDatamodel.Machines.Count);
        }
        finally
        {
          if (groupedAggregatedCellPasses != task.AggregatedCellPasses)
          {
            groupedAggregatedCellPasses?.Dispose();
          }

          task.AggregatedCellPasses?.Dispose();
          task.AggregatedCellPasses = null;
          _workingModelUpdateMap = null;
        }
      }
      finally
      {
        Log.LogInformation($"Aggregation Task Process --> Completed integrating {processedTasks.Count} TAG files and {totalPassCountInAggregation} cell passes for PersistedMachine: {task?.PersistedTargetMachineID} FinalMachine: {task?.IntermediaryTargetMachine.ID} in project {task?.PersistedTargetSiteModelID} in elapsed time of {sw.Elapsed}");
      }

      return true;
    }

    public void CompleteTaskProcessing()
    {
      Log.LogInformation($"Aggregation Task Process --> Dropping cached content for site model {SiteModelID}");

      // Finally, drop the site model context being used to perform the aggregation/integration to free up the cached
      // sub grid and segment information used during this processing epoch.
      DIContext.Obtain<ISiteModels>().DropSiteModel(SiteModelID);
    }
  }
}
