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
  public class GetLayerIdsExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<ProjectID>(item);
      log.LogInformation(
        $"GetLayerIdsExecutor: {JsonConvert.SerializeObject(request)}, UseTRexGateway: {UseTRexGateway("ENABLE_TREX_GATEWAY_LAYERS")}");

      List<LayerIdDetails> layers;
      bool haveUids = true;

#if RAPTOR
      if (UseTRexGateway("ENABLE_TREX_GATEWAY_LAYERS"))
#endif
      {
        if (request.ProjectUid.HasValue && request.ProjectUid != System.Guid.Empty)
        {
          var siteModelId = request.ProjectUid.ToString();

          var layersResult = await trexCompactionDataProxy
            .SendDataGetRequest<LayerIdsExecutionResult>(siteModelId, $"/sitemodels/{siteModelId}/machinelayers",
              customHeaders);
          layers = layersResult.Layers;
        }
        else
        {
          log.LogError($"GetLayerIdsExecutor: No projectUid provided. ");
          throw CreateServiceException<GetLayerIdsExecutor>();
        }
      }

#if RAPTOR
      else
      {
        if (request.ProjectId.HasValue && request.ProjectId >= 1)
        {
          haveUids = false;
          TDesignLayer[] layerList = null;
          raptorClient.GetOnMachineLayers(request.ProjectId ?? -1, out layerList);
          if (layerList == null || layerList.Length == 0)
            return new LayerIdsExecutionResult(new List<LayerIdDetails>());

          layers = ConvertLayerList(layerList);
        }
        else
        {
          log.LogError($"GetLayerIdsExecutor: No projectId provided. ");
          throw CreateServiceException<GetLayerIdsExecutor>();
        }
      }
#endif

      PairUpAssetIdentifiersAsync(layers, haveUids);
      return new LayerIdsExecutionResult(layers);
    }

    private async void PairUpAssetIdentifiersAsync(List<LayerIdDetails> layers, bool haveUids)
    {
      if (layers == null || layers.Count == 0)
        return;

      var assetsResult = await assetProxy.GetAssetsV1(customerUid, customHeaders);
      if (haveUids)
      {
        foreach (var layer in layers)
        {
          var legacyAssetId = assetsResult.Where(a => a.AssetUID == layer.AssetUid).Select(a => a.LegacyAssetID)
            .FirstOrDefault();
          layer.AssetId = legacyAssetId < 1 ? -1 : legacyAssetId;
        }
      }
      else
        foreach (var layer in layers)
        {
          if (layer.AssetId < 1)
            layer.AssetUid = null;
          else
          {
            layer.AssetUid = assetsResult.Where(a => a.LegacyAssetID == layer.AssetId).Select(a => a.AssetUID)
              .FirstOrDefault();
          }
        }
    }

#if RAPTOR
    private List<LayerIdDetails> ConvertLayerList(TDesignLayer[] layerList)
    {
      var layers = new List<LayerIdDetails>(layerList.Length);

      for (var i = 0; i < layerList.Length; i++)
      {
        layers.Add(new LayerIdDetails
        (
          layerList[i].FAssetID, layerList[i].FDesignID, layerList[i].FLayerID, layerList[i].FStartTime,
          layerList[i].FEndTime, null
        ));
      }

      return layers;
    }
#endif

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
