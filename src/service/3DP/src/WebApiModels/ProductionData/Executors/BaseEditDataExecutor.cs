using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  /// <summary>
  /// Common code for all edit data executors
  /// </summary>
  public abstract class BaseEditDataExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Matches up the legacy asset ID with the corresponding asset UID.
    /// </summary>
    protected async Task<Guid?> GetAssetUid(ProjectIDs projectIds, long? assetId)
    {
      if (assetId.HasValue)
      {
        var machineExecutionResult = await GetMachines(projectIds) as MachineExecutionResult;

        return machineExecutionResult?.MachineStatuses?.FirstOrDefault(a => a.AssetId == assetId.Value)?.AssetUid;
      }

      return null;
    }

    /// <summary>
    /// Matches asset UIDs returned from TRex to legacy asset IDs.
    /// </summary>
    protected async Task<Dictionary<Guid, long>> GetAssetIds(ProjectIDs projectIds, List<Guid> assetUids)
    {
      var machineExecutionResult = await GetMachines(projectIds) as MachineExecutionResult;
      return machineExecutionResult?.MachineStatuses?.Where(a => a.AssetUid.HasValue && assetUids.Contains(a.AssetUid.Value)).ToDictionary(m => m.AssetUid.Value, m => m.AssetId);
    }

    /// <summary>
    /// Gets the list of machines from Raptor/TRex
    /// </summary>
    private Task<ContractExecutionResult> GetMachines(ProjectIDs projectIds)
    {
      return RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(
          loggerFactory,
#if RAPTOR
          raptorClient,
#endif
          configStore: configStore,
          trexCompactionDataProxy: trexCompactionDataProxy,
          deviceProxy: deviceProxy,
          customHeaders: customHeaders,
          customerUid: customerUid)
        .ProcessAsync(projectIds);
    }
  }
}
