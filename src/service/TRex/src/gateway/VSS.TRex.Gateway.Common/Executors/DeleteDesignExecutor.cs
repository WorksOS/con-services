using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Common;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Consts = VSS.TRex.ExistenceMaps.Interfaces.Consts;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class DeleteDesignExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// TagFileExecutor
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    /// <param name="exceptionHandler"></param>
    public DeleteDesignExecutor(IConfigurationStore configStore,
        ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DeleteDesignExecutor()
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
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 38);
      }

      ContractExecutionResult result = new ContractExecutionResult();

      try
      {
        log.LogInformation($"#In# DeleteDesignExecutor. Delete design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");

        var removedOk = false;
        if (request.FileType == ImportedFileType.SurveyedSurface)
        {
          removedOk = DIContext.Obtain<IDesignManager>().Remove(request.ProjectUid, request.DesignUid);
        }
        else
        {
          removedOk = DIContext.Obtain<ISurveyedSurfaceManager>().Remove(request.ProjectUid, request.DesignUid);
        }
        if (!removedOk)
        {
          log.LogError($"#Out# DeleteDesignExecutor. deleting of design failed :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");
          return new ContractExecutionResult((int)RequestErrorStatus.DesignImportUnableToDeleteDesign, RequestErrorStatus.DesignImportUnableToDeleteDesign.ToString());
        }

        var localPathAndFileName = Path.Combine(new[] { TRexServerConfig.PersistentCacheStoreLocation, request.ProjectUid.ToString(), request.FileName });
        if (File.Exists(localPathAndFileName))
        {
          try
          {
            File.Delete(localPathAndFileName);
            File.Delete(localPathAndFileName + Designs.TTM.Optimised.Consts.kDesignSubgridIndexFileExt);
            File.Delete(localPathAndFileName + Designs.TTM.Optimised.Consts.kDesignSpatialIndexFileExt);
          }
          catch (Exception)
          {
            // ignored
          }
        }
        log.LogInformation($"#Out# DeleteDesignExecutor. Process Delete design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, Result Code: {result.Code}, Message:{result.Message}");
      }
      catch (Exception e)
      {
        log.LogError($"#Out# DeleteDesignExecutor. Deletion failed design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, Exception: {e}");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, (int)RequestErrorStatus.DesignImportUnableToDeleteDesign, e.Message);
      }

      return result;
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
