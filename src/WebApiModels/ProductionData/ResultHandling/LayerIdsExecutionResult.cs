using VLPDDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  public class LayerIdsExecutionResult : ContractExecutionResult
  {
    public LayerIdDetails[] LayerIdDetailsArray { get; private set; }

    public static LayerIdsExecutionResult CreateLayerIdsExecutionResult(TDesignLayer[] layerlist)
    {
      var layerIdDetails = new LayerIdDetails[layerlist.Length];

      for (var i = 0; i < layerlist.Length; i++)
      {
        layerIdDetails[i] = new LayerIdDetails
        {
          AssetId = layerlist[i].FAssetID,
          DesignId = layerlist[i].FDesignID,
          LayerId = layerlist[i].FLayerID,
          StartDate = layerlist[i].FStartTime,
          EndDate = layerlist[i].FEndTime
        };
      }

      return new LayerIdsExecutionResult { LayerIdDetailsArray = layerIdDetails };
    }
  }
}
