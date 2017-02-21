using System.Net;
using DesignProfilerDecls;
using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApi.ProductionData.Controllers
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