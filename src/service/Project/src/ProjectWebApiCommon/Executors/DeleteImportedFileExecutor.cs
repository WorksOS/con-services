using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// Delete an imported file
  ///    Validation includes checking if the file is referenced by a filter
  ///
  /// For Raptor, the file is deleted on TCC and notified to Raptor via a 3dp notification (1 for add/update)
  ///        Min/Max zoom is returned from 3dp
  /// For TRex, the file is stored on S3 and notified to Trex via a 3dp notification (1 for add and another for update)
  ///        Min/max zoom will not be determined this way for TRex-only (todo Elspeth?)
  ///        It continues to write a FileDescription to the DB,
  ///              even though the tcc-specific filespaceID and path are not required for TRex.
  ///              I decided to leave this for now as s3 is probably not the final storage medium,
  ///              that will probably be DataOcean, and requirements are not known yet.
  ///
  /// </summary>
  /// <returns>Details of the upload file</returns>
  public class DeleteImportedFileExecutor : RequestExecutorContainer
  {

    /// <summary>
    /// Adds file via Raptor and/or Trex
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var deleteImportedFile = CastRequestObjectTo<DeleteImportedFile>(item, errorCode: 68);

      await CheckIfUsedInFilter(deleteImportedFile).ConfigureAwait(false);

      await CheckIfHasReferenceSurfacesAsync(deleteImportedFile);

      bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT"), out var useTrexGatewayDesignImport);
      bool.TryParse(configStore.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT"), out var useRaptorGatewayDesignImport);

      DeleteImportedFileEvent deleteImportedFileEvent = null;
      if (useTrexGatewayDesignImport && deleteImportedFile.IsDesignFileType)
      {
        await ImportedFileRequestHelper.NotifyTRexDeleteFile(deleteImportedFile.ProjectUid,
          deleteImportedFile.ImportedFileType, deleteImportedFile.FileDescriptor.FileName,
          deleteImportedFile.ImportedFileUid,
          deleteImportedFile.SurveyedUtc,
          log, customHeaders, serviceExceptionHandler,
          tRexImportFileProxy).ConfigureAwait(false);

        // DB change must be made before productivity3dV2ProxyNotification.DeleteFile is called as it calls back here to get list of Active files
        deleteImportedFileEvent = await ImportedFileRequestDatabaseHelper.DeleteImportedFileInDb
          (deleteImportedFile.ProjectUid, deleteImportedFile.ImportedFileUid, serviceExceptionHandler, projectRepo)
          .ConfigureAwait(false);
      }

      if (useRaptorGatewayDesignImport)
      {
        // DB change must be made before productivity3dV2ProxyNotification.DeleteFile is called as it calls back here to get list of Active files
        if (deleteImportedFileEvent == null)
        {
          deleteImportedFileEvent = await ImportedFileRequestDatabaseHelper.DeleteImportedFileInDb
            (deleteImportedFile.ProjectUid, deleteImportedFile.ImportedFileUid, serviceExceptionHandler, projectRepo)
            .ConfigureAwait(false);
        }

        var importedFileInternalResult = await ImportedFileRequestHelper.NotifyRaptorDeleteFile
          (deleteImportedFile.ProjectUid, deleteImportedFile.ImportedFileType,
            deleteImportedFile.ImportedFileUid, deleteImportedFile.FileDescriptor,
            deleteImportedFile.ImportedFileId, deleteImportedFile.LegacyImportedFileId,
            log, customHeaders, productivity3dV2ProxyNotification)
          .ConfigureAwait(false);

        if (importedFileInternalResult == null)
        {
          if (deleteImportedFile.ImportedFileType != ImportedFileType.ReferenceSurface)
          {
            await TccHelper.DeleteFileFromTCCRepository
              (deleteImportedFile.FileDescriptor, deleteImportedFile.ProjectUid.ToString(), deleteImportedFile.ImportedFileUid.ToString(),
                log, serviceExceptionHandler, fileRepo)
              .ConfigureAwait(false);

            var dataOceanFileName = DataOceanFileUtil.DataOceanFileName(deleteImportedFile.FileDescriptor.FileName,
              deleteImportedFile.ImportedFileType == ImportedFileType.SurveyedSurface || deleteImportedFile.ImportedFileType == ImportedFileType.GeoTiff,
              deleteImportedFile.ImportedFileUid, deleteImportedFile.SurveyedUtc);

            importedFileInternalResult = await DataOceanHelper.DeleteFileFromDataOcean(
              dataOceanFileName, deleteImportedFile.DataOceanRootFolder, customerUid,
              deleteImportedFile.ProjectUid, deleteImportedFile.ImportedFileUid, log, dataOceanClient, authn, configStore);

            if (deleteImportedFile.ImportedFileType == ImportedFileType.Alignment ||
                deleteImportedFile.ImportedFileType == ImportedFileType.Linework ||
                deleteImportedFile.ImportedFileType == ImportedFileType.GeoTiff)
            {
              var tasks = new List<Task>();
              //delete generated DXF tiles
              string dxfFileName = DataOceanFileUtil.GeneratedFileName(dataOceanFileName, deleteImportedFile.ImportedFileType);
              var dataOceanPath = DataOceanFileUtil.DataOceanPath(deleteImportedFile.DataOceanRootFolder, customerUid, deleteImportedFile.ProjectUid.ToString());
              var fullFileName = $"{dataOceanPath}{Path.DirectorySeparatorChar}{dxfFileName}";
              tasks.Add(pegasusClient.DeleteTiles(fullFileName, DataOceanHelper.CustomHeaders(authn)));

              if (deleteImportedFile.ImportedFileType == ImportedFileType.Alignment)
              {
                //Do we care if deleting generated DXF file fails?
                tasks.Add(DataOceanHelper.DeleteFileFromDataOcean(
                  dxfFileName, deleteImportedFile.DataOceanRootFolder, customerUid,
                  deleteImportedFile.ProjectUid, deleteImportedFile.ImportedFileUid, log, dataOceanClient, authn, configStore));
              }
              await Task.WhenAll(tasks);
            }
          }
        }

        if (importedFileInternalResult != null)
        {
          await ImportedFileRequestDatabaseHelper.UndeleteImportedFile
            (deleteImportedFile.ProjectUid, deleteImportedFile.ImportedFileUid, serviceExceptionHandler, projectRepo)
            .ConfigureAwait(false);
          serviceExceptionHandler.ThrowServiceException(importedFileInternalResult.StatusCode, importedFileInternalResult.ErrorNumber, importedFileInternalResult.ResultCode, importedFileInternalResult.ErrorMessage1);
        }
      }

      return new ContractExecutionResult();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    private async Task CheckIfUsedInFilter(DeleteImportedFile deleteImportedFile)
    {
      if (deleteImportedFile.IsDesignFileType)
      {
        var filters = await ImportedFileRequestDatabaseHelper.GetFilters(deleteImportedFile.ProjectUid, customHeaders, filterServiceProxy);
        if (filters != null && filters.Any())
        {
          var fileUidStr = deleteImportedFile.ImportedFileUid.ToString();
          if (filters.Any(f => f.DesignUid == fileUidStr || f.AlignmentUid == fileUidStr))
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 112);
          }
        }
      }
    }

    private async Task CheckIfHasReferenceSurfacesAsync(DeleteImportedFile deleteImportedFile)
    {
      //Cannot delete a design which has reference surfaces associated with it
      if (deleteImportedFile.ImportedFileType == ImportedFileType.DesignSurface)
      {
        var children = await projectRepo.GetReferencedImportedFiles(deleteImportedFile.ImportedFileUid.ToString());
        if (children != null && children.Any())
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 119);
        }
      }
    }
  }
}
