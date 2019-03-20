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
      var request = CastRequestObjectTo<ProjectID>(item);
      log.LogInformation($"GetAssetOnDesignPeriodsExecutor: {JsonConvert.SerializeObject(request)}, UseTRexGateway: {UseTRexGateway("ENABLE_TREX_GATEWAY_MACHINEDESIGNS")}");

      List<AssetOnDesignPeriod> assetOnDesignPeriods;
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
          assetOnDesignPeriods = machineDesignsResult.AssetOnDesignPeriods;
        }
        else
        {
          log.LogError($"GetAssetOnDesignPeriodsExecutor: No projectUid provided. ");
          throw CreateServiceException<GetAssetOnDesignPeriodsExecutor>();
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
            return new MachineDesignsExecutionResult(new List<AssetOnDesignPeriod>());

          assetOnDesignPeriods = ConvertDesignList(raptorDesigns);
        }
        else
        {
          log.LogError($"GetAssetOnDesignPeriodsExecutor: No projectId provided. ");
          throw CreateServiceException<GetAssetOnDesignPeriodsExecutor>();
        }
      }
#endif

      await PairUpAssetIdentifiers(assetOnDesignPeriods, haveUids);
      return CreateResultantListFromDesigns(assetOnDesignPeriods);
    }

    private async Task PairUpAssetIdentifiers(List<AssetOnDesignPeriod> assetOnDesignPeriods, bool haveUids)
    {
      if (assetOnDesignPeriods == null || assetOnDesignPeriods.Count == 0)
        return;

      if (haveUids)
      {
        // assetMatch will return rows if Uids found, however the legacyAssetIds may be invalid
        var assetUids = new List<Guid>(assetOnDesignPeriods.Where(a => a.AssetUid.HasValue && a.AssetUid.Value != Guid.Empty).Select(a => a.AssetUid.Value).Distinct());
        var assetMatchingResult = (await assetResolverProxy.GetMatchingAssets(assetUids, customHeaders)).ToList();
        foreach (var assetMatch in assetMatchingResult)
        {
          if (assetMatch.Value > 0)
            foreach (var assetOnDesignPeriod in assetOnDesignPeriods.FindAll(x => x.AssetUid == assetMatch.Key))
              assetOnDesignPeriod.MachineId = assetMatch.Value;
        }
      }
      else
      {
        // assetMatch will only return rows if Uids found for the legacyAssetIds
        var assetIds = new List<long>(assetOnDesignPeriods.Where(a => a.MachineId > 0).Select(a => a.MachineId).Distinct());
        var assetMatchingResult = (await assetResolverProxy.GetMatchingAssets(assetIds, customHeaders)).ToList();
        foreach (var assetMatch in assetMatchingResult)
        {
          if (assetMatch.Value > 0) // machineId of 0/-1 may occur for >1 AssetUid
            foreach (var assetOnDesignPeriod in assetOnDesignPeriods.FindAll(x => x.MachineId == assetMatch.Value))
              assetOnDesignPeriod.AssetUid = assetMatch.Key;
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
            machineDesigns[i - 1].Name,
            machineDesigns[i - 1].Id,
            machineDesigns[i - 1].MachineId,
            machineDesigns[i - 1].StartDate,
            machineDesigns[i].StartDate,
            machineDesigns[i - 1].AssetUid
            ));
        }

        assetOnDesignPeriodsResult.Add(new AssetOnDesignPeriod(
          machineDesigns[machineDesigns.Count - 1].Name,
          machineDesigns[machineDesigns.Count - 1].Id,
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
