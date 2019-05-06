using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
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
    private static readonly ILogger Log = Logging.Logger.CreateLogger<AggregatedDataIntegratorWorker>();

    /// <summary>
    /// A queue of the tasks this worker will process into the TRex data stores
    /// </summary>
    private readonly ConcurrentQueue<AggregatedDataIntegratorTask> TasksToProcess;

    /// <summary>
    /// A bitmask sub grid tree that tracks all sub grids modified by the tasks this worker has processed
    /// </summary>
    private ISubGridTreeBitMask WorkingModelUpdateMap;

    /// <summary>
    /// The mutable grid storage proxy
    /// </summary>
    private readonly IStorageProxy storageProxy_Mutable = DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage();

    private readonly bool _adviseOtherServicesOfDataModelChanges = DIContext.Obtain<IConfigurationStore>().GetValueBool("ADVISEOTHERSERVICES_OFMODELCHANGES", Consts.ADVISEOTHERSERVICES_OFMODELCHANGES);

    public int MaxMappedTagFilesToProcessPerAggregationEpoch { get; set; } = DIContext.Obtain<IConfigurationStore>().GetValueInt("MAXMAPPEDTAGFILES_TOPROCESSPERAGGREGATIONEPOCH", Consts.MAXMAPPEDTAGFILES_TOPROCESSPERAGGREGATIONEPOCH);

    private Guid SiteModelID { get; set; }

    private AggregatedDataIntegratorWorker(Guid siteModelID)
    {
      SiteModelID = siteModelID;
    }

    /// <summary>
    /// Worker constructor accepting the list of tasks for it to process.
    /// The tasks in the tasksToProcess list contain TAG files relating to a single machine's activities
    /// within a single project
    /// </summary>
    /// <param name="tasksToProcess"></param>
    /// <param name="siteModelID"></param>
    public AggregatedDataIntegratorWorker(ConcurrentQueue<AggregatedDataIntegratorTask> tasksToProcess,
      Guid siteModelID) : this(siteModelID)
    {
      TasksToProcess = tasksToProcess;
    }

    /// <summary>
    /// Event that records a particular sub grid has been modified, identified by the address of a 
    /// cell within that sub grid
    /// </summary>
    /// <param name="CellX"></param>
    /// <param name="CellY"></param>
    private void SubGridHasChanged(uint CellX, uint CellY)
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
      var eventIntegrator = new EventIntegrator();

      /* The task contains a set of machine events and cell passes that need to be integrated into the
        machine and site model references in the task respectively. Machine events need to be integrated
        before the cell passes that reference them are integrated.

        All other tasks in the task list that contain aggregated machine events and cell passes
        are integrated together into the machine events and site model in one operation prior to
        the modified information being committed to disk.

        A task is only said to be completed when all integrations and resulting updates are
        persisted to disk.*/

      ProcessedTasks.Clear();

      // Set capacity to maximum expected size to prevent List resizing while assembling tasks
      ProcessedTasks.Capacity = MaxMappedTagFilesToProcessPerAggregationEpoch;

      AggregatedDataIntegratorTask Task = null;
      try
      {
        if (!TasksToProcess.TryDequeue(out Task) || Task == null)
          return true; // There is nothing in the queue to work on so just return true

        Log.LogInformation("Aggregation Task Process: Clearing mutable storage proxy");

        storageProxy_Mutable.Clear();

        // Note: This request for the SiteModel specifically asks for the mutable grid SiteModel,
        // and also explicitly provides the transactional storage proxy being used for processing the
        // data from TAG files into the model
        var SiteModelFromDM = DIContext.Obtain<ISiteModels>().GetSiteModel(Task.PersistedTargetSiteModelID, true);

        if (SiteModelFromDM == null)
        {
          Log.LogError($"Unable to lock SiteModel {Task.PersistedTargetSiteModelID} from the data model file");
          return false;
        }

        SiteModelFromDM.SetStorageRepresentationToSupply(StorageMutability.Mutable);

        Task.StartProcessingTime = Consts.MIN_DATETIME_AS_UTC;

        ProcessedTasks.Add(Task); // Seed task is always a part of the processed tasks

        // First check to see if this task has been catered to by previous task processing
        bool AnyMachineEvents = Task.AggregatedMachineEvents != null;
        bool AnyCellPasses = Task.AggregatedCellPasses != null;

        if (!(AnyMachineEvents || AnyCellPasses))
        {
          Log.LogWarning($"Suspicious task with no cell passes or machine events in site model {Task.PersistedTargetSiteModelID}");
          return true;
        }

        Log.LogInformation("Aggregation Task Process --> Filter tasks to aggregate");

        // ====== STAGE 1: ASSEMBLE LIST OF TAG FILES TO AGGREGATE IN ONE OPERATION

        // Populate the tasks to process list with the aggregations that will be
        // processed at this time. These tasks are also removed from the main task
        // list to allow the TAG file processors to prepare additional TAG files
        // while this set is being integrated into the model.                    

        while (ProcessedTasks.Count < MaxMappedTagFilesToProcessPerAggregationEpoch &&
               TasksToProcess.TryDequeue(out AggregatedDataIntegratorTask task))
        {
          ProcessedTasks.Add(task);
        }

        /*    if (TasksToProcess.Count > 0)
            {
              for (int I = 0; I < TasksToProcess.Count; I++)
              {
                if (ProcessedTasks.Count < MaxMappedTagFilesToProcessPerAggregationEpoch)
                {
                  if (TasksToProcess.TryDequeue(out AggregatedDataIntegratorTask task))
                    ProcessedTasks.Add(task);
                }
                else
                {
                  break;
                }
              }
            }*/

        Log.LogInformation($"Aggregation Task Process --> Integrating {ProcessedTasks.Count} TAG files for machine {Task.PersistedTargetMachineID} in project {Task.PersistedTargetSiteModelID}");

        // ====== STAGE 2: AGGREGATE ALL EVENTS AND CELL PASSES FROM ALL TAG FILES INTO THE FIRST ONE IN THE LIST

        for (int I = 1; I < ProcessedTasks.Count; I++) // Zeroth item in the list is Task
        {
          var processedTask = ProcessedTasks[I];

          // 'Include' the extents etc of each site model being merged into 'task' into its extents and design change events
          Task.IntermediaryTargetSiteModel.Include(processedTask.IntermediaryTargetSiteModel);

          // Integrate the machine events
          eventIntegrator.IntegrateMachineEvents(processedTask.AggregatedMachineEvents, Task.AggregatedMachineEvents, false, processedTask.IntermediaryTargetSiteModel, Task.IntermediaryTargetSiteModel);

          //Log.LogDebug($"Aggregation Task Process --> Integrate {ProcessedTasks.Count} cell pass trees");

          // Integrate the cell passes from all cell pass aggregators 
          var subGridIntegrator = new SubGridIntegrator(processedTask.AggregatedCellPasses, null, Task.AggregatedCellPasses, null);
          subGridIntegrator.IntegrateSubGridTree(SubGridTreeIntegrationMode.UsingInMemoryTarget, SubGridHasChanged);

          //Update current DateTime with the latest one
          if (processedTask.IntermediaryTargetMachine.LastKnownPositionTimeStamp.CompareTo(Task.IntermediaryTargetMachine.LastKnownPositionTimeStamp) == -1)
          {
            Task.IntermediaryTargetMachine.LastKnownPositionTimeStamp = processedTask.IntermediaryTargetMachine.LastKnownPositionTimeStamp;
            Task.IntermediaryTargetMachine.LastKnownX = processedTask.IntermediaryTargetMachine.LastKnownX;
            Task.IntermediaryTargetMachine.LastKnownY = processedTask.IntermediaryTargetMachine.LastKnownY;
          }

          if (Task.IntermediaryTargetMachine.MachineHardwareID == "")
            Task.IntermediaryTargetMachine.MachineHardwareID = processedTask.IntermediaryTargetMachine.MachineHardwareID;

          if (Task.IntermediaryTargetMachine.MachineType == 0)
            Task.IntermediaryTargetMachine.MachineType = processedTask.IntermediaryTargetMachine.MachineType;

          processedTask.AggregatedCellPasses = null;
        }

        // Integrate the items present in the 'IntermediaryTargetSiteModel' into the real site model
        // read from the datamodel file itself, then synchronously write it to the DataModel

        IMachine MachineFromDM;
        IProductionEventLists SiteModelMachineTargetValues;
        lock (SiteModelFromDM)
        {
          // 'Include' the extents etc of the 'task' each site model being merged into the persistent database
          SiteModelFromDM.Include(Task.IntermediaryTargetSiteModel);

          // Need to locate or create a matching machine in the site model.
          MachineFromDM = SiteModelFromDM.Machines.Locate(Task.PersistedTargetMachineID, Task.IntermediaryTargetMachine.IsJohnDoeMachine);

          if (MachineFromDM == null)
          {
            MachineFromDM = SiteModelFromDM.Machines.CreateNew(Task.IntermediaryTargetMachine.Name,
              Task.IntermediaryTargetMachine.MachineHardwareID,
              Task.IntermediaryTargetMachine.MachineType,
              Task.IntermediaryTargetMachine.DeviceType,
              Task.IntermediaryTargetMachine.IsJohnDoeMachine,
              Task.PersistedTargetMachineID);
            Task.PersistedTargetMachineID = MachineFromDM.ID;
            Task.IntermediaryTargetMachine.ID = MachineFromDM.ID;
            Task.IntermediaryTargetMachine.InternalSiteModelMachineIndex = MachineFromDM.InternalSiteModelMachineIndex;
            MachineFromDM.Assign(Task.IntermediaryTargetMachine);
          }

          // Update the internal name of the machine with the machine name from the TAG file
          if (Task.IntermediaryTargetMachine.Name != "" && MachineFromDM.Name != Task.IntermediaryTargetMachine.Name)
            MachineFromDM.Name = Task.IntermediaryTargetMachine.Name;

          // Update the internal type of the machine with the machine type from the TAG file
          // if the existing internal machine type is zero then
          if (Task.IntermediaryTargetMachine.MachineType != 0 && MachineFromDM.MachineType == 0)
            MachineFromDM.MachineType = Task.IntermediaryTargetMachine.MachineType;

          // If the machine target values can't be found then create them
          SiteModelMachineTargetValues = SiteModelFromDM.MachinesTargetValues[MachineFromDM.InternalSiteModelMachineIndex];

          if (SiteModelMachineTargetValues == null)
            SiteModelFromDM.MachinesTargetValues.Add(new ProductionEventLists(SiteModelFromDM, MachineFromDM.InternalSiteModelMachineIndex));

          // Check to see the machine target values were created correctly
          SiteModelMachineTargetValues = SiteModelFromDM.MachinesTargetValues[MachineFromDM.InternalSiteModelMachineIndex];
        }

        // ====== STAGE 3: INTEGRATE THE AGGREGATED EVENTS INTO THE PRIMARY LIVE DATABASE

        // Perform machine event integration outside of the SiteModel write access interlock as the
        // individual event lists have independent exclusive locks event integration uses.
        eventIntegrator.IntegrateMachineEvents(Task.AggregatedMachineEvents, SiteModelMachineTargetValues, true, Task.IntermediaryTargetSiteModel, SiteModelFromDM);

        // Integrate the machine events into the main site model. This requires the
        // site model interlock as aspects of the site model state (machine) are being changed.
        lock (SiteModelFromDM)
        {
          if (SiteModelMachineTargetValues != null)
          {
            //Update machine last known value (events) from integrated model before saving
            int Comparison = MachineFromDM.LastKnownPositionTimeStamp.CompareTo(Task.IntermediaryTargetMachine.LastKnownPositionTimeStamp);
            if (Comparison == -1)
            {
              MachineFromDM.LastKnownDesignName = SiteModelFromDM.SiteModelMachineDesigns[SiteModelMachineTargetValues.MachineDesignNameIDStateEvents.LastStateValue()].Name;
              MachineFromDM.LastKnownLayerId = SiteModelMachineTargetValues.LayerIDStateEvents.Count() > 0 ? SiteModelMachineTargetValues.LayerIDStateEvents.LastStateValue() : (ushort) 0;
              MachineFromDM.LastKnownPositionTimeStamp = Task.IntermediaryTargetMachine.LastKnownPositionTimeStamp;
              MachineFromDM.LastKnownX = Task.IntermediaryTargetMachine.LastKnownX;
              MachineFromDM.LastKnownY = Task.IntermediaryTargetMachine.LastKnownY;
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
          Log.LogInformation($"Aggregation Task Process --> Labeling aggregated cell pass with correct machine ID for {SiteModelFromDM.ID}");

          // This is a dirty map for the leaf sub grids and is stored as a bitmap grid
          // with one level fewer that the sub grid tree it is representing, and
          // with cells the size of the leaf sub grids themselves. As the cell coordinates
          // we have been given are with respect to the sub grid, we must transform them
          // into coordinates relevant to the dirty bitmap sub grid tree.

          WorkingModelUpdateMap = new SubGridTreeSubGridExistenceBitMask
          {
            CellSize = SubGridTreeConsts.SubGridTreeDimension * SiteModelFromDM.CellSize,
            ID = SiteModelFromDM.ID
          };

          long totalPassCountInAggregation = 0;
          // Integrate the cell pass data into the main site model and commit each sub grid as it is updated
          // ... first relabel the passes with the machine ID as it is set to null in the swathing engine
          Task.AggregatedCellPasses?.ScanAllSubGrids(leaf =>
          {
            var serverLeaf = (ServerSubGridTreeLeaf) leaf;

            foreach (var segment in serverLeaf.Cells.PassesData.Items)
            {
              SubGridUtilities.SubGridDimensionalIterator((x, y) =>
              {
                uint passCount = segment.PassesData.PassCount(x, y);
                for (int i = 0; i < passCount; i++)
                  segment.PassesData.SetInternalMachineID(x, y, i, MachineFromDM.InternalSiteModelMachineIndex);

                totalPassCountInAggregation += passCount;
              });
            }

            return true;
          });

          // ... then integrate them
          Log.LogInformation($"Aggregation Task Process --> Integrating aggregated results for {totalPassCountInAggregation} cell passes into primary data model for {SiteModelFromDM.ID}");

          var subGridIntegrator = new SubGridIntegrator(Task.AggregatedCellPasses, SiteModelFromDM, SiteModelFromDM.Grid, storageProxy_Mutable);
          if (!subGridIntegrator.IntegrateSubGridTree_ParallelisedTasks(SubGridTreeIntegrationMode.SaveToPersistentStore, SubGridHasChanged))
            return false; 
          //if (!subGridIntegrator.IntegrateSubGridTree(SubGridTreeIntegrationMode.SaveToPersistentStore, SubGridHasChanged))
          //  return false;

          Log.LogInformation($"Aggregation Task Process --> Completed integrating aggregated results into primary data model for {SiteModelFromDM.ID}");

          // Use the synchronous command to save the site model information to the persistent store into the deferred (asynchronous model)
          SiteModelFromDM.SaveToPersistentStoreForTAGFileIngest(storageProxy_Mutable);

          // ====== Stage 5 : Commit all prepared data to the transactional storage proxy
          // All operations within the transaction to integrate the changes into the live model have completed successfully.
          // Now commit those changes as a block.

          var startTime = DateTime.UtcNow;
          Log.LogInformation("Starting storage proxy Commit()");
          storageProxy_Mutable.Commit(out int numDeleted, out int numUpdated, out long numBytesWritten);
          Log.LogInformation($"Completed storage proxy Commit(), duration = {DateTime.UtcNow - startTime}, requiring {numDeleted} deletions, {numUpdated} updates with {numBytesWritten} bytes written");

          // Advise the segment retirement manager of any segments/sub grids that needs to be retired as as result of this integration
          Log.LogInformation($"Aggregation Task Process --> Updating segment retirement queue for {SiteModelFromDM.ID}");
          if (subGridIntegrator.InvalidatedSpatialStreams.Count > 0)
          {
            // Stamp all the invalidated spatial streams with the project ID
            foreach (var key in subGridIntegrator.InvalidatedSpatialStreams)
              key.ProjectUID = SiteModelFromDM.ID;

            try
            {
              var retirementQueue = DIContext.Obtain<ISegmentRetirementQueue>();

              if (retirementQueue == null)
              {
                throw new TRexTAGFileProcessingException("No registered segment retirement queue in DI context");
              }

              var insertUTC = DateTime.UtcNow;

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
              Log.LogCritical(e, "Unable to add segment invalidation list to segment retirement queue due to exception:");
              Log.LogCritical("The following segments will NOT be retired as a result:");
              foreach (var invalidatedItem in subGridIntegrator.InvalidatedSpatialStreams)
                Log.LogCritical($"{invalidatedItem}");
            }
          }

          if (_adviseOtherServicesOfDataModelChanges)
          {
            // Notify the site model in all contents in the grid that it's attributes have changed
            Log.LogInformation($"Aggregation Task Process --> Notifying site model attributes changed for {SiteModelFromDM.ID}");

            // Notify the immutable grid listeners that attributes of this site model have changed.
            var sender = DIContext.Obtain<ISiteModelAttributesChangedEventSender>();
            sender.ModelAttributesChanged
            (targetGrid: SiteModelNotificationEventGridMutability.NotifyImmutable,
              siteModelID: SiteModelFromDM.ID,
              existenceMapChanged: true,
              existenceMapChangeMask: WorkingModelUpdateMap,
              machinesChanged: true,
              machineTargetValuesChanged: true,
              machineDesignsModified: true,
              proofingRunsModified: true);
          }

          // Update the metadata for the site model
          Log.LogInformation($"Aggregation Task Process --> Updating site model metadata for {SiteModelFromDM.ID}");
          DIContext.Obtain<ISiteModelMetadataManager>().Update
          (siteModelID: SiteModelFromDM.ID, lastModifiedDate: DateTime.UtcNow, siteModelExtent: SiteModelFromDM.SiteModelExtent,
            machineCount: SiteModelFromDM.Machines.Count);
        }
        finally
        {
          Task.AggregatedCellPasses = null;
          WorkingModelUpdateMap = null;
        }
      }
      finally
      {
        Log.LogInformation($"Aggregation Task Process --> Completed integrating {ProcessedTasks.Count} TAG files for PersistedMachine: {Task?.PersistedTargetMachineID} FinalMachine: {Task?.IntermediaryTargetMachine.ID} in project {Task?.PersistedTargetSiteModelID}");
      }

      return true;
    }

    public void TaskProcessingComplete()
    {
      Log.LogInformation($"Aggregation Task Process --> Dropping cached content for site model {SiteModelID}");
      // Finally, drop the site model context being used to perform the aggregation/integration to free up the cached
      // sub grid and segment information used during this processing epoch.
      DIContext.Obtain<ISiteModels>().DropSiteModel(SiteModelID);
    }
  }
}
