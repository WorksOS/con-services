using System.Threading.Tasks;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.ProductionData.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;

namespace VSS.Productivity3D.WebApi.Models.Extensions
{
  public class ExecutorHelper : RequestExecutorContainer
  {
    protected async Task PairUpAssetIdentifiers(ProjectID projectIds, FilterResult filter1, FilterResult filter2 = null)
    {
      // filters coming from TBC can have only the legacy assetId
      if ((filter1 == null || filter1.ContributingMachines == null || filter1.ContributingMachines.Count == 0) &&
          (filter2 == null || filter2.ContributingMachines == null || filter2.ContributingMachines.Count == 0))
        return;

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
