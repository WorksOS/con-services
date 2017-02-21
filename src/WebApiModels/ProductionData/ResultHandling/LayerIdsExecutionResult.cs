using System;
using VLPDDecls;
using VSS.Raptor.Service.Common.Contracts;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling
{
    public class LayerIdsExecutionResult : ContractExecutionResult
    {

        public LayerIdDetails[] LayerIdDetailsArray { get; private set; }

        public static LayerIdsExecutionResult CreateLayerIdsExecutionResult(TDesignLayer[] layerlist)
        {
            LayerIdDetails[] LayerIdDetails = new LayerIdDetails[layerlist.Length];

            for (int i =0;i<layerlist.Length;i++)
            {
                LayerIdDetails[i] = new LayerIdDetails();
                LayerIdDetails[i].AssetId = layerlist[i].FAssetID;
                LayerIdDetails[i].DesignId = layerlist[i].FDesignID;
                LayerIdDetails[i].LayerId = layerlist[i].FLayerID;
                LayerIdDetails[i].StartDate = layerlist[i].FStartTime;
                LayerIdDetails[i].EndDate = layerlist[i].FEndTime;
            }
            return new LayerIdsExecutionResult() {LayerIdDetailsArray = LayerIdDetails};

        }

        /// <summary>
        /// Create example instance of MachineExecutionResult to display in Help documentation.
        /// </summary>
        public static LayerIdsExecutionResult HelpSample
        {
            get
            {
                return new LayerIdsExecutionResult()
                {
                    LayerIdDetailsArray = new[] { LayerIdDetails.HelpSample, LayerIdDetails.HelpSample }
                };
            }
        }
    }

    public class LayerIdDetails
    {
        public long AssetId { get; set; }
        public long DesignId { get; set; }
        public long LayerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public static LayerIdDetails HelpSample {
            get
            {
                return new LayerIdDetails()
                       {
                         AssetId = 1137642418461469,
                         DesignId = 1005,
                         StartDate = DateTime.UtcNow,
                         EndDate = DateTime.UtcNow.AddDays(1),
                         LayerId = 42
                       };
            } 
        }
    }


}