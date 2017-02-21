using System.Net;
using Microsoft.Extensions.Logging;
using VLPDDecls;
using VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.Executors
{
    public class GetMachineDesignsExecutor : RequestExecutorContainer
    {
        /// <summary>
        /// This constructor allows us to mock RaptorClient
        /// </summary>
        /// <param name="raptorClient"></param>
        public GetMachineDesignsExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
        {
        }

        /// <summary>
        /// Default constructor for RequestExecutorContainer.Build
        /// </summary>
        public GetMachineDesignsExecutor()
        {
        }
        protected override ContractExecutionResult ProcessEx<T>(T item)
        {
            ContractExecutionResult result = null;
            try
            {
                ProjectID request = item as ProjectID;
                TDesignName[] designs = raptorClient.GetOnMachineDesigns(request.projectId ?? -1);
                if (designs != null)
                    result =
                            MachineDesignsExecutionResult.CreateMachineExecutionResult(designs);
                else
                    throw new ServiceException(HttpStatusCode.BadRequest,
                            new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                                    "Failed to get requested machines designs details"));
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