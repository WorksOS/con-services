using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class DeleteSVLDesignExecutor : BaseExecutor
  {
    /// <summary>
    /// TagFileExecutor
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    /// <param name="exceptionHandler"></param>
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
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = item as DesignRequest;
      if (request == null)
      {
        ThrowRequestTypeCastException<DesignRequest>();
        return null; // to keep compiler happy
      }

      try
      {
        log.LogInformation($"#In# DeleteSVLDesignExecutor. Delete design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");

        if (!DIContext.Obtain<IAlignmentManager>().Remove(request.ProjectUid, request.DesignUid))
        {
          log.LogError($"#Out# DeleteSVLDesignExecutor. Deletion failed, of design:{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");
          throw CreateServiceException<DeleteSVLDesignExecutor>
          (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError,
            RequestErrorStatus.DesignImportUnableToDeleteDesign);
        }

        var localPathAndFileName = Path.Combine(new[] { TRexServerConfig.PersistentCacheStoreLocation, request.ProjectUid.ToString(), request.FileName });
        if (File.Exists(localPathAndFileName))
        {
          try
          {
            File.Delete(localPathAndFileName);
          }
          catch (Exception)
          {
            // ignored
          }
        }
        log.LogInformation($"#Out# DeleteSVLDesignExecutor. Process Delete design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");
      }
      catch (Exception e)
      {
        log.LogError(e, $"#Out# DeleteSVLDesignExecutor. Deletion failed, of design:{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, Exception:");
        throw CreateServiceException<DeleteSVLDesignExecutor>
        (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError,
          RequestErrorStatus.DesignImportUnableToDeleteDesign, e.Message);
      }

      return new ContractExecutionResult(); 
    }


    /// <summary>
    /// Processes the request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}
