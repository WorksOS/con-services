using System;
using VLPDDecls;
using VSS.Common.ResultsHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  public class LayerIdsExecutionResult : ContractExecutionResult
    {

        public LayerIdDetails[] LayerIdDetailsArray { get; private set; }

        public static LayerIdsExecutionResult CreateLayerIdsExecutionResult(TDesignLayer[] layerlist)
        {
            LayerIdDetails[] LayerIdDetails = new LayerIdDetails[layerlist.Length];

            for (int i =0;i<layerlist.Length;i++)
            {
              LayerIdDetails[i] = new LayerIdDetails
              {
                AssetId = layerlist[i].FAssetID,
                DesignId = layerlist[i].FDesignID,
                LayerId = layerlist[i].FLayerID,
                StartDate = layerlist[i].FStartTime,
                EndDate = layerlist[i].FEndTime
              };
            }

            return new LayerIdsExecutionResult {LayerIdDetailsArray = LayerIdDetails};
        }
    }

    public class LayerIdDetails
    {
        public long AssetId { get; set; }
        public long DesignId { get; set; }
        public long LayerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}