using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.TRex.Alignments.GridFabric.Arguments;
using VSS.TRex.Alignments.GridFabric.Requests;
using VSS.TRex.Common;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class DeleteSVLDesignExecutor : BaseExecutor
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

        // Remove the alignment
        var tRexRequest = new RemoveAlignmentRequest();
        var removeResponse = await tRexRequest.ExecuteAsync(new RemoveAlignmentArgument
        {
          ProjectID = request.ProjectUid,
          AlignmentID = request.DesignUid
        });

        if (removeResponse.RequestResult != Designs.Models.DesignProfilerRequestResult.OK)
        {
          log.LogError($"#Out# DeleteSVLDesignExecutor. Deletion failed, of alignment:{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");
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
          catch (Exception e)
          {
            log.LogError(e, $"Failed to delete file {localPathAndFileName} for design {request.DesignUid} in project {request.ProjectUid}");
          }
        }
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
