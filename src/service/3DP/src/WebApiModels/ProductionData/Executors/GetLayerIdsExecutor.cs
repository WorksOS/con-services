using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
#if RAPTOR
using VLPDDecls;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
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

      var layers = new List<LayerIdDetails>();
      bool haveIds = false;

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
          throw CreateServiceException<GetLayerIdsExecutor>();
      }

#if RAPTOR
      else
      {
        if (request.ProjectId.HasValue && request.ProjectId >= 1)
        {
          haveIds = true;
          TDesignLayer[] layerList = null;
          raptorClient.GetOnMachineLayers(request.ProjectId ?? -1, out layerList);
          if (layerList == null || layerList.Length == 0)
            return new LayerIdsExecutionResult(new List<LayerIdDetails>());

          layers = ConvertLayerList(layerList);
        }
        else
          throw CreateServiceException<GetLayerIdsExecutor>();
      }
#endif

      // todoJeannie pair machineUids
      PairUpAssetIdentifiers(layers, haveIds);
      return new LayerIdsExecutionResult(layers);
    }

    private void PairUpAssetIdentifiers(List<LayerIdDetails> layers, bool haveIds)
    {
      if (layers == null || layers.Count == 0)
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
    private List<LayerIdDetails> ConvertLayerList(TDesignLayer[] layerList)
    {
      var layers = new List<LayerIdDetails>(layerList.Length);

      for (var i = 0; i < layerList.Length; i++)
      {
        layers.Add(new LayerIdDetails
        (
          layerList[i].FAssetID, layerList[i].FDesignID, layerList[i].FLayerID, layerList[i].FStartTime, layerList[i].FEndTime, null
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
