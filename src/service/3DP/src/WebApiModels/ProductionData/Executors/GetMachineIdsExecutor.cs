using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Extensions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Productivity3D.Models.ProductionData;
using VSS.Productivity3D.Productivity3D.Models.ProductionData.ResultHandling;
using VSS.TRex.Common;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  public class GetMachineIdsExecutor : RequestExecutorContainer
  {

    /// <summary>
    /// Get list of Machines from trex for projectUid
    ///    resolve legacy (short) machine (device/asset) ids for TBC
    /// Note that a request will always include both projectUID and ProjectID (soon to be obsolete)
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var projectIds = CastRequestObjectToProjectIDs(item);
      log.LogInformation(
        $"GetMachineIdsExecutor: {JsonConvert.SerializeObject(projectIds)}");

      var machines = await GetTrexMachines(projectIds.ProjectUid.ToString());

      PairUpAssetIdentifiers(machines);
      return new MachineExecutionResult(machines);
    }

    private async Task<List<MachineStatus>> GetTrexMachines(string projectUid)
    {
      return ( await trexCompactionDataProxy
        .SendDataGetRequest<MachineExecutionResult>(projectUid, $"/sitemodels/{projectUid}/machines",
          customHeaders)).MachineStatuses;
    }
    
    private void PairUpAssetIdentifiers(List<MachineStatus> machines)
    {
      if (machines == null || machines.Count == 0)
        return;

      foreach (var machine in machines)
      {
        if (machine.AssetUid.HasValue && machine.AssetUid.Value != Guid.Empty)
          machine.AssetId = machine.AssetUid.Value.ToLegacyId();
        else
          machine.AssetId = Consts.NULL_LEGACY_ASSETID;
      }
    }
  }
}
