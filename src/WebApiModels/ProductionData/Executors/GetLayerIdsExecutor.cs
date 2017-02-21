using System.Net;
using Microsoft.Extensions.Logging;
using VLPDDecls;
using VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApi.ProductionData.Controllers
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