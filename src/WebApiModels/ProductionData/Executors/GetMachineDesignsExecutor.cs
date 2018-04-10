using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Executors
{
  public class GetMachineDesignsExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result;
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