using System;
using System.Collections.Generic;
using System.Linq;
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
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = CastRequestObjectTo<ProjectID>(item);
      log.LogInformation(
        $"GetMachineIdsExecutor: {JsonConvert.SerializeObject(request)}, UseTRexGateway: {UseTRexGateway("ENABLE_TREX_GATEWAY_MACHINES")}");

      MachineExecutionResult machinesResult = null;

#if RAPTOR
      if (UseTRexGateway("ENABLE_TREX_GATEWAY_MACHINES"))
#endif
      {
        var siteModelId = request.ProjectUid.ToString();

        machinesResult = trexCompactionDataProxy
          .SendDataGetRequest<MachineExecutionResult>(siteModelId, $"/sitemodels/{siteModelId}/machines", customHeaders)
          .Result;
        PairUpAssetIdentifiers(machinesResult, false);
        return machinesResult;
      }

#if RAPTOR
      if (request.ProjectId.HasValue)
      {
        TMachineDetail[] machines = raptorClient.GetMachineIDs(request.ProjectId ?? -1);

        if (machines != null)
        {
          machinesResult =
            MachineExecutionResult.CreateMachineExecutionResult(convertMachineStatus(machines).ToArray());
          PairUpAssetIdentifiers(machinesResult, true);
          return machinesResult;
        }
      }
#endif
      throw CreateServiceException<GetMachineIdsExecutor>();
    }

    private void PairUpAssetIdentifiers(MachineExecutionResult machinesResult, bool haveIds)
    {
      if (machinesResult?.MachineStatuses == null || machinesResult.MachineStatuses.Length == 0)
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
    private IEnumerable<MachineStatus> convertMachineStatus(TMachineDetail[] machines)
    {
      foreach (TMachineDetail machineDetail in machines)
        yield return
            MachineStatus.CreateMachineStatus(
            machineDetail.ID,
            machineDetail.Name,
            machineDetail.IsJohnDoeMachine,
            string.IsNullOrEmpty(machineDetail.LastKnownDesignName) ? null : machineDetail.LastKnownDesignName,
            machineDetail.LastKnownLayerId == 0 ? (ushort?)null : machineDetail.LastKnownLayerId,
            machineDetail.LastKnownTimeStamp.ToDateTime() == ConversionConstants.PDS_MIN_DATE ? (DateTime?)null : machineDetail.LastKnownTimeStamp.ToDateTime(),
            machineDetail.LastKnownLat == 0 ? (double?)null : machineDetail.LastKnownLat,
            machineDetail.LastKnownLon == 0 ? (double?)null : machineDetail.LastKnownLon,
            machineDetail.LastKnownX == 0 ? (double?)null : machineDetail.LastKnownX,
            machineDetail.LastKnownY == 0 ? (double?)null : machineDetail.LastKnownY
            );
    }
#endif
  }
}
