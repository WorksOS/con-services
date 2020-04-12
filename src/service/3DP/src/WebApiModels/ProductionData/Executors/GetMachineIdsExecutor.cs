using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
#if RAPTOR
using VLPDDecls;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.ProductionData;
using VSS.Productivity3D.Productivity3D.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  public class GetMachineIdsExecutor : RequestExecutorContainer
  {

    /// <summary>
    /// Get list of Machines using required pathway tRex/raptor
    ///    resolve non-JohnDoe assetID/Uid using assetResolver
    ///    resolve JohnDoe assetID/Uid using opposite tRex/raptor pathway
    /// Note that a request will always include both projectUID and ProjectID
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var projectIds = CastRequestObjectToProjectIDs(item);
      log.LogInformation(
        $"GetMachineIdsExecutor: {JsonConvert.SerializeObject(projectIds)}, UseTRexGateway: {UseTRexGateway("ENABLE_TREX_GATEWAY_MACHINES")}");

      List<MachineStatus> machines;
      bool haveUids = true;

#if RAPTOR
      if (UseTRexGateway("ENABLE_TREX_GATEWAY_MACHINES"))
#endif
        machines = await GetTrexMachines(projectIds.ProjectUid.ToString());

#if RAPTOR
      else
      {
        haveUids = false;
        machines = GetRaptorMachines(projectIds.ProjectId);
      }
#endif

      await PairUpAssetIdentifiers(projectIds, machines, haveUids);
      return new MachineExecutionResult(machines);
    }

    // if primary call is intended for raptor i.e. #RAPTOR and UseTRexGateway == false
    //    TREX_IS_AVAILABLE indicates that a TRex service is available, and can be used for resolving JohnDoes 
    private async Task<List<MachineStatus>> GetTrexMachines(string projectUid)
    {
      var machinesResult = new MachineExecutionResult(new List<MachineStatus>());
      if (IsTRexAvailable("TREX_IS_AVAILABLE") || UseTRexGateway("ENABLE_TREX_GATEWAY_MACHINES"))
        machinesResult = await trexCompactionDataProxy
        .SendDataGetRequest<MachineExecutionResult>(projectUid, $"/sitemodels/{projectUid}/machines",
          customHeaders);
      return machinesResult.MachineStatuses;
    }

#if RAPTOR
    private List<MachineStatus> GetRaptorMachines(long projectId)
    {
      TMachineDetail[] tMachines = raptorClient.GetMachineIDs(projectId);

      if (tMachines == null || tMachines.Length == 0)
        return new List<MachineStatus>();

      return ConvertMachineStatus(tMachines);
    }
#endif

    private async Task PairUpAssetIdentifiers(ProjectIDs request, List<MachineStatus> machines, bool haveUids)
    {
      if (machines == null || machines.Count == 0)
        return;

      await PairUpVSSAssets(machines, haveUids);
      await PairUpJohnDoeAssets(request, machines, haveUids);

      var unMatchedList = (machines.Where(a => !a.AssetUid.HasValue || a.AssetUid.Value == Guid.Empty || a.AssetId < 1)
        .Select(a => new {assetUid = a.AssetUid, assetId = a.AssetId, isJohnDoe = a.IsJohnDoe, machineName = a.MachineName})
        .Distinct()).ToList();
      if (unMatchedList.Any())
        log.LogWarning($"PairUpAssetIdentifiers: UnmatchedAssetCount: {unMatchedList.Count} MatchedAssetCount: {machines.Count - unMatchedList.Count} UnableToMatchAllAssets: {JsonConvert.SerializeObject(unMatchedList)}");
    }

    private async Task PairUpVSSAssets(List<MachineStatus> machines, bool haveUids)
    {
      if (haveUids)
      {
        // VSS assetMatch will return rows if Uids found, however the legacyAssetIds may be invalid
        var assetUids = new List<Guid>(machines.Where(a => a.AssetUid.HasValue && a.AssetUid.Value != Guid.Empty && !a.IsJohnDoe).Select(a => a.AssetUid.Value).Distinct());
        if (assetUids.Count > 0)
        {
          var assetMatchingResult = (await deviceProxy.GetMatchingDevices(assetUids, customHeaders)).ToList();
          foreach (var assetMatch in assetMatchingResult)
          {
            if (assetMatch.Value > 0)
              foreach (var assetOnDesignPeriod in machines.FindAll(x => x.AssetUid == assetMatch.Key))
                assetOnDesignPeriod.AssetId = assetMatch.Value;
          }
        }
      }
      else
      {
        // VSS assetMatch will only return rows if Uids found for the legacyAssetIds
        var assetIds = new List<long>(machines.Where(a => a.AssetId > 0 && !a.IsJohnDoe).Select(a => a.AssetId).Distinct());
        if (assetIds.Count > 0)
        {
          var assetMatchingResult = (await deviceProxy.GetMatchingDevices(assetIds, customHeaders)).ToList();
          foreach (var assetMatch in assetMatchingResult)
          {
            if (assetMatch.Value > 0) // machineId of 0/-1 may occur for >1 AssetUid
              foreach (var assetOnDesignPeriod in machines.FindAll(x => x.AssetId == assetMatch.Value))
                assetOnDesignPeriod.AssetUid = assetMatch.Key;
          }
        }
      }
    }

    private async Task PairUpJohnDoeAssets(ProjectIDs request, List<MachineStatus> machines, bool haveUids)
    {
      if (haveUids)
      {
#if RAPTOR
        // JohnDoe assetMatch looks in Raptor machines for JohnDoe with matching name
        var johnDoeAssets = new List<string>(
          machines.Where(a => a.AssetUid.HasValue && a.AssetUid.Value != Guid.Empty && a.IsJohnDoe)
            .Select(a => a.MachineName)
            .Distinct());
        if (johnDoeAssets.Count > 0)
        {
          var assetMatchingResult = GetRaptorMachines(request.ProjectId).Where(a => a.IsJohnDoe).ToList();
          foreach (var assetMatch in assetMatchingResult)
          {
            foreach (var assetOnDesignPeriod in machines.FindAll(x => x.MachineName.Equals(assetMatch.MachineName, StringComparison.OrdinalIgnoreCase)))
              assetOnDesignPeriod.AssetId = assetMatch.AssetId;
          }
        }
#endif
      }
      else
      {
        // JohnDoe assetMatch looks in TRex machines for JohnDoe with matching name
        var johnDoeAssets = new List<string>(
          machines.Where(a => a.AssetId > 0 && a.IsJohnDoe)
            .Select(a => a.MachineName)
            .Distinct());
        if (johnDoeAssets.Count > 0)
        {
          var assetMatchingResult = (await GetTrexMachines(request.ProjectUid.ToString())).Where(a => a.IsJohnDoe);
          foreach (var assetMatch in assetMatchingResult)
          {
            foreach (var assetOnDesignPeriod in machines.FindAll(x => x.MachineName.Equals(assetMatch.MachineName, StringComparison.OrdinalIgnoreCase)))
              assetOnDesignPeriod.AssetUid = assetMatch.AssetUid;
          }
        }
      }
    }

#if RAPTOR
    private List<MachineStatus> ConvertMachineStatus(TMachineDetail[] tMachines)
    {
      var machines = new List<MachineStatus>(tMachines.Length);

      for (var i = 0; i < tMachines.Length; i++)
      {
        machines.Add(new MachineStatus
        (
          tMachines[i].ID,
          tMachines[i].Name,
          tMachines[i].IsJohnDoeMachine,
          string.IsNullOrEmpty(tMachines[i].LastKnownDesignName) ? null : tMachines[i].LastKnownDesignName,
          tMachines[i].LastKnownLayerId == 0 ? (ushort?) null : tMachines[i].LastKnownLayerId,
          tMachines[i].LastKnownTimeStamp.ToDateTime() == ConversionConstants.PDS_MIN_DATE
            ? (DateTime?) null
            : tMachines[i].LastKnownTimeStamp.ToDateTime(),
          tMachines[i].LastKnownLat == 0 ? (double?) null : tMachines[i].LastKnownLat,
          Math.Abs(tMachines[i].LastKnownLon) < 0.001 ? (double?) null : tMachines[i].LastKnownLon,
          Math.Abs(tMachines[i].LastKnownX) < 0.001 ? (double?) null : tMachines[i].LastKnownX,
          Math.Abs(tMachines[i].LastKnownY) < 0.001 ? (double?) null : tMachines[i].LastKnownY,
          null
        ));
      }

      return machines;
    }
#endif

  }
}
