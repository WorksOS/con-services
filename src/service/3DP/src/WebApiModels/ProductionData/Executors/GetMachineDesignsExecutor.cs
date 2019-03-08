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

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  public class GetMachineDesignsExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<ProjectID>(item);
      log.LogInformation($"GetMachineDesignsExecutor: {JsonConvert.SerializeObject(request)}, UseTRexGateway: {UseTRexGateway("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")}");

      var designs = new List<DesignName>();
      bool haveIds = false;

#if RAPTOR
      if (UseTRexGateway("ENABLE_TREX_GATEWAY_MACHINEDESIGNS"))
#endif
      {
        if (request.ProjectUid.HasValue && request.ProjectUid != Guid.Empty)
        {
          var siteModelId = request.ProjectUid.ToString();

          var machineDesignsResult = await trexCompactionDataProxy
            .SendDataGetRequest<MachineDesignsExecutionResult>(siteModelId, $"/sitemodels/{siteModelId}/machinedesigns",
              customHeaders);
          designs = machineDesignsResult.Designs;
        }
        else
          throw CreateServiceException<GetMachineDesignsExecutor>();
      }

#if RAPTOR
      else
      {
        if (request.ProjectId.HasValue && request.ProjectId >= 1)
        {
          haveIds = true;
          var raptorDesigns = raptorClient.GetOnMachineDesignEvents(request.ProjectId ?? -1);

          if (raptorDesigns == null)
            return new MachineDesignsExecutionResult(new List<DesignName>());

          designs = ConvertDesignList(raptorDesigns);
        }
        else
          throw CreateServiceException<GetMachineDesignsExecutor>();
      }
#endif

      // todoJeannie pair machineUids do this before the following call, to fill in Uids (which it uses)
      PairUpAssetIdentifiers(designs, haveIds);
      return CreateResultantListFromDesigns(designs);
    }

    private void PairUpAssetIdentifiers(List<DesignName> designs, bool haveIds)
    {
      if (designs == null || designs.Count == 0)
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
    private List<DesignName> ConvertDesignList(TDesignName[] designList)
    {
      var designs = new List<DesignName>(designList.Length);

      for (var i = 0; i < designList.Length; i++)
      {
        designs.Add(new DesignName
        (
          designList[i].FName,
          designList[i].FID,
          designList[i].FMachineID,
          designList[i].FStartDate,
          designList[i].FEndDate,
          null
        ));
      }

      return designs;
    }
#endif

    private MachineDesignsExecutionResult CreateResultantListFromDesigns(List<DesignName> designs)
    {
      //For details, need to set the end dates so can test date range
      var designDetails = new List<DesignName>();
      var machineUids = designs.Select(d => d.MachineUid).Distinct();

      foreach (var machineUid in machineUids)
      {
        var machineDesigns = designs.Where(d => d.MachineUid == machineUid).OrderBy(d => d.StartDate).ToList();
        for (var i = 1; i < machineDesigns.Count; i++)
        {
          designDetails.Add(new DesignName(
            machineDesigns[i - 1].Name,
            machineDesigns[i - 1].Id,
            machineDesigns[i - 1].MachineId,
            machineDesigns[i - 1].StartDate,
            machineDesigns[i].StartDate,
            machineDesigns[i - 1].MachineUid
            ));
        }

        designDetails.Add(new DesignName(
          machineDesigns[machineDesigns.Count - 1].Name,
          machineDesigns[machineDesigns.Count - 1].Id,
          machineDesigns[machineDesigns.Count - 1].MachineId,
          machineDesigns[machineDesigns.Count - 1].StartDate,
          DateTime.UtcNow,
          machineDesigns[machineDesigns.Count - 1].MachineUid));
      }

      return new MachineDesignsExecutionResult(designDetails);
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
