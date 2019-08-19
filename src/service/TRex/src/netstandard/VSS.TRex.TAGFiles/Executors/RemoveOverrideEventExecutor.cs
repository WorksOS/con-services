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
    private static readonly ILogger Log = Logging.Logger.CreateLogger<OverrideEventExecutor>();

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
        if (arg.AssetID == Guid.Empty)
        {
          //If AssetID not provided, remove all override events for project
          foreach (var machine in siteModel.Machines)
          {
            result = RemoveOverrideEventsForMachine(siteModel, machine, arg);
            if (!result.Success)
              break;
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

          result = RemoveOverrideEventsForMachine(siteModel, machine, arg);        
        }

        //Notify all PSNodes something has changed
        //TODO
      }
      return result;
    }

    /// <summary>
    /// Remove requested machine design and layer override events for given machine
    /// </summary>
    private OverrideEventResponse RemoveOverrideEventsForMachine(ISiteModel siteModel, IMachine machine, OverrideEventRequestArgument arg)
    {
      var result = new OverrideEventResponse{Success = true};
      var machineTargetValues = siteModel.MachinesTargetValues[machine.InternalSiteModelMachineIndex];
      //Remove design overrides
      if (!string.IsNullOrEmpty(arg.MachineDesignName))
      {
        var overrideDesignEvents = machineTargetValues.DesignOverrideEvents;
        result = RemoveOverrideEventsForMachine(machine, arg.StartUTC, arg.EndUTC, overrideDesignEvents, OverrideEvent<int>.Null());
      }
      //Remove layer overrides
      if (arg.LayerID.HasValue && result.Success)
      {
        var overrideLayerEvents = machineTargetValues.LayerOverrideEvents;
        result = RemoveOverrideEventsForMachine(machine, arg.StartUTC, arg.EndUTC, overrideLayerEvents, OverrideEvent<ushort>.Null());
      }

      return result;
    }

    /// <summary>
    /// Remove requested override events of required type for given machine
    /// </summary>
    private OverrideEventResponse RemoveOverrideEventsForMachine<T>(IMachine machine, DateTime startUtc, DateTime endUtc, IProductionEvents<T> existingList, T nullValue)
    {
      var result = new OverrideEventResponse { Success = true };
      if (startUtc == Consts.MIN_DATETIME_AS_UTC && endUtc == Consts.MAX_DATETIME_AS_UTC)
      {
        //No date range - remove all override events of required type for machine
        //existingList.Clear();
        //Raptor - SaveToFile
      }
      else
      {
        //Find the specific event to remove
        var evt = existingList.GetValueAtDate(startUtc, out int index, nullValue);
        if (evt.Equals(OverrideEvent<int>.Null()))
        {
          //We are not able to remove override event as there is no such event
          result.Message = $"Failed to find override event to remove: AssetUid={machine.ID}, Date Range={startUtc}-{endUtc}";
          result.Success = false;
        }
        else
        {
          //Remove specific event
          //existingList.Remove(evt);
          //Raptor - SaveToFile
        }
      }

      return result;
    }
  }
}
