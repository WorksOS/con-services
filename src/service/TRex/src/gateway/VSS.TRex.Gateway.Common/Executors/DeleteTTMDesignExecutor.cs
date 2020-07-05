using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.TRex.Common;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.Models;
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;
using VSS.TRex.SurveyedSurfaces.GridFabric.Requests;
using VSS.TRex.Types;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class DeleteTTMDesignExecutor : BaseExecutor
  {
    /// <summary>
    /// TagFileExecutor
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

        bool removedOk = false;
        if (request.FileType == ImportedFileType.DesignSurface)
        {
          // Remove the designSurface
          var tRexRequest = new RemoveTTMDesignRequest();
          var removeResponse = await tRexRequest.ExecuteAsync(new RemoveTTMDesignArgument
          {
            ProjectID = request.ProjectUid,
            DesignID = request.DesignUid
          });

          removedOk = removeResponse.RequestResult == DesignProfilerRequestResult.OK;
        }

        if (request.FileType == ImportedFileType.SurveyedSurface)
        {
          // Remove the new surveyedSurface
          var tRexRequest = new RemoveSurveyedSurfaceRequest();
          var removeResponse = await tRexRequest.ExecuteAsync(new RemoveSurveyedSurfaceArgument
          {
            ProjectID = request.ProjectUid,
            DesignID = request.DesignUid
          });

          removedOk = removeResponse.RequestResult == DesignProfilerRequestResult.OK;
        }

        if (!removedOk)
        {
          log.LogError($"#Out# DeleteTTMDesignExecutor. Deletion failed, of design:{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");
          throw CreateServiceException<DeleteTTMDesignExecutor>
            (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError,
              RequestErrorStatus.DesignImportUnableToDeleteDesign);
        }

        var localPathAndFileName = Path.Combine(new[] { TRexServerConfig.PersistentCacheStoreLocation, request.ProjectUid.ToString(), request.FileName });
        if (File.Exists(localPathAndFileName))
        {
          try
          {
            File.Delete(localPathAndFileName);

            if (File.Exists(localPathAndFileName + Designs.TTM.Optimised.Consts.DESIGN_SUB_GRID_INDEX_FILE_EXTENSION))
              File.Delete(localPathAndFileName + Designs.TTM.Optimised.Consts.DESIGN_SUB_GRID_INDEX_FILE_EXTENSION);
            if (File.Exists(localPathAndFileName + Designs.TTM.Optimised.Consts.DESIGN_SPATIAL_INDEX_FILE_EXTENSION))
              File.Delete(localPathAndFileName + Designs.TTM.Optimised.Consts.DESIGN_SPATIAL_INDEX_FILE_EXTENSION);
            if (File.Exists(localPathAndFileName + Designs.TTM.Optimised.Consts.DESIGN_BOUNDARY_FILE_EXTENSION))
              File.Delete(localPathAndFileName + Designs.TTM.Optimised.Consts.DESIGN_BOUNDARY_FILE_EXTENSION);
          }
          catch (Exception e)
          {
            log.LogError(e, $"Failed to delete files related to design/surveyed surface {request.DesignUid} in project {request.ProjectUid}");
          }
        }
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
