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
  ///  Executor for deleting alignments
  /// </summary>
  public class DeleteSVLDesignExecutor : BaseDesignExecutor<DeleteSVLDesignExecutor>
  {
    /// <summary>
    /// TagFileExecutor
    /// </summary>
    public DeleteSVLDesignExecutor(IConfigurationStore configStore,
        ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DeleteSVLDesignExecutor()
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
        log.LogInformation($"#In# DeleteSVLDesignExecutor. Delete design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");

        await RemoveDesign(request, "DeleteSVLDesignExecutor");
   
        log.LogInformation($"#Out# DeleteSVLDesignExecutor. Process Delete design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");

        return new ContractExecutionResult();
      }
      catch (Exception e)
      {
        log.LogError(e, $"#Out# DeleteSVLDesignExecutor. Deletion failed, of design:{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, Exception:");
        throw CreateServiceException<DeleteSVLDesignExecutor>
        (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError,
          RequestErrorStatus.DesignImportUnableToDeleteDesign, e.Message);
      }
    }
  }
}
