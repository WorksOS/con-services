using DesignProfilerDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors.CellPass;
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
#if RAPTOR
      var request = CastRequestObjectTo<DesignNameRequest>(item);
      var result = raptorClient.UpdateCacheWithDesign(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, request.DesignFilename, 0, true);

      if (result == TDesignProfilerRequestResult.dppiOK)
        return new ContractExecutionResult();

      throw CreateServiceException<CellPassesExecutor>((int)result);
#else
      throw new NotImplementedException();
#endif
      
    }

    protected sealed override void ProcessErrorCodes()
    {
      RaptorResult.AddDesignProfileErrorMessages(ContractExecutionStates);
    }
  }
}
