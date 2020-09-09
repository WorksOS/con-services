using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.ProductionData.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;

namespace VSS.Productivity3D.WebApi.Models.Extensions
{
  public class TbcExecutorHelper : RequestExecutorContainer
  {
    protected async Task PairUpAssetIdentifiers(Guid projectUid, FilterResult filter1, FilterResult filter2 = null)
    {
      // filters coming from TBC can have only the legacy assetId
      if ((filter1 == null || filter1.ContributingMachines == null || filter1.ContributingMachines.Count == 0 || filter1.ContributingMachines.All(m => m.AssetUid != null)) &&
          (filter2 == null || filter2.ContributingMachines == null || filter2.ContributingMachines.Count == 0 || filter2.ContributingMachines.All(m => m.AssetUid != null)))
        return;

      var projectIds = new ProjectIDs(-1, projectUid);

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

    protected async Task PairUpImportedFileIdentifiers(Guid projectUid, DesignDescriptor designDescriptor = null, FilterResult filter1 = null, FilterResult filter2 = null)
    {
      var designDescriptors = new List<DesignDescriptor>();
      designDescriptors.Add(designDescriptor);
      if (filter1 != null)
      {
        designDescriptors.Add(filter1.AlignmentFile);
        designDescriptors.Add(filter1.DesignFile);
        designDescriptors.Add(filter1.LayerDesignOrAlignmentFile);
      }
      if (filter2 != null)
      {
        designDescriptors.Add(filter2.AlignmentFile);
        designDescriptors.Add(filter2.DesignFile);
        designDescriptors.Add(filter2.LayerDesignOrAlignmentFile);
      }
      await PairUpImportedFileIdentifiers(projectUid, designDescriptors);
    }

    protected async Task PairUpImportedFileIdentifiers(Guid projectUid, List<DesignDescriptor> designDescriptors)
    {
      // DesignDescriptors which come from TBC requests will only have legacy fileId
      designDescriptors = designDescriptors.Where(d => d != null).ToList();
      if (!designDescriptors.Any())
        return;

      var filesList = await fileImportProxy.GetFiles(projectUid.ToString(), userId, customHeaders);
      foreach (var d in designDescriptors)
      {
        var fileUid = filesList.Where(f => f.LegacyFileId == d.Id).Select(f => f.ImportedFileUid).FirstOrDefault()?.ToString();
        d.FileUid = string.IsNullOrEmpty(fileUid) ? (Guid?) null : new Guid(fileUid);
        if (d.FileUid == null)
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, $"Unable to identify 1st File Id {d.Id} for projectUid {projectUid}"));
      }
    }
  }
}
