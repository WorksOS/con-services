﻿using System;
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
using VSS.TRex.Gateway.Common.Requests;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Types;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Consts = VSS.TRex.ExistenceMaps.Interfaces.Consts;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class AddDesignExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// TagFileExecutor
    /// </summary>
    /// <param name="configStore"></param>
    /// <param name="logger"></param>
    /// <param name="exceptionHandler"></param>
    public AddDesignExecutor(IConfigurationStore configStore,
        ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public AddDesignExecutor()
    {
    }

    /// <summary>
    /// Process add design request
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
        log.LogInformation($"#In# AddDesignExecutor. Add design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");

        // add or update, load core file from s3 to local
        var localPath = Path.Combine(new[] {TRexServerConfig.PersistentCacheStoreLocation, request.ProjectUid.ToString()});
        var localPathAndFileName = Path.Combine(new[] { localPath, request.FileName});
        TTMDesign TTM = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
        var designLoadResult = TTM.LoadFromStorage(request.ProjectUid, request.FileName, localPath, false);
        if (designLoadResult != DesignLoadResult.Success)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, (int) RequestErrorStatus.DesignImportUnableToRetrieveFromS3);
        }
        // This generates the 2 index files below
        TTM.LoadFromFile(localPathAndFileName);

        BoundingWorldExtent3D extents = new BoundingWorldExtent3D();
        TTM.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
        TTM.GetHeightRange(out extents.MinZ, out extents.MaxZ);

        if (request.FileType == ImportedFileType.DesignSurface)
        {
          // Create the new designSurface in our site model
          var designSurface = DIContext.Obtain<IDesignManager>().Add(request.ProjectUid,
            new VSS.TRex.Designs.Models.DesignDescriptor(request.DesignUid, localPathAndFileName, request.FileName, 0),
            extents);
          DIContext.Obtain<IExistenceMaps>().SetExistenceMap(request.DesignUid, Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, designSurface.ID, TTM.SubgridOverlayIndex());
        }

        if (request.FileType == ImportedFileType.SurveyedSurface)
        {
          // Create the new SurveyedSurface in our site model
          var surveyedSurface = DIContext.Obtain<ISurveyedSurfaceManager>().Add(request.ProjectUid,
            new VSS.TRex.Designs.Models.DesignDescriptor(request.DesignUid, localPathAndFileName, request.FileName, 0),
            request.SurveyedUtc ?? DateTime.MinValue, // validation will have ensured this exists
            extents);
          DIContext.Obtain<IExistenceMaps>().SetExistenceMap(request.DesignUid, Consts.EXISTENCE_SURVEYED_SURFACE_DESCRIPTOR, surveyedSurface.ID, TTM.SubgridOverlayIndex());
        }

        //  TTM.LoadFromFile() will have created these 2 files. We need to store them on S3 to reload cache when required
        S3FileTransfer.WriteFile(TRexServerConfig.PersistentCacheStoreLocation, request.ProjectUid, request.FileName + VSS.TRex.Designs.TTM.Optimised.Consts.kDesignSubgridIndexFileExt);
        S3FileTransfer.WriteFile(TRexServerConfig.PersistentCacheStoreLocation, request.ProjectUid, request.FileName + VSS.TRex.Designs.TTM.Optimised.Consts.kDesignSpatialIndexFileExt);

        log.LogInformation($"#Out# AddDesignExecutor. Process add design :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, Result Code: {result.Code}, Message:{result.Message}");
      }
      catch (Exception e)
      {
        log.LogError($"#Out# CreateDesignExecutor. Addition of design failed :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, Exception: {e}");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, (int)RequestErrorStatus.DesignImportUnableToCreateDesign, e.Message);
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
