using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which creates a project - appropriate for v2 and v4 controllers
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
      UpdateImportedFile importedFileUpsertEvent = item as UpdateImportedFile;
      if (importedFileUpsertEvent == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 68);
        return new ContractExecutionResult(); // keeps compiler happy
      }

      string fileSpaceId = configStore.GetValueString("TCCFILESPACEID");
      bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT", "FALSE"),
        out var useTrexGatewayDesignImport);
      bool.TryParse(configStore.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT", "TRUE"),
        out var useRaptorGatewayDesignImport);
      var isDesignFileType = importedFileUpsertEvent.ImportedFileType == ImportedFileType.DesignSurface ||
                             importedFileUpsertEvent.ImportedFileType == ImportedFileType.SurveyedSurface;

      FileDescriptor fileDescriptor = null;
      var existing = await projectRepo.GetImportedFile(importedFileUpsertEvent.ImportedFileUid.ToString())
        .ConfigureAwait(false);

      using (var fileStream = new FileStream(importedFileUpsertEvent.FilePathAndFileName, FileMode.Open))
      {
        if (useTrexGatewayDesignImport && isDesignFileType)
        {
          fileDescriptor = ProjectRequestHelper.WriteFileToS3Repository(
            fileStream, importedFileUpsertEvent.ProjectUid.ToString(), importedFileUpsertEvent.FileName,
            importedFileUpsertEvent.ImportedFileType == ImportedFileType.SurveyedSurface,
            importedFileUpsertEvent.SurveyedUtc,
            log, serviceExceptionHandler, persistantTransferProxy);
        }

        if (useRaptorGatewayDesignImport)
        {
          fileDescriptor = await TccHelper.WriteFileToTCCRepository(
              fileStream, customerUid, importedFileUpsertEvent.ProjectUid.ToString(),
              importedFileUpsertEvent.FilePathAndFileName,
              importedFileUpsertEvent.ImportedFileType == ImportedFileType.SurveyedSurface,
              importedFileUpsertEvent.SurveyedUtc, fileSpaceId, log, serviceExceptionHandler, fileRepo)
            .ConfigureAwait(false);
        }
      }

      if (useTrexGatewayDesignImport && isDesignFileType)
      {
        // todoJeannie
        await ImportedFileRequestHelper.NotifyTRexUpdateFile(importedFileUpsertEvent.ProjectUid,
          importedFileUpsertEvent.ImportedFileType, importedFileUpsertEvent.FileName, importedFileUpsertEvent.ImportedFileUid,
          importedFileUpsertEvent.SurveyedUtc,
          log, customHeaders, serviceExceptionHandler).ConfigureAwait(false);
      }

      if (useRaptorGatewayDesignImport)
      {
        var addFileResult = await ImportedFileRequestHelper.NotifyRaptorAddFile(
            importedFileUpsertEvent.LegacyProjectId, Guid.Parse(importedFileUpsertEvent.ProjectUid.ToString()),
            importedFileUpsertEvent.ImportedFileType, importedFileUpsertEvent.DxfUnitsTypeId,
            fileDescriptor, importedFileUpsertEvent.ImportedFileId,
            Guid.Parse(importedFileUpsertEvent.ImportedFileUid.ToString()), false, log, customHeaders,
            serviceExceptionHandler, raptorProxy,
            projectRepo)
          .ConfigureAwait(false);
        existing.MinZoomLevel = addFileResult.MinZoomLevel;
        existing.MaxZoomLevel = addFileResult.MaxZoomLevel;
      }

      // if all succeeds, update Db and  put update to kafka que
      var updateImportedFileEvent = await ImportedFileRequestDatabaseHelper.UpdateImportedFileInDb(existing,
          JsonConvert.SerializeObject(fileDescriptor),
          importedFileUpsertEvent.SurveyedUtc, existing.MinZoomLevel, existing.MaxZoomLevel,
          importedFileUpsertEvent.FileCreatedUtc, importedFileUpsertEvent.FileUpdatedUtc, userEmailAddress,
          log, serviceExceptionHandler, projectRepo)
        .ConfigureAwait(false);

      var messagePayload = JsonConvert.SerializeObject(new { UpdateImportedFileEvent = updateImportedFileEvent });
        producer.Send(kafkaTopicName,
          new List<KeyValuePair<string, string>>
          {
            new KeyValuePair<string, string>(updateImportedFileEvent.ImportedFileUID.ToString(), messagePayload)
          });

      var importedFile = new ImportedFileDescriptorSingleResult(
        (await ImportedFileRequestDatabaseHelper.GetImportedFileList(importedFileUpsertEvent.ProjectUid.ToString(), log, userId, projectRepo).ConfigureAwait(false))
        .ToImmutableList()
        .FirstOrDefault(f => f.ImportedFileUid == importedFileUpsertEvent.ImportedFileUid.ToString())
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