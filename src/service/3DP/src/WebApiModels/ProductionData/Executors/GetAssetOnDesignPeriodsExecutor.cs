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
  public class GetAssetOnDesignPeriodsExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var projectIds = CastRequestObjectToProjectIDs(item);
      log.LogInformation($"GetAssetOnDesignPeriodsExecutor: {JsonConvert.SerializeObject(projectIds)}, UseTRexGateway: {UseTRexGateway("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")}");

      List<AssetOnDesignPeriod> assetOnDesignPeriods;
      bool haveUids = true;

#if RAPTOR
      if (UseTRexGateway("ENABLE_TREX_GATEWAY_MACHINEDESIGNS"))
#endif
      {
        var machineDesignsResult = await trexCompactionDataProxy
          .SendDataGetRequest<MachineDesignsExecutionResult>(projectIds.ProjectUid.ToString(), $"/sitemodels/{projectIds.ProjectUid.ToString()}/machinedesigns",
            customHeaders);
        // Trex will set OnMachineDesignId = -1, so that OnMachineDesignName should be used
        assetOnDesignPeriods = machineDesignsResult.AssetOnDesignPeriods;
      }

#if RAPTOR
      else
      {
        haveUids = false;
        var raptorDesigns = raptorClient.GetOnMachineDesignEvents(projectIds.ProjectId);

        if (raptorDesigns == null)
          return new MachineDesignsExecutionResult(new List<AssetOnDesignPeriod>());

        assetOnDesignPeriods = ConvertDesignList(raptorDesigns);
      }
#endif

      await PairUpAssetIdentifiers(projectIds, assetOnDesignPeriods);
      return CreateResultantListFromDesigns(assetOnDesignPeriods);
    }

    private async Task PairUpAssetIdentifiers(ProjectIDs projectIds, List<AssetOnDesignPeriod> assetOnDesignPeriods)
    {
      if (assetOnDesignPeriods == null || assetOnDesignPeriods.Count == 0)
        return;

      if (await RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(loggerFactory,
#if RAPTOR
              raptorClient,
#endif
              configStore: configStore, trexCompactionDataProxy: trexCompactionDataProxy, assetResolverProxy: assetResolverProxy,
              customHeaders: customHeaders, customerUid: customerUid)
            .ProcessAsync(projectIds) is MachineExecutionResult machineExecutionResult && machineExecutionResult.MachineStatuses.Count > 0)
      {
        foreach (var assetMatch in machineExecutionResult.MachineStatuses.Where(a => a.AssetUid.HasValue && a.AssetUid.Value != Guid.Empty && a.AssetId > 0))
        {
          foreach (var assetOnDesignPeriod in assetOnDesignPeriods.FindAll(a => a.AssetUid == assetMatch.AssetUid && a.MachineId < 1))
            assetOnDesignPeriod.MachineId = assetMatch.AssetId;

          foreach (var assetOnDesignPeriod in assetOnDesignPeriods.FindAll(a => a.MachineId == assetMatch.AssetId && (!a.AssetUid.HasValue || a.AssetUid.Value == Guid.Empty)))
            assetOnDesignPeriod.AssetUid = assetMatch.AssetUid;
        }
      }
    }

#if RAPTOR
    private List<AssetOnDesignPeriod> ConvertDesignList(TDesignName[] designList)
    {
      var assetOnDesignPeriods = new List<AssetOnDesignPeriod>(designList.Length);

      for (var i = 0; i < designList.Length; i++)
      {
        assetOnDesignPeriods.Add(new AssetOnDesignPeriod
        (
          designList[i].FName,
          designList[i].FID,
          designList[i].FMachineID,
          designList[i].FStartDate,
          designList[i].FEndDate,
          null
        ));
      }

      return assetOnDesignPeriods;
    }
#endif

    private MachineDesignsExecutionResult CreateResultantListFromDesigns(List<AssetOnDesignPeriod> assetOnDesignPeriods)
    {
      //For details, need to set the end dates so can test date range
      var assetOnDesignPeriodsResult = new List<AssetOnDesignPeriod>();
      var assetUids = assetOnDesignPeriods.Select(d => d.AssetUid).Distinct();

      foreach (var assetUid in assetUids)
      {
        var machineDesigns = assetOnDesignPeriods.Where(d => d.AssetUid == assetUid).OrderBy(d => d.StartDate).ToList();
        for (var i = 1; i < machineDesigns.Count; i++)
        {
          assetOnDesignPeriodsResult.Add(new AssetOnDesignPeriod(
            machineDesigns[i - 1].OnMachineDesignName,
            machineDesigns[i - 1].OnMachineDesignId,
            machineDesigns[i - 1].MachineId,
            machineDesigns[i - 1].StartDate,
            machineDesigns[i].StartDate,
            machineDesigns[i - 1].AssetUid
          ));
        }

        assetOnDesignPeriodsResult.Add(new AssetOnDesignPeriod(
          machineDesigns[machineDesigns.Count - 1].OnMachineDesignName,
          machineDesigns[machineDesigns.Count - 1].OnMachineDesignId,
          machineDesigns[machineDesigns.Count - 1].MachineId,
          machineDesigns[machineDesigns.Count - 1].StartDate,
          DateTime.UtcNow,
          machineDesigns[machineDesigns.Count - 1].AssetUid));
      }

      return new MachineDesignsExecutionResult(assetOnDesignPeriodsResult);
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
