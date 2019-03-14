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
            .SendDataGetRequest<AssetOnDesignLayerPeriodsExecutionResult>(siteModelId, $"/sitemodels/{siteModelId}/machinelayers",
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

          assetOnDesignLayerPeriods = ConvertLayerList(layerList);
        }
        else
        {
          log.LogError($"GetAssetOnDesignLayerPeriodsExecutor: No projectId provided. ");
          throw CreateServiceException<GetAssetOnDesignLayerPeriodsExecutor>();
        }
      }
#endif

      PairUpAssetIdentifiersAsync(assetOnDesignLayerPeriods, haveUids);
      return new AssetOnDesignLayerPeriodsExecutionResult(assetOnDesignLayerPeriods);
    }

    private void PairUpAssetIdentifiersAsync(List<AssetOnDesignLayerPeriod> assetOnDesignLayerPeriods, bool haveUids)
    {
      if (assetOnDesignLayerPeriods == null || assetOnDesignLayerPeriods.Count == 0)
        return;

      // todo await assetProxy.GetAssetsV1(customerUid, customHeaders);
      var assetsResult = new List<AssetData>(0);
      if (haveUids)
      {
        foreach (var layer in assetOnDesignLayerPeriods)
        {
          var legacyAssetId = assetsResult.Where(a => a.AssetUID == layer.AssetUid).Select(a => a.LegacyAssetID).DefaultIfEmpty(-1).First();
          layer.AssetId = legacyAssetId < 1 ? -1 : legacyAssetId;
        }
      }
      else
        foreach (var layer in assetOnDesignLayerPeriods)
        {
          if (layer.AssetId < 1)
            layer.AssetUid = null;
          else
          {
            layer.AssetUid = assetsResult.Where(a => a.LegacyAssetID == layer.AssetId).Select(a => a.AssetUID).FirstOrDefault();
            layer.AssetUid = layer.AssetUid == Guid.Empty ? null : layer.AssetUid;
          }
        }
    }

#if RAPTOR
    private List<AssetOnDesignLayerPeriod> ConvertLayerList(TDesignLayer[] layerList)
    {
      var assetOnDesignLayerPeriods = new List<AssetOnDesignLayerPeriod>(layerList.Length);

      for (var i = 0; i < layerList.Length; i++)
      {
        assetOnDesignLayerPeriods.Add(new AssetOnDesignLayerPeriod
        (
          layerList[i].FAssetID, layerList[i].FDesignID, layerList[i].FLayerID, layerList[i].FStartTime,
          layerList[i].FEndTime, null
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
