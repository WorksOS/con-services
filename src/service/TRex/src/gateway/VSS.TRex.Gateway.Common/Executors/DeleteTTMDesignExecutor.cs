using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.TRex.Common;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class DeleteTTMDesignExecutor : BaseExecutor
  {
    /// <summary>
    /// TagFileExecutor
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    /// <param name="exceptionHandler"></param>
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
        log.LogInformation($"#In# DeleteTTMDesignExecutor. Delete design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");

        bool removedOk;
        if (request.FileType == ImportedFileType.DesignSurface)
        {
          removedOk = DIContext.Obtain<IDesignManager>().Remove(request.ProjectUid, request.DesignUid);
        }
        else
        {
          removedOk = DIContext.Obtain<ISurveyedSurfaceManager>().Remove(request.ProjectUid, request.DesignUid);
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
            File.Delete(localPathAndFileName + Designs.TTM.Optimised.Consts.DESIGN_SUB_GRID_INDEX_FILE_EXTENSION);
            File.Delete(localPathAndFileName + Designs.TTM.Optimised.Consts.DESIGN_SPATIAL_INDEX_FILE_EXTENSION);
          }
          catch (Exception)
          {
            // ignored
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


    /// <summary>
    /// Processes the request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}
