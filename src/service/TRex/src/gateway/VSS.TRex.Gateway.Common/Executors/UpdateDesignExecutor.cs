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
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Gateway.Common.Helpers;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Consts = VSS.TRex.ExistenceMaps.Interfaces.Consts;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class UpdateDesignExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// TagFileExecutor
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    /// <param name="exceptionHandler"></param>
    public UpdateDesignExecutor(IConfigurationStore configStore,
        ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public UpdateDesignExecutor()
    {
    }

    /// <summary>
    /// Process update design request
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
        return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "shouldn't get here"); // to keep compiler happy
      }

      try
      {
        log.LogInformation($"#In# UpdateDesignExecutor. Update design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");

        bool removedOk;
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
          return new ContractExecutionResult((int)RequestErrorStatus.DesignImportUnableToDeleteDesign, RequestErrorStatus.DesignImportUnableToDeleteDesign.ToString());
        }

        // load core file from s3 to local
        var localPath = DesignHelper.EstablishLocalDesignFilepath(request.ProjectUid.ToString());
        var localPathAndFileName = Path.Combine(new[] { localPath, request.FileName });
        TTMDesign ttm = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
        var designLoadResult = ttm.LoadFromStorage(request.ProjectUid, request.FileName, localPath, false);
        if (designLoadResult != DesignLoadResult.Success)
        {
          log.LogError($"#Out# UpdateDesignExecutor. Loading of design failed :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, designLoadResult: {designLoadResult.ToString()}");
          return new ContractExecutionResult((int)RequestErrorStatus.DesignImportUnableToRetrieveFromS3, designLoadResult.ToString());
        }

        // This generates the 2 index files 
        designLoadResult = ttm.LoadFromFile(localPathAndFileName);
        if (designLoadResult != DesignLoadResult.Success)
        {
          log.LogError($"#Out# UpdateDesignExecutor. Addition of design failed :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, designLoadResult: {designLoadResult.ToString()}");
          return new ContractExecutionResult((int)RequestErrorStatus.DesignImportUnableToCreateDesign, designLoadResult.ToString());
        }

        BoundingWorldExtent3D extents = new BoundingWorldExtent3D();
        ttm.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
        ttm.GetHeightRange(out extents.MinZ, out extents.MaxZ);

        var existanceMaps = DIContext.Obtain<IExistenceMaps>();
        if (request.FileType == ImportedFileType.DesignSurface)
        {
          // Create the new designSurface in our site model
          var designSurface = DIContext.Obtain<IDesignManager>().Add(request.ProjectUid,
            new Designs.Models.DesignDescriptor(request.DesignUid, localPathAndFileName, request.FileName, 0),
            extents);
          existanceMaps.SetExistenceMap(request.DesignUid, Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, designSurface.ID, ttm.SubgridOverlayIndex());
        }

        if (request.FileType == ImportedFileType.SurveyedSurface)
        {
          // Create the new SurveyedSurface in our site model
          var surveyedSurface = DIContext.Obtain<ISurveyedSurfaceManager>().Add(request.ProjectUid,
            new Designs.Models.DesignDescriptor(request.DesignUid, localPathAndFileName, request.FileName, 0),
            request.SurveyedUtc ?? DateTime.MinValue, // validation will have ensured this exists
            extents);
          existanceMaps.SetExistenceMap(request.DesignUid, Consts.EXISTENCE_SURVEYED_SURFACE_DESCRIPTOR, surveyedSurface.ID, ttm.SubgridOverlayIndex());
        }

        //  TTM.LoadFromFile() will have created these 2 files. We need to store them on S3 to reload cache when required
        S3FileTransfer.WriteFile(localPath, request.ProjectUid, request.FileName + Designs.TTM.Optimised.Consts.kDesignSubgridIndexFileExt);
        S3FileTransfer.WriteFile(localPath, request.ProjectUid, request.FileName + Designs.TTM.Optimised.Consts.kDesignSpatialIndexFileExt);

        log.LogInformation($"#Out# UpdateDesignExecutor. Processed update design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");
      }
      catch (Exception e)
      {
        log.LogError($"#Out# UpdateDesignExecutor. Update of design failed :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, Exception: {e}");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, (int)RequestErrorStatus.DesignImportUnableToUpdateDesign, e.Message);
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
