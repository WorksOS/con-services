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
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  public class GetAssetOnDesignLayerPeriodsExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var projectIds = CastRequestObjectToProjectIDs(item);
      log.LogInformation(
        $"GetAssetOnDesignLayerPeriodsExecutor: {JsonConvert.SerializeObject(projectIds)}, UseTRexGateway: {UseTRexGateway("ENABLE_TREX_GATEWAY_LAYERS")}");

      List<AssetOnDesignLayerPeriod> assetOnDesignLayerPeriods;

#if RAPTOR
      if (UseTRexGateway("ENABLE_TREX_GATEWAY_LAYERS"))
#endif
      {
        var layersResult = await trexCompactionDataProxy
          .SendDataGetRequest<AssetOnDesignLayerPeriodsExecutionResult>(projectIds.ProjectUid.ToString(),
            $"/sitemodels/{projectIds.ProjectUid.ToString()}/machinelayers",
            customHeaders);
        assetOnDesignLayerPeriods = layersResult.AssetOnDesignLayerPeriods;
      }

#if RAPTOR
      else
      {
        raptorClient.GetOnMachineLayers(projectIds.ProjectId, out var layerList);
        if (layerList == null || layerList.Length == 0)
          return new AssetOnDesignLayerPeriodsExecutionResult(new List<AssetOnDesignLayerPeriod>());

        var raptorDesigns = raptorClient.GetOnMachineDesignEvents(projectIds.ProjectId);
        if (raptorDesigns == null)
        {
          log.LogError(
            "GetAssetOnDesignLayerPeriodsExecutor: no raptor machineDesigns found to match with layers");
          return new AssetOnDesignLayerPeriodsExecutionResult(new List<AssetOnDesignLayerPeriod>());
        }

        assetOnDesignLayerPeriods = ConvertLayerList(layerList, raptorDesigns);
      }
#endif

      await PairUpAssetIdentifiers(projectIds, assetOnDesignLayerPeriods);
      return new AssetOnDesignLayerPeriodsExecutionResult(assetOnDesignLayerPeriods);
    }

    private async Task PairUpAssetIdentifiers(ProjectIDs projectIds, List<AssetOnDesignLayerPeriod> assetOnDesignLayerPeriods)
    {
      if (assetOnDesignLayerPeriods == null || assetOnDesignLayerPeriods.Count == 0)
        return;

      if (await RequestExecutorContainerFactory.Build<GetMachineIdsExecutor>(loggerFactory,
#if RAPTOR
              raptorClient,
#endif
              configStore: configStore, trexCompactionDataProxy: trexCompactionDataProxy,
              customHeaders: customHeaders, customerUid: customerUid,
              userId: userId, fileImportProxy: fileImportProxy)
            .ProcessAsync(projectIds) is MachineExecutionResult machineExecutionResult && machineExecutionResult.MachineStatuses.Count > 0)
      {
        foreach (var assetMatch in machineExecutionResult.MachineStatuses)
        {
          foreach (var assetOnDesignPeriod in assetOnDesignLayerPeriods.FindAll(a => a.AssetUid == assetMatch.AssetUid))
            assetOnDesignPeriod.AssetId = assetMatch.AssetId;
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
        var designName = designNames.Where(d => d.FID == layerList[i].FDesignID).Select(d => d.FName).FirstOrDefault();
        assetOnDesignLayerPeriods.Add(new AssetOnDesignLayerPeriod
        (
          layerList[i].FAssetID, layerList[i].FDesignID, layerList[i].FLayerID, layerList[i].FStartTime,
          layerList[i].FEndTime, null, designName
        ));
      }

      return assetOnDesignLayerPeriods;
    }
#endif

  }
}
