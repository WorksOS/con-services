using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.Cells;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Events.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Responses;

namespace VSS.TRex.TAGFiles.Executors
{
  /// <summary>
  /// Execute internal business logic to handle requests to add override events
  /// </summary>
  public class OverrideEventExecutor : IOverrideEventExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<OverrideEventExecutor>();

    private readonly IStorageProxy storageProxy_Mutable = DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage();

    /// <summary>
    /// Inserts the override event into the site model.
    /// </summary>
    public async Task<OverrideEventResponse> ExecuteAsync(OverrideEventRequestArgument arg)
    {
      Log.LogInformation($"Override Event Executor: {arg.ProjectID}, Asset={arg.AssetID}, Date Range={arg.StartUTC}-{arg.EndUTC}");

      var result = new OverrideEventResponse {Success = false};

      //TODO: Do we need the lock around everything?

      if (arg.StartUTC >= arg.EndUTC)
      {
        result.Message = $"Invalid date range. Start:{arg.StartUTC} End:{arg.EndUTC}";
        return result;
      }

      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(arg.ProjectID);
      if (siteModel == null)
      {
        result.Message = $"Failed to locate site model {arg.ProjectID}";
        Log.LogError(result.Message);
        return result;
      }
      var machine = siteModel.Machines.Locate(arg.AssetID);
      if (machine == null)
      {
        result.Message = $"Failed to locate machine {arg.AssetID}";
        Log.LogError(result.Message);
        return result;
      }

      if (string.IsNullOrEmpty(arg.MachineDesignName) && !arg.LayerID.HasValue)
      {
        result.Message = "Missing override values";
        Log.LogError(result.Message);
        return result;
      }

      //Test targets exist
      var machineTargetValues = siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex];
      //TODO: How do we check targets exist here? (Always returns list of event lists though may have nothing in it)
      /*
      if (xxx)
      {
        result.Message = $"No target values found to override";
        Log.LogError(result.Message);
        return result;
      }
      */

      // Now we should check there are no overlapping override events for a new event in this list
      if (arg.LayerID.HasValue && 
          !ValidateNewOverrideEventAgainstExistingOverridingEvents(machineTargetValues.LayerOverrideEvents, arg, CellEvents.NullLayerID))
      {
        //We are not able to override event as there is already overridden event
        result.Message = $"Layer override failed event date validation {arg.StartUTC}-{arg.EndUTC}";
        Log.LogError(result.Message);
        return result;
      }
      if (!string.IsNullOrEmpty(arg.MachineDesignName) && 
          !ValidateNewOverrideEventAgainstExistingOverridingEvents(machineTargetValues.DesignOverrideEvents, arg, Consts.kNoDesignNameID))
      {
        result.Message = $"Design override failed event date validation {arg.StartUTC}-{arg.EndUTC}";
        Log.LogError(result.Message);
        return result;
      }

      lock (siteModel)
      {
        //Override lift
        if (arg.LayerID.HasValue)
        {
          machineTargetValues.LayerOverrideEvents.PutValueAtDate(arg.StartUTC, new OverrideEvent<ushort>(arg.EndUTC, arg.LayerID.Value));
        }
        //Override machine design
        if (!string.IsNullOrEmpty(arg.MachineDesignName))
        {
          //TODO: do we need to lock this as whole site model is locked.
          lock (siteModel.SiteModelMachineDesigns)
          {
            //Try to find corresponding designId
            var siteModelMachineDesign = siteModel.SiteModelMachineDesigns.Locate(arg.MachineDesignName);
            if (siteModelMachineDesign == null)
            {
              siteModelMachineDesign = siteModel.SiteModelMachineDesigns.CreateNew(arg.MachineDesignName);
            }

            machineTargetValues.DesignOverrideEvents.PutValueAtDate(arg.StartUTC, new OverrideEvent<int>(arg.EndUTC, siteModelMachineDesign.Id));
          }
        }
        //Raptor - SaveToFile

        //TODO Do we need this inside the designs lock
        machineTargetValues.SaveMachineEventsToPersistentStore(storageProxy_Mutable);
        siteModel.SaveToPersistentStoreForTAGFileIngest(storageProxy_Mutable);

        //Notify the site model details have changed
        //TODO: ASk Raymond what's needed - see AggregatedDataIntegratorWorker

        result.Success = true;
      }

      return result;
    }

    /// <summary>
    /// Checks the new override event has a valid date range. It cannot overlap existing override events.
    /// </summary>
    private bool ValidateNewOverrideEventAgainstExistingOverridingEvents<T>(IProductionEvents<OverrideEvent<T>> existingList, OverrideEventRequestArgument arg, T defaultValue)
    {
      //Test if we have override event at the same date
      var value = existingList.GetValueAtDate(arg.StartUTC, out _, OverrideEvent<T>.Null());
      if (!value.Equals(defaultValue))
        return false;

      //Test if we override event within the range or we have overlapping overriding
      //Test if we have previous override event which overlaps start date of the new one
      //Test if we have overlapping for the start date of the event
      var index = existingList.IndexOfClosestEventPriorToDate(arg.StartUTC);
      if (index > 0)
      {
        existingList.GetStateAtIndex(index, out _, out OverrideEvent<T> evt);
        if (evt.EndDate >= arg.StartUTC)
          return false;
      }
      //Test if we have overlapping for the end date of the event
      index = existingList.IndexOfClosestEventSubsequentToDate(arg.EndUTC);
      if (index > 0)
      {
        existingList.GetStateAtIndex(index, out DateTime startDate, out _);
        if (startDate <= arg.EndUTC)
          return false;
      }

      return true;
    }


  }
}
