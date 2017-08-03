using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Executors
{
  public class GetMachineDesignsExecutor : RequestExecutorContainer
    {
        /// <summary>
        /// Default constructor for RequestExecutorContainer.Build
        /// </summary>
        public GetMachineDesignsExecutor()
        {
        }
        protected override ContractExecutionResult ProcessEx<T>(T item)
        {
            ContractExecutionResult result = null;
          ProjectID request = item as ProjectID;
          TDesignName[] designs = raptorClient.GetOnMachineDesigns(request.projectId ?? -1);
          if (designs != null)
            result =
              MachineDesignsExecutionResult.CreateMachineExecutionResult(designs);
          else
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                "Failed to get requested machines designs details"));
          return result;
        }
    }
}