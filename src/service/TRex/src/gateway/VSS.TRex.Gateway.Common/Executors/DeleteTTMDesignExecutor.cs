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
  /// Executor for deleting designs and surveyed surfaces
  /// </summary>
  public class DeleteTTMDesignExecutor : BaseDesignExecutor<DeleteTTMDesignExecutor>
  {
    /// <summary>
    /// 
    /// </summary>
    public DeleteTTMDesignExecutor(IConfigurationStore configStore,
        ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DeleteTTMDesignExecutor()
    {
    }

    /// <summary>
    /// Process delete design request
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Processes the request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<DesignRequest>(item);

      try
      {
        log.LogInformation($"#In# DeleteTTMDesignExecutor. Delete design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");

        await RemoveDesign(request, "DeleteTTMDesignExecutor");

        log.LogInformation($"#Out# DeleteTTMDesignExecutor. Process Delete design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");
      }
      catch (Exception e)
      {
        log.LogError(e, $"#Out# DeleteTTMDesignExecutor. Deletion failed, of design:{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, Exception:");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, (int)RequestErrorStatus.DesignImportUnableToDeleteDesign, e.Message);
        throw CreateServiceException<DeleteTTMDesignExecutor>
          (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError,
            RequestErrorStatus.DesignImportUnableToDeleteDesign, e.Message);
      }

      return new ContractExecutionResult();
    }
  }
}
