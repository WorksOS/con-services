using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
#if RAPTOR
using VLPDDecls;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using LayerIdDetails = VSS.Productivity3D.Models.Models.LayerIdDetails;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  public class GetLayerIdsExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = CastRequestObjectTo<ProjectID>(item);
      log.LogInformation(
        $"GetLayerIdsExecutor: {JsonConvert.SerializeObject(request)}, UseTRexGateway: {UseTRexGateway("ENABLE_TREX_GATEWAY_LAYERS")}");

      LayerIdsExecutionResult layersResult;

#if RAPTOR
      if (UseTRexGateway("ENABLE_TREX_GATEWAY_LAYERS"))
#endif
      {
        if (request.ProjectUid.HasValue && request.ProjectUid != System.Guid.Empty)
        {
          var siteModelId = request.ProjectUid.ToString();

          layersResult = trexCompactionDataProxy
            .SendDataGetRequest<LayerIdsExecutionResult>(siteModelId, $"/sitemodels/{siteModelId}/layers",
              customHeaders)
            .Result;
          // todoJeannie PairUpAssetIdentifiers(layersResult, false); and designUids
          return layersResult;
        }
      }

#if RAPTOR
      if (request.ProjectId.HasValue && request.ProjectId >= 1)
      {
        if (raptorClient.GetOnMachineLayers(request.ProjectId ?? -1, out var layerlist) >= 0 && layerlist != null)
        {
          layersResult = new LayerIdsExecutionResult(ConvertLayerList(layerlist));
          // todoJeannie PairUpAssetIdentifiers(layersResult, false); and designUids
          return layersResult;
        }
      }
#endif

      throw CreateServiceException<GetLayerIdsExecutor>();
    }

#if RAPTOR
    private LayerIdDetails[]  ConvertLayerList(TDesignLayer[] layerlist)
    {
      var layerIdDetails = new LayerIdDetails[layerlist.Length];

      for (var i = 0; i<layerlist.Length; i++)
      {
        layerIdDetails[i] = new LayerIdDetails
        {
          AssetId = layerlist[i].FAssetID,
          DesignId = layerlist[i].FDesignID,
          LayerId = layerlist[i].FLayerID,
          StartDate = layerlist[i].FStartTime,
          EndDate = layerlist[i].FEndTime,
          AssetUid = null,
          DesignUid = null
        };
      }

      return layerIdDetails;
    }
    #endif
  }
}
