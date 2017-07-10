using System.Net;
using Microsoft.Extensions.Logging;
using VLPDDecls;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Executors
{
    public class GetLayerIdsExecutor : RequestExecutorContainer
    {

        /// <summary>
        /// This constructor allows us to mock RaptorClient
        /// </summary>
        /// <param name="raptorClient"></param>
        public GetLayerIdsExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
        {
        }

        /// <summary>
        /// Default constructor for RequestExecutorContainer.Build
        /// </summary>
        public GetLayerIdsExecutor()
        {
        }


        protected override ContractExecutionResult ProcessEx<T>(T item)
        {
            ContractExecutionResult result = null;
            try
            {
                ProjectID request = item as ProjectID;
                TDesignLayer[] layerlist;
                if (raptorClient.GetOnMachineLayers(request.projectId ?? -1, out layerlist) >= 0 && layerlist != null)
                    result = LayerIdsExecutionResult.CreateLayerIdsExecutionResult(layerlist);
                else
                    throw new ServiceException(HttpStatusCode.BadRequest,
                            new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                                    "Failed to get requested layer ids"));
            }
            finally
            {
                //TODO: clean up
            }
            return result;
        }

        protected override void ProcessErrorCodes()
        {
        }
    }
}