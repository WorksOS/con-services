using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.Common;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
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
        log.LogInformation($"#In# DeleteDesignExecutor. Add design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");

        // todojeannie rather than removing it here, should there be a DesignManager/SS.Update() which effectively does this?
        //    how about removing the indexes from local and s3 storage?
        //    should this remove go into the Designmanager.Update?

        if (request.FileType == ImportedFileType.SurveyedSurface)
        {
          var isDeletedOk = DIContext.Obtain<IDesignManager>().Remove(request.ProjectUid, request.DesignUid);

        }
        else
        {
          var isDeletedOk = DIContext.Obtain<ISurveyedSurfaceManager>().Remove(request.ProjectUid, request.DesignUid);
        }
      }
      catch (Exception e)
      {
        result = new ContractExecutionResult(/* todojeannie */ 9999, "Unable to Delete Design. Exception: {e}");
      }
      finally
      {
        log.LogInformation($"#Out# DeleteDesignExecutor. Process add design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, Result Code: {result.Code}, Message:{result.Message}");
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
