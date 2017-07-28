using System.Net;
using DesignProfilerDecls;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Executors
{
  public class DesignNameUpdateCacheExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    public DesignNameUpdateCacheExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DesignNameUpdateCacheExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {

      DesignNameRequest request = item as DesignNameRequest;

      var result = raptorClient.UpdateCacheWithDesign(request.projectId ?? -1, request.DesignFilename, 0, true);

      if (result == TDesignProfilerRequestResult.dppiOK) return new ContractExecutionResult();

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,"Failed to update DesignCache"));

    }

    protected override void ProcessErrorCodes()
    {
      //TODO Add DesignProfiler error handling here
    }
  }
}