using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;
using VSS.TRex.SurveyedSurfaces.GridFabric.Requests;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class UpdateTTMDesignExecutor : BaseExecutor
  {
    /// <summary>
    /// TagFileExecutor
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    /// <param name="exceptionHandler"></param>
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
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<DesignRequest>(item);

      try
      {
        log.LogInformation($"#In# UpdateTTMDesignExecutor. Update design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");

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

        if (removedOk)
        {
          // Broadcast to listeners that design has changed
          var sender = DIContext.Obtain<IDesignChangedEventSender>();
          sender.DesignStateChanged(DesignNotificationGridMutability.NotifyImmutable, request.ProjectUid, request.DesignUid, request.FileType, designRemoved: true);
        }
        else
        {
          throw CreateServiceException<UpdateTTMDesignExecutor>
            (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError, 
            RequestErrorStatus.DesignImportUnableToDeleteDesign);
        }

        // load core file from s3 to local
        var localPath = FilePathHelper.GetTempFolderForProject(request.ProjectUid);
        var localPathAndFileName = Path.Combine(new[] { localPath, request.FileName });
        var ttm = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
        var designLoadResult = await ttm.LoadFromStorage(request.ProjectUid, request.FileName, localPath);
        if (designLoadResult != DesignLoadResult.Success)
        {
          log.LogError($"#Out# UpdateTTMDesignExecutor. Loading of design failed :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, designLoadResult: {designLoadResult.ToString()}");
          throw CreateServiceException<UpdateTTMDesignExecutor>
            (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError,
              RequestErrorStatus.DesignImportUnableToRetrieveFromS3, designLoadResult.ToString());
        }

        // This generates the 2 index files 
        designLoadResult = ttm.LoadFromFile(localPathAndFileName);
        if (designLoadResult != DesignLoadResult.Success)
        {
          log.LogError($"#Out# UpdateTTMDesignExecutor. Addition of design failed :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, designLoadResult: {designLoadResult.ToString()}");
          throw CreateServiceException<UpdateTTMDesignExecutor>
            (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError,
              RequestErrorStatus.DesignImportUnableToUpdateDesign, designLoadResult.ToString());
        }

        var extents = new BoundingWorldExtent3D();
        ttm.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
        ttm.GetHeightRange(out extents.MinZ, out extents.MaxZ);

        if (request.FileType == ImportedFileType.DesignSurface)
        {
          // Create the new designSurface in our site 
          var tRexRequest = new AddTTMDesignRequest();
          var designSurfaceUid = await tRexRequest.ExecuteAsync(new AddTTMDesignArgument
          {
            ProjectID = request.ProjectUid,
            DesignDescriptor = new Designs.Models.DesignDescriptor(request.DesignUid, localPathAndFileName, request.FileName),
            Extents = extents,
            ExistenceMap = ttm.SubGridOverlayIndex()
          });
        }

        if (request.FileType == ImportedFileType.SurveyedSurface)
        {
          // Create the new SurveyedSurface in our site model
          var tRexRequest = new AddSurveyedSurfaceRequest();
          var surveyedSurfaceUid = await tRexRequest.ExecuteAsync(new AddSurveyedSurfaceArgument
          {
            ProjectID = request.ProjectUid,
            DesignDescriptor = new Designs.Models.DesignDescriptor(request.DesignUid, localPathAndFileName, request.FileName),
            AsAtDate = request.SurveyedUtc ?? TRex.Common.Consts.MIN_DATETIME_AS_UTC, // validation will have ensured this exists
            Extents = extents,
            ExistenceMap = ttm.SubGridOverlayIndex()
          });
        }

        //  TTM.LoadFromFile() will have created these 2 files. We need to store them on S3 to reload cache when required
        var s3FileTransfer = new S3FileTransfer(TransferProxyType.DesignImport);
        s3FileTransfer.WriteFile(localPath, request.ProjectUid, request.FileName + Designs.TTM.Optimised.Consts.DESIGN_SUB_GRID_INDEX_FILE_EXTENSION);
        s3FileTransfer.WriteFile(localPath, request.ProjectUid, request.FileName + Designs.TTM.Optimised.Consts.DESIGN_SPATIAL_INDEX_FILE_EXTENSION);
        s3FileTransfer.WriteFile(localPath, request.ProjectUid, request.FileName + Designs.TTM.Optimised.Consts.DESIGN_BOUNDARY_FILE_EXTENSION);

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
