using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
#if RAPTOR
using VLPDDecls;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
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

      var machines = new List<MachineStatus>();
      bool haveIds = false;

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
          throw CreateServiceException<GetMachineIdsExecutor>();
      }

#if RAPTOR
      else
      {
        if (request.ProjectId.HasValue && request.ProjectId >= 1)
        {
          haveIds = true;
          TMachineDetail[] tMachines = raptorClient.GetMachineIDs(request.ProjectId ?? -1);

          if (tMachines == null || tMachines.Length == 0)
            return new MachineExecutionResult(new List<MachineStatus>());

          machines = ConvertMachineStatus(tMachines);
        }
        else
          throw CreateServiceException<GetMachineIdsExecutor>();
      }
#endif

      PairUpAssetIdentifiers(machines, haveIds);
      return new MachineExecutionResult(machines);
    }

    private void PairUpAssetIdentifiers(List<MachineStatus> machines, bool haveIds)
    {
      if (machines == null || machines.Count == 0)
        return;

      // todoJeannie get assetList from AssetService and match e.g. longs with Uids

      // note that new assets (since Gen3) will not have a valid legacyId. It will be null/-1/0. set to -1?
      //if (haveIds)
      // { }
      //  else
      // { }
      return;
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
