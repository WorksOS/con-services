using DesignProfilerDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  public class DesignNameUpdateCacheExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DesignNameUpdateCacheExecutor()
    {
      ProcessErrorCodes();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = CastRequestObjectTo<DesignNameRequest>(item);
      var result = raptorClient.UpdateCacheWithDesign(request.ProjectId ?? -1, request.DesignFilename, 0, true);

      if (result == TDesignProfilerRequestResult.dppiOK)
        return new ContractExecutionResult();

      throw CreateServiceException<CellPassesExecutor>((int)result);
    }

    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddDesignProfileErrorMessages(ContractExecutionStates);
    }
  }
}
