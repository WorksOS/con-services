using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
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

      List<DesignName> designs;
      bool haveUids = true;

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
        {
          log.LogError($"GetMachineDesignsExecutor: No projectUid provided. ");
          throw CreateServiceException<GetMachineDesignsExecutor>();
        }
      }

#if RAPTOR
      else
      {
        if (request.ProjectId.HasValue && request.ProjectId >= 1)
        {
          haveUids = false;
          var raptorDesigns = raptorClient.GetOnMachineDesignEvents(request.ProjectId ?? -1);

          if (raptorDesigns == null)
            return new MachineDesignsExecutionResult(new List<DesignName>());

          designs = ConvertDesignList(raptorDesigns);
        }
        else
        {
          log.LogError($"GetMachineDesignsExecutor: No projectId provided. ");
          throw CreateServiceException<GetMachineDesignsExecutor>();
        }
      }
#endif

      PairUpAssetIdentifiers(designs, haveUids);
      return CreateResultantListFromDesigns(designs);
    }

    private async void PairUpAssetIdentifiers(List<DesignName> designs, bool haveUids)
    {
      if (designs == null || designs.Count == 0)
        return;

      // todo await assetProxy.GetAssetsV1(customerUid, customHeaders);
      var assetsResult = new List<AssetData>(0);
      if (haveUids)
      {
        foreach (var design in designs)
        {
          var legacyAssetId = assetsResult.Where(a => a.AssetUID == design.AssetUid).Select(a => a.LegacyAssetID).DefaultIfEmpty(-1).First();
          design.MachineId = legacyAssetId < 1 ? -1 : legacyAssetId;
        }
      }
      else
        foreach (var design in designs)
        {
          if (design.MachineId < 1)
            design.AssetUid = null;
          else
          {
            design.AssetUid = assetsResult.Where(a => a.LegacyAssetID == design.MachineId).Select(a => a.AssetUID).FirstOrDefault();
            design.AssetUid = design.AssetUid == Guid.Empty ? null : design.AssetUid;
          }
        }
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
      var assetUids = designs.Select(d => d.AssetUid).Distinct();

      foreach (var assetUid in assetUids)
      {
        var machineDesigns = designs.Where(d => d.AssetUid == assetUid).OrderBy(d => d.StartDate).ToList();
        for (var i = 1; i < machineDesigns.Count; i++)
        {
          designDetails.Add(new DesignName(
            machineDesigns[i - 1].Name,
            machineDesigns[i - 1].Id,
            machineDesigns[i - 1].MachineId,
            machineDesigns[i - 1].StartDate,
            machineDesigns[i].StartDate,
            machineDesigns[i - 1].AssetUid
            ));
        }

        designDetails.Add(new DesignName(
          machineDesigns[machineDesigns.Count - 1].Name,
          machineDesigns[machineDesigns.Count - 1].Id,
          machineDesigns[machineDesigns.Count - 1].MachineId,
          machineDesigns[machineDesigns.Count - 1].StartDate,
          DateTime.UtcNow,
          machineDesigns[machineDesigns.Count - 1].AssetUid));
      }

      return new MachineDesignsExecutionResult(designDetails);
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
