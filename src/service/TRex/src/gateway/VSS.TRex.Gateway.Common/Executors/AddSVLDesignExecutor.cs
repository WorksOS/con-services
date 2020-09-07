using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  ///  Executor for adding alignments
  /// </summary>
  public class AddSVLDesignExecutor : BaseDesignExecutor<AddSVLDesignExecutor>
  {
    /// <summary>
    /// AddSVLDesignExecutor
    /// </summary>
    public AddSVLDesignExecutor(IConfigurationStore configStore,
      ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public AddSVLDesignExecutor()
    {
    }

    /// <summary>
    /// Process add design request
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<DesignRequest>(item);

      try
      {
        log.LogInformation($"#In# AddSVLDesignExecutor. Add design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");

        await AddDesign(request, "AddSVLDesignExecutor");
       
        log.LogInformation($"#Out# AddSVLDesignExecutor. Processed add design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");
      }
      catch (Exception e)
      {
        log.LogError(e, $"#Out# AddSVLDesignExecutor. Addition of design failed :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, Exception:");
        throw CreateServiceException<AddSVLDesignExecutor>
          (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError,
            RequestErrorStatus.DesignImportUnableToCreateDesign, e.Message);
      }

      return new ContractExecutionResult();
    }

    /// <summary>
    /// Processes the request synchronously.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new TRexException("Use the asynchronous form of this method");
    }
  }
}
