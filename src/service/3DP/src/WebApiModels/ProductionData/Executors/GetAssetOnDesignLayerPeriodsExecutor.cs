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
  public class GetAssetOnDesignLayerPeriodsExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<ProjectID>(item);
      log.LogInformation(
        $"GetAssetOnDesignLayerPeriodsExecutor: {JsonConvert.SerializeObject(request)}, UseTRexGateway: {UseTRexGateway("ENABLE_TREX_GATEWAY_LAYERS")}");

      List<AssetOnDesignLayerPeriod> assetOnDesignLayerPeriods;
      bool haveUids = true;

#if RAPTOR
      if (UseTRexGateway("ENABLE_TREX_GATEWAY_LAYERS"))
#endif
      {
        if (request.ProjectUid.HasValue && request.ProjectUid != System.Guid.Empty)
        {
          var siteModelId = request.ProjectUid.ToString();

          var layersResult = await trexCompactionDataProxy
            .SendDataGetRequest<AssetOnDesignLayerPeriodsExecutionResult>(siteModelId,
              $"/sitemodels/{siteModelId}/machinelayers",
              customHeaders);
          assetOnDesignLayerPeriods = layersResult.AssetOnDesignLayerPeriods;
        }
        else
        {
          log.LogError($"GetAssetOnDesignLayerPeriodsExecutor: No projectUid provided. ");
          throw CreateServiceException<GetAssetOnDesignLayerPeriodsExecutor>();
        }
      }

#if RAPTOR
      else
      {
        if (request.ProjectId.HasValue && request.ProjectId >= 1)
        {
          haveUids = false;
          raptorClient.GetOnMachineLayers(request.ProjectId ?? -1, out var layerList);
          if (layerList == null || layerList.Length == 0)
            return new AssetOnDesignLayerPeriodsExecutionResult(new List<AssetOnDesignLayerPeriod>());

          var raptorDesigns = raptorClient.GetOnMachineDesignEvents(request.ProjectId ?? -1);

          if (raptorDesigns == null)
          {
            log.LogError(
            $"GetAssetOnDesignLayerPeriodsExecutor: no raptor machineDesigns found to match with layers");
            return new AssetOnDesignLayerPeriodsExecutionResult(new List<AssetOnDesignLayerPeriod>());
          }

          assetOnDesignLayerPeriods = ConvertLayerList(layerList, raptorDesigns);
        }
        else
        {
          log.LogError($"GetAssetOnDesignLayerPeriodsExecutor: No projectId provided. ");
          throw CreateServiceException<GetAssetOnDesignLayerPeriodsExecutor>();
        }
      }
#endif

      await PairUpAssetIdentifiersAsync(assetOnDesignLayerPeriods, haveUids);
      return new AssetOnDesignLayerPeriodsExecutionResult(assetOnDesignLayerPeriods);
    }

    private async Task PairUpAssetIdentifiersAsync(List<AssetOnDesignLayerPeriod> assetOnDesignLayerPeriods,
      bool haveUids)
    {
      if (assetOnDesignLayerPeriods == null || assetOnDesignLayerPeriods.Count == 0)
        return;

      if (haveUids)
      {
        // assetMatch will return rows if Uids found, however the legacyAssetIds may be invalid
        var assetUids = new List<Guid>(assetOnDesignLayerPeriods
          .Where(a => a.AssetUid.HasValue && a.AssetUid.Value != Guid.Empty).Select(a => a.AssetUid.Value).Distinct());
        if (assetUids.Count > 0)
        {
          var assetMatchingResult = (await assetResolverProxy.GetMatchingAssets(assetUids, customHeaders)).ToList();
          foreach (var assetMatch in assetMatchingResult)
          {
            if (assetMatch.Value > 0)
              foreach (var assetOnDesignPeriod in assetOnDesignLayerPeriods.FindAll(x => x.AssetUid == assetMatch.Key))
                assetOnDesignPeriod.AssetId = assetMatch.Value;
          }
        }
      }
      else
      {
        // assetMatch will only return rows if Uids found for the legacyAssetIds
        var assetIds =
          new List<long>(assetOnDesignLayerPeriods.Where(a => a.AssetId > 0).Select(a => a.AssetId).Distinct());
        if (assetIds.Count > 0)
        {
          var assetMatchingResult = (await assetResolverProxy.GetMatchingAssets(assetIds, customHeaders)).ToList();
          foreach (var assetMatch in assetMatchingResult)
          {
            if (assetMatch.Value > 0) // machineId of 0/-1 may occur for >1 AssetUid
              foreach (var assetOnDesignPeriod in assetOnDesignLayerPeriods.FindAll(x => x.AssetId == assetMatch.Value))
                assetOnDesignPeriod.AssetUid = assetMatch.Key;
          }
        }
      }
    }

#if RAPTOR
    private List<AssetOnDesignLayerPeriod> ConvertLayerList(TDesignLayer[] layerList, TDesignName[] raptorDesigns)
    {
      var assetOnDesignLayerPeriods = new List<AssetOnDesignLayerPeriod>(layerList.Length);
      var designNames = raptorDesigns.ToList();

      for (var i = 0; i < layerList.Length; i++)
      {
        var designName = designNames.Where(d => d.FID == layerList[i].FDesignID).Select( d => d.FName).FirstOrDefault();
        assetOnDesignLayerPeriods.Add(new AssetOnDesignLayerPeriod
        (
          layerList[i].FAssetID, layerList[i].FDesignID, layerList[i].FLayerID, layerList[i].FStartTime,
          layerList[i].FEndTime, null, designName
        ));
      }

      return assetOnDesignLayerPeriods;
    }
#endif

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
