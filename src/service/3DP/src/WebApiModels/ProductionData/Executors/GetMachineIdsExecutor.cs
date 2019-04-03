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
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Models.Utilities;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  public class GetMachineIdsExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<ProjectID>(item);
      log.LogInformation(
        $"GetMachineIdsExecutor: {JsonConvert.SerializeObject(request)}, UseTRexGateway: {UseTRexGateway("ENABLE_TREX_GATEWAY_MACHINES")}");

      List<MachineStatus> machines;
      bool haveUids = true;

#if RAPTOR
      if (UseTRexGateway("ENABLE_TREX_GATEWAY_MACHINES"))
#endif
      {
        if (request.ProjectUid.HasValue && request.ProjectUid != Guid.Empty)
        {
          var siteModelId = request.ProjectUid.ToString();

          var machinesResult = await trexCompactionDataProxy
            .SendDataGetRequest<MachineExecutionResult>(siteModelId, $"/sitemodels/{siteModelId}/machines",
              customHeaders);
          machines = machinesResult.MachineStatuses;
        }
        else
        {
          log.LogError($"GetMachineIdsExecutor: No projectUid provided. ");
          throw CreateServiceException<GetMachineIdsExecutor>();
        }
      }

#if RAPTOR
      else
      {
        if (request.ProjectId.HasValue && request.ProjectId >= 1)
        {
          haveUids = false;
          TMachineDetail[] tMachines = raptorClient.GetMachineIDs(request.ProjectId ?? -1);

          if (tMachines == null || tMachines.Length == 0)
            return new MachineExecutionResult(new List<MachineStatus>());

          machines = ConvertMachineStatus(tMachines);
        }
        else
        {
          log.LogError($"GetMachineIdsExecutor: No projectId provided. ");
          throw CreateServiceException<GetMachineIdsExecutor>();
        }
      }
#endif

      await PairUpAssetIdentifiers(machines, haveUids);
      return new MachineExecutionResult(machines);
    }

    private async Task PairUpAssetIdentifiers(List<MachineStatus> machines, bool haveUids)
    {
      if (machines == null || machines.Count == 0)
        return;

      if (haveUids)
      {
        // assetMatch will return rows if Uids found, however the legacyAssetIds may be invalid
        var assetUids = new List<Guid>(machines.Where(a => a.AssetUid.HasValue && a.AssetUid.Value != Guid.Empty).Select(a => a.AssetUid.Value).Distinct());
        var assetMatchingResult = (await assetResolverProxy.GetMatchingAssets(assetUids, customHeaders)).ToList();
        foreach (var assetMatch in assetMatchingResult)
        {
          if (assetMatch.Value > 0)
            foreach (var assetOnDesignPeriod in machines.FindAll(x => x.AssetUid == assetMatch.Key))
              assetOnDesignPeriod.AssetId = assetMatch.Value;
        }
      }
      else
      {
        // assetMatch will only return rows if Uids found for the legacyAssetIds
        var assetIds = new List<long>(machines.Where(a => a.AssetId > 0).Select(a => a.AssetId).Distinct());
        var assetMatchingResult = (await assetResolverProxy.GetMatchingAssets(assetIds, customHeaders)).ToList();
        foreach (var assetMatch in assetMatchingResult)
        {
          if (assetMatch.Value > 0) // machineId of 0/-1 may occur for >1 AssetUid
            foreach (var assetOnDesignPeriod in machines.FindAll(x => x.AssetId == assetMatch.Value))
              assetOnDesignPeriod.AssetUid = assetMatch.Key;
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
          tMachines[i].LastKnownLon == 0 ? (double?) null : tMachines[i].LastKnownLon,
          tMachines[i].LastKnownX == 0 ? (double?) null : tMachines[i].LastKnownX,
          tMachines[i].LastKnownY == 0 ? (double?) null : tMachines[i].LastKnownY,
          null
        ));
      }

      return machines;
    }
#endif

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
