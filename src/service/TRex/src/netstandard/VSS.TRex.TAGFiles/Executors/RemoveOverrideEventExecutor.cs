using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Events.Models;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.Responses;

namespace VSS.TRex.TAGFiles.Executors
{
  /// <summary>
  /// Execute internal business logic to handle requests to remove override events
  /// </summary>
  public class RemoveOverrideEventExecutor : IOverrideEventExecutor
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<AddOverrideEventExecutor>();

    private readonly IStorageProxy storageProxy_Mutable = DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage();

    /// <summary>
    /// Inserts the override event into the site model.
    /// </summary>
    public async Task<OverrideEventResponse> ExecuteAsync(OverrideEventRequestArgument arg)
    {
      Log.LogInformation($"Override Event Executor: {arg.ProjectID}, Asset={arg.AssetID}, Date Range={arg.StartUTC}-{arg.EndUTC}");

      var result = new OverrideEventResponse {Success = false};

      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(arg.ProjectID);
      if (siteModel == null)
      {
        result.Message = $"Failed to locate site model {arg.ProjectID}";
        Log.LogError(result.Message);
        return result;
      }

      lock (siteModel)
      {
        bool changed = false;
        if (arg.AssetID == Guid.Empty)
        {
          //If AssetID not provided, remove all override events for project
          foreach (var machine in siteModel.Machines)
          {
            changed = changed || RemoveOverrideEventsForMachine(siteModel, machine, arg);
          }
        }
        else
        {
          var machine = siteModel.Machines.Locate(arg.AssetID);
          if (machine == null)
          {
            result.Message = $"Failed to locate machine {arg.AssetID}";
            Log.LogError(result.Message);
            return result;
          }

          changed = RemoveOverrideEventsForMachine(siteModel, machine, arg);        
        }

        if (changed)
        {
          //Notify all PSNodes something has changed
          //TODO
        }
      }

      result.Success = true;
      return result;
    }

    /// <summary>
    /// Remove requested machine design and layer override events for given machine
    /// </summary>
    private bool RemoveOverrideEventsForMachine(ISiteModel siteModel, IMachine machine, OverrideEventRequestArgument arg)
    {
      var changed = false;
      var machineTargetValues = siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex];
      //Remove design overrides
      if (!string.IsNullOrEmpty(arg.MachineDesignName))
      {
        var overrideDesignEvents = machineTargetValues.DesignOverrideEvents;
        if (RemoveOverrideEventsForMachine(arg.StartUTC, arg.EndUTC, overrideDesignEvents))
          changed = true;
      }
      //Remove layer overrides
      if (arg.LayerID.HasValue)
      {
        var overrideLayerEvents = machineTargetValues.LayerOverrideEvents;
        if (RemoveOverrideEventsForMachine(arg.StartUTC, arg.EndUTC, overrideLayerEvents))
          changed = true;
      }

      if (changed)
        // Use the synchronous command to save the machine events to the persistent store into the deferred (asynchronous model)
        machineTargetValues.SaveMachineEventsToPersistentStore(storageProxy_Mutable);

      return changed;
    }

    /// <summary>
    /// Remove requested override events of required type for given machine
    /// </summary>
    private bool RemoveOverrideEventsForMachine<T>(DateTime startUtc, DateTime endUtc, IProductionEvents<T> existingList)
    {
      if (startUtc == Consts.MIN_DATETIME_AS_UTC && endUtc == Consts.MAX_DATETIME_AS_UTC)
        //No date range - remove all override events of required type for machine
        existingList.Clear();
      else
        //TODO: should the remove be more rigorous i.e. check start and end date match?
        existingList.RemoveValueAtDate(startUtc);
      return existingList.EventsChanged;
    }
  }
}
