using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
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
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a ContractExecutionResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var deleteImportedFile = item as DeleteImportedFile;
      if (deleteImportedFile == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 68);
        return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "shouldn't get here"); // to keep compiler happy
      }

      await CheckIfUsedInAFilterAsync(deleteImportedFile).ConfigureAwait(false);

      bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT"), out var useTrexGatewayDesignImport);
      bool.TryParse(configStore.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT"), out var useRaptorGatewayDesignImport);
      var isDesignFileType = deleteImportedFile.ImportedFileType == ImportedFileType.DesignSurface ||
                             deleteImportedFile.ImportedFileType == ImportedFileType.SurveyedSurface;

      DeleteImportedFileEvent deleteImportedFileEvent = null;
      if (useTrexGatewayDesignImport && isDesignFileType)
      {
        await ImportedFileRequestHelper.NotifyTRexDeleteFile(deleteImportedFile.ProjectUid,
          deleteImportedFile.ImportedFileType, deleteImportedFile.FileDescriptor.fileName,
          deleteImportedFile.ImportedFileUid,
          deleteImportedFile.SurveyedUtc, // todoJeannie
          log, customHeaders, serviceExceptionHandler,
          tRexImportFileProxy, projectRepo).ConfigureAwait(false);

        // DB change must be made before raptorProxy.DeleteFile is called as it calls back here to get list of Active files
        deleteImportedFileEvent = await ImportedFileRequestDatabaseHelper.DeleteImportedFileInDb
          (deleteImportedFile.ProjectUid, deleteImportedFile.ImportedFileUid, serviceExceptionHandler, projectRepo, false)
          .ConfigureAwait(false);
      }

      if (useRaptorGatewayDesignImport)
      {
        // DB change must be made before raptorProxy.DeleteFile is called as it calls back here to get list of Active files
        if (deleteImportedFileEvent == null)
        {
          deleteImportedFileEvent = await ImportedFileRequestDatabaseHelper.DeleteImportedFileInDb
            (deleteImportedFile.ProjectUid, deleteImportedFile.ImportedFileUid, serviceExceptionHandler, projectRepo, false)
            .ConfigureAwait(false);
        }

        var importedFileInternalResult = await ImportedFileRequestHelper.NotifyRaptorDeleteFile
          (deleteImportedFile.ProjectUid, deleteImportedFile.ImportedFileType,
            deleteImportedFile.ImportedFileUid, deleteImportedFile.FileDescriptor,
            deleteImportedFile.ImportedFileId, deleteImportedFile.LegacyImportedFileId,
            log, customHeaders, serviceExceptionHandler,
            projectRepo, raptorProxy)
          .ConfigureAwait(false);

        if (importedFileInternalResult == null)
        {
          importedFileInternalResult = await TccHelper.DeleteFileFromTCCRepository
            (deleteImportedFile.FileDescriptor, deleteImportedFile.ProjectUid, deleteImportedFile.ImportedFileUid,
            log, serviceExceptionHandler, fileRepo, projectRepo)
            .ConfigureAwait(false);
        }
        if (importedFileInternalResult != null)
        {
          await ImportedFileRequestDatabaseHelper.UndeleteImportedFile
            (deleteImportedFile.ProjectUid, deleteImportedFile.ImportedFileUid, serviceExceptionHandler, projectRepo)
            .ConfigureAwait(false);
          serviceExceptionHandler.ThrowServiceException(importedFileInternalResult.StatusCode, importedFileInternalResult.ErrorNumber, importedFileInternalResult.ResultCode, importedFileInternalResult.ErrorMessage1);
        }
      }

      var messagePayload = JsonConvert.SerializeObject(new { DeleteImportedFileEvent = deleteImportedFileEvent });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>
        {
          new KeyValuePair<string, string>(deleteImportedFileEvent.ImportedFileUID.ToString(), messagePayload)
        });

      return new ContractExecutionResult();
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    private async Task CheckIfUsedInAFilterAsync(DeleteImportedFile deleteImportedFile)
    {
      //Cannot delete a design that is used in a filter
      //TODO: When scheduled reports are implemented, extend this check to them as well.
      var t1 = configStore.GetValueString("FILTER_API_URL");  //todoJeannie
      var t2 = configStore.GetValueString("FILTER_CACHE_LIFE"); //todoJeannie
      if (deleteImportedFile.ImportedFileType == ImportedFileType.DesignSurface || deleteImportedFile.ImportedFileType == ImportedFileType.Alignment)
      {
        var filters = await ImportedFileRequestDatabaseHelper.GetFilters
          (deleteImportedFile.ProjectUid, customHeaders, filterServiceProxy);
        if (filters != null)
        {
          var fileUidStr = deleteImportedFile.ImportedFileUid.ToString();
          if (filters.Any(f => f.DesignUid == fileUidStr || f.AlignmentUid == fileUidStr))
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 112);
          }
        }
      }
    }

  }
}