using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.ProductionData.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;

namespace VSS.Productivity3D.WebApi.Models.Extensions
{
  public class TbcExecutorHelper : RequestExecutorContainer
  {
    protected async Task PairUpAssetIdentifiers(long? projectId, Guid? projectUid, FilterResult filter1, FilterResult filter2 = null)
    {
      // filters coming from TBC can have only the legacy assetId
      if ((filter1 == null || filter1.ContributingMachines == null || filter1.ContributingMachines.Count == 0 || filter1.ContributingMachines.All(m => m.AssetUid != null)) &&
          (filter2 == null || filter2.ContributingMachines == null || filter2.ContributingMachines.Count == 0 || filter2.ContributingMachines.All(m => m.AssetUid != null)))
        return;

      // projectUid is setup in controller, but 
      if (projectUid == null)
        throw new ServiceException(
          HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError, "Missing project Uid"));
      var projectIds = new ProjectIDs(projectId ?? -1, projectUid.Value);

      if (await RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(loggerFactory,
          configStore: configStore, trexCompactionDataProxy: trexCompactionDataProxy,
          customHeaders: customHeaders, customerUid: customerUid)
        .ProcessAsync(projectIds) is MachineExecutionResult machineExecutionResult && machineExecutionResult.MachineStatuses.Count > 0)
      {
        foreach (var assetMatch in machineExecutionResult.MachineStatuses)
        {
          if ((filter1 != null && filter1.ContributingMachines != null && filter1.ContributingMachines.Count > 0))
            foreach (var machine in filter1.ContributingMachines.FindAll(a => a.AssetId == assetMatch.AssetId && a.AssetUid == null))
              machine.AssetUid = assetMatch.AssetUid;
          if ((filter2 != null && filter2.ContributingMachines != null && filter2.ContributingMachines.Count > 0))
            foreach (var machine in filter2.ContributingMachines.FindAll(a => a.AssetId == assetMatch.AssetId && a.AssetUid == null))
              machine.AssetUid = assetMatch.AssetUid;
        }
      }
    }
  }
}
