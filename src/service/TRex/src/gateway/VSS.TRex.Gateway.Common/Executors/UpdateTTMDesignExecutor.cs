using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Executor for updating alignments
  /// </summary>
  public class UpdateTTMDesignExecutor : BaseDesignExecutor<UpdateTTMDesignExecutor>
  {
    /// <summary>
    /// 
    /// </summary>
    public UpdateTTMDesignExecutor(IConfigurationStore configStore,
        ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public UpdateTTMDesignExecutor()
    {
    }

    /// <summary>
    /// Process update design request
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<DesignRequest>(item);

      try
      {
        log.LogInformation($"#In# UpdateTTMDesignExecutor. Update design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");

        await RemoveDesign(request, "UpdateTTMDesignExecutor");

        await AddDesign(request, "UpdateTTMDesignExecutor");

        log.LogInformation($"#Out# UpdateTTMDesignExecutor. Processed update design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");
      }
      catch (Exception e)
      {
        log.LogError(e, $"#Out# UpdateTTMDesignExecutor. Update of design failed :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, Exception:");
        throw CreateServiceException<UpdateTTMDesignExecutor>
          (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError,
            RequestErrorStatus.DesignImportUnableToUpdateDesign, e.Message);
      }

      return new ContractExecutionResult();
    }
    
    /// <summary>
    /// Processes the request synchronously.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
