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
using VSS.TRex.Alignments.GridFabric.Arguments;
using VSS.TRex.Alignments.GridFabric.Requests;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.GridFabric.Arguments;
using VSS.TRex.SurveyedSurfaces.GridFabric.Requests;
using VSS.TRex.Types;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Gateway.Common.Executors
{
  /// <summary>
  /// Base executor for adding, updating and deleting designs, surveyed surfaces and alignment files
  /// </summary>
  public class BaseDesignExecutor<TExecutor> : BaseExecutor
  {
    public BaseDesignExecutor(IConfigurationStore configStore,
      ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    public BaseDesignExecutor()
    {
    }

    private string SubGridIndexFileName(string filename) => $"{filename}{Designs.TTM.Optimised.Consts.DESIGN_SUB_GRID_INDEX_FILE_EXTENSION}";
    private string SpatialIndexFileName(string filename) => $"{filename}{Designs.TTM.Optimised.Consts.DESIGN_SPATIAL_INDEX_FILE_EXTENSION}";
    private string BoundaryFileName(string filename) => $"{filename}{Designs.TTM.Optimised.Consts.DESIGN_BOUNDARY_FILE_EXTENSION}";

    /// <summary>
    /// Add a design or surveyed surface or alignment file to TRex
    /// </summary>
    protected async Task AddDesign(DesignRequest request, string executorName)
    {
      // load core file from s3 to local
      var localPath = FilePathHelper.GetTempFolderForProject(request.ProjectUid);
      var localPathAndFileName = Path.Combine(new[] { localPath, request.FileName });

      DesignBase design;
      if (request.FileType == ImportedFileType.Alignment)
        design = new SVLAlignmentDesign();
      else
        design = new TTMDesign(SubGridTreeConsts.DefaultCellSize);
      var designLoadResult = design.LoadFromStorage(request.ProjectUid, request.FileName, localPath);
      if (designLoadResult != DesignLoadResult.Success)
      {
        log.LogError($"#Out# {executorName}. Addition of design failed :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, designLoadResult: {designLoadResult}");
        throw CreateServiceException<TExecutor>
          (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError,
            RequestErrorStatus.DesignImportUnableToRetrieveFromS3, designLoadResult.ToString());
      }

      if (request.FileType != ImportedFileType.Alignment)
      {
        // This generates the 2 index files 
        designLoadResult = design.LoadFromFile(localPathAndFileName);
        if (designLoadResult != DesignLoadResult.Success)
        {
          log.LogError($"#Out# {executorName}. Addition of design failed :{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}, designLoadResult: {designLoadResult}");
          throw CreateServiceException<TExecutor>
          (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError,
            RequestErrorStatus.DesignImportUnableToCreateDesign, designLoadResult.ToString());
        }
      }

      var extents = new BoundingWorldExtent3D();
      design.GetExtents(out extents.MinX, out extents.MinY, out extents.MaxX, out extents.MaxY);
      design.GetHeightRange(out extents.MinZ, out extents.MaxZ);

      if (request.FileType == ImportedFileType.DesignSurface)
      {
        // Create the new designSurface in our site 
        var tRexRequest = new AddTTMDesignRequest();
        var designSurfaceUid = await tRexRequest.ExecuteAsync(new AddTTMDesignArgument
        {
          ProjectID = request.ProjectUid,
          DesignDescriptor = new Designs.Models.DesignDescriptor(request.DesignUid, localPathAndFileName, request.FileName),
          Extents = extents,
          ExistenceMap = design.SubGridOverlayIndex()
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
          ExistenceMap = design.SubGridOverlayIndex()
        });
      }

      if (request.FileType == ImportedFileType.Alignment)
      {
        // Create the new alignment in our site model
        var tRexRequest = new AddAlignmentRequest();
        var alignmentUid = await tRexRequest.ExecuteAsync(new AddAlignmentArgument
        {
          ProjectID = request.ProjectUid,
          DesignDescriptor = new Designs.Models.DesignDescriptor(request.DesignUid, localPathAndFileName, request.FileName),
          Extents = extents
        });
      }

      if (request.FileType != ImportedFileType.Alignment)
      {
        //  TTM.LoadFromFile() will have created these 3 files. We need to store them on S3 to reload cache when required
        var s3FileTransfer = new S3FileTransfer(TransferProxyType.DesignImport);
        s3FileTransfer.WriteFile(localPath, request.ProjectUid, SubGridIndexFileName(request.FileName));
        s3FileTransfer.WriteFile(localPath, request.ProjectUid, SpatialIndexFileName(request.FileName));
        s3FileTransfer.WriteFile(localPath, request.ProjectUid, BoundaryFileName(request.FileName));
      }
    }

    /// <summary>
    /// Delete a design or surveyed surface or alignment file from TRex
    /// </summary>
    protected async Task RemoveDesign(DesignRequest request, string executorName)
    {
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

      if (request.FileType == ImportedFileType.Alignment)
      {
        // Remove the alignment
        var tRexRequest = new RemoveAlignmentRequest();
        var removeResponse = await tRexRequest.ExecuteAsync(new RemoveAlignmentArgument
        {
          ProjectID = request.ProjectUid,
          AlignmentID = request.DesignUid
        });

        removedOk = removeResponse.RequestResult == DesignProfilerRequestResult.OK;
      }

      if (!removedOk)
      {
        log.LogError($"#Out# {executorName}. Deletion failed, of design:{request.FileName}, Project:{request.ProjectUid}, DesignUid:{request.DesignUid}");
        throw CreateServiceException<TExecutor>
        (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError,
          RequestErrorStatus.DesignImportUnableToDeleteDesign);
      }

      //Remove local copies of files
      var localPath = FilePathHelper.GetTempFolderForProject(request.ProjectUid);
      var localPathAndFileName = Path.Combine(new[] { localPath, request.FileName });
      if (File.Exists(localPathAndFileName))
      {
        try
        {
          File.Delete(localPathAndFileName);

          if (request.FileType != ImportedFileType.Alignment)
          {
            //Delete index files
            var indexFileName = SubGridIndexFileName(localPathAndFileName);
            if (File.Exists(indexFileName))
              File.Delete(indexFileName);
            indexFileName = SpatialIndexFileName(localPathAndFileName);
            if (File.Exists(indexFileName))
              File.Delete(indexFileName);
            indexFileName = BoundaryFileName(localPathAndFileName);
            if (File.Exists(indexFileName))
              File.Delete(indexFileName);
          }
        }
        catch (Exception e)
        {
          log.LogError(e, $"Failed to delete files related to design/surveyed surface {request.DesignUid} in project {request.ProjectUid}");
        }
      }

      if (request.FileType != ImportedFileType.Alignment)
      {
        //Remove the index files from s3 (project service removes the actual file from s3 as it put it there originally)
        var s3FileTransfer = new S3FileTransfer(TransferProxyType.DesignImport);
        s3FileTransfer.RemoveFileFromBucket(request.ProjectUid, SubGridIndexFileName(request.FileName));
        s3FileTransfer.RemoveFileFromBucket(request.ProjectUid, SpatialIndexFileName(request.FileName));
        s3FileTransfer.RemoveFileFromBucket(request.ProjectUid, BoundaryFileName(request.FileName));
      }

    }
  }
}
