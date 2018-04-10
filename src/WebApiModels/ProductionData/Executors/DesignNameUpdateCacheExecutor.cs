using DesignProfilerDecls;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Executors
{
  public class DesignNameUpdateCacheExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      DesignNameRequest request = item as DesignNameRequest;

      var result = raptorClient.UpdateCacheWithDesign(request.projectId ?? -1, request.DesignFilename, 0, true);
      if (result == TDesignProfilerRequestResult.dppiOK)
      {
        return new ContractExecutionResult();
      }

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,"Failed to update DesignCache"));
    }
  }
}