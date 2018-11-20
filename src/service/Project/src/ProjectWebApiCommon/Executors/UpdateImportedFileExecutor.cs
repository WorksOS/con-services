using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which Updates an importedFile
  /// 
  /// For Raptor, the file is stored on TCC and notified to Raptor via a 3dp notification (1 for add/update)
  ///        Min/Max zoom is returned from 3dp
  /// For TRex, the file is stored on S3 and notified to Trex via a 3dp notification (1 for add and another for update)
  ///        Min/max zoom will not be determined this way for TRex-only (todo Elspeth?)
  ///        It continues to update the FileDescription to the DB
  ///
  /// </summary>
  public class UpdateImportedFileExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the Upsert
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a ContractExecutionResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      UpdateImportedFile updateImportedFile = item as UpdateImportedFile;
      if (updateImportedFile == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 68);
        return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "shouldn't get here"); // to keep compiler happy
      }

      bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT"), out var useTrexGatewayDesignImport);
      bool.TryParse(configStore.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT"), out var useRaptorGatewayDesignImport);
      var isDesignFileType = updateImportedFile.ImportedFileType == ImportedFileType.DesignSurface ||
                             updateImportedFile.ImportedFileType == ImportedFileType.SurveyedSurface;

      var existing = await projectRepo.GetImportedFile(updateImportedFile.ImportedFileUid.ToString())
        .ConfigureAwait(false);

      if (useTrexGatewayDesignImport && isDesignFileType)
      {
        await ImportedFileRequestHelper.NotifyTRexUpdateFile(updateImportedFile.ProjectUid,
          updateImportedFile.ImportedFileType, updateImportedFile.FileDescriptor.fileName, updateImportedFile.ImportedFileUid,
          updateImportedFile.SurveyedUtc,  // todoJeannie
          log, customHeaders, serviceExceptionHandler,
          tRexImportFileProxy, projectRepo).ConfigureAwait(false);
      }

      if (useRaptorGatewayDesignImport)
      {
        var addFileResult = await ImportedFileRequestHelper.NotifyRaptorAddFile(
            updateImportedFile.LegacyProjectId, Guid.Parse(updateImportedFile.ProjectUid.ToString()),
            updateImportedFile.ImportedFileType, updateImportedFile.DxfUnitsTypeId,
            updateImportedFile.FileDescriptor, updateImportedFile.ImportedFileId,
            Guid.Parse(updateImportedFile.ImportedFileUid.ToString()), false, log, customHeaders,
            serviceExceptionHandler, raptorProxy,
            projectRepo)
          .ConfigureAwait(false);
        existing.MinZoomLevel = addFileResult.MinZoomLevel;
        existing.MaxZoomLevel = addFileResult.MaxZoomLevel;
      }

      // if all succeeds, update Db and  put update to kafka que
      var updateImportedFileEvent = await ImportedFileRequestDatabaseHelper.UpdateImportedFileInDb(existing,
          JsonConvert.SerializeObject(existing.FileDescriptor),
          updateImportedFile.SurveyedUtc, existing.MinZoomLevel, existing.MaxZoomLevel,
          updateImportedFile.FileCreatedUtc, updateImportedFile.FileUpdatedUtc, userEmailAddress,
          log, serviceExceptionHandler, projectRepo)
        .ConfigureAwait(false);

      var messagePayload = JsonConvert.SerializeObject(new { UpdateImportedFileEvent = updateImportedFileEvent });
        producer.Send(kafkaTopicName,
          new List<KeyValuePair<string, string>>
          {
            new KeyValuePair<string, string>(updateImportedFileEvent.ImportedFileUID.ToString(), messagePayload)
          });

      var importedFile = new ImportedFileDescriptorSingleResult(
        (await ImportedFileRequestDatabaseHelper.GetImportedFileList(updateImportedFile.ProjectUid.ToString(), log, userId, projectRepo).ConfigureAwait(false))
        .ToImmutableList()
        .FirstOrDefault(f => f.ImportedFileUid == updateImportedFile.ImportedFileUid.ToString())
      );

      log.LogInformation(
        $"UpdateImportedFileExecutor. entry {(importedFile.ImportedFileDescriptor == null ? "not " : "")}retrieved from DB : {JsonConvert.SerializeObject(importedFile)}");

      return importedFile;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}