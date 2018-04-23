using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  /// The executor which creates a project - appropriate for v2 and v4 controllers
  /// </summary>
  public class UpsertImportedFileExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// Processes the Upsert
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a ContractExecutionResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      ImportedFileUpsertEvent importedFileUpsertEvent = item as ImportedFileUpsertEvent;
      if (importedFileUpsertEvent == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 68 /* todo */);
        return new ContractExecutionResult();
      }

      var importedFiles = await ImportedFileRequestHelper.GetImportedFiles(importedFileUpsertEvent.Project.ProjectUID, log, projectRepo).ConfigureAwait(false);
      ImportedFile existing = null;
      if (importedFiles.Count > 0)
      {
        existing = importedFiles.FirstOrDefault(
          f => string.Equals(f.Name, importedFileUpsertEvent.FileDescriptor.BaseFileName(), StringComparison.OrdinalIgnoreCase)
               && f.ImportedFileType == importedFileUpsertEvent.ImportedFileTypeId
               && (
                 importedFileUpsertEvent.ImportedFileTypeId == ImportedFileType.SurveyedSurface &&
                 f.SurveyedUtc == importedFileUpsertEvent.SurveyedUtc ||
                 importedFileUpsertEvent.ImportedFileTypeId != ImportedFileType.SurveyedSurface
               ));
      }
      bool creating = existing == null;
      log.LogInformation(
        creating
          ? $"UpdateImportedFileV4. file doesn't exist already in DB: {importedFileUpsertEvent.FileDescriptor.fileName} projectUid {importedFileUpsertEvent.Project.ProjectUID} ImportedFileType: {importedFileUpsertEvent.ImportedFileTypeId} surveyedUtc {(importedFileUpsertEvent.SurveyedUtc == null ? "N/A" : importedFileUpsertEvent.SurveyedUtc.ToString())}"
          : $"UpdateImportedFileV4. file exists already in DB. Will be updated: {JsonConvert.SerializeObject(existing)}");


      // if all succeeds, send insert to Db and kafka que
      var importedFileUid = existing?.ImportedFileUid;
      var importedFileId = existing?.ImportedFileId;
      CreateImportedFileEvent createImportedFileEvent = null;
      if (creating)
      {
        // need to write to Db prior to notifying raptor, as raptor needs the legacyImportedFileID 
        createImportedFileEvent = await ImportedFileRequestHelper.CreateImportedFileinDb(Guid.Parse(customerUid), Guid.Parse(importedFileUpsertEvent.Project.ProjectUID),
            importedFileUpsertEvent.ImportedFileTypeId, importedFileUpsertEvent.DxfUnitsTypeId, importedFileUpsertEvent.FileDescriptor.BaseFileName(), importedFileUpsertEvent.SurveyedUtc, 
            JsonConvert.SerializeObject(importedFileUpsertEvent.FileDescriptor),
            importedFileUpsertEvent.FileCreatedUtc, importedFileUpsertEvent.FileUpdatedUtc, userEmailAddress,
            log, serviceExceptionHandler, projectRepo)
          .ConfigureAwait(false);
        importedFileUid = createImportedFileEvent.ImportedFileUID.ToString();
        importedFileId = createImportedFileEvent.ImportedFileID;
      }

      var result = await ImportedFileRequestHelper.NotifyRaptorAddFile(importedFileUpsertEvent.Project.LegacyProjectID, Guid.Parse(importedFileUpsertEvent.Project.ProjectUID),
          importedFileUpsertEvent.ImportedFileTypeId, importedFileUpsertEvent.DxfUnitsTypeId, importedFileUpsertEvent.FileDescriptor, importedFileId.Value,
          Guid.Parse(importedFileUid), (existing == null), log, customHeaders, serviceExceptionHandler, raptorProxy, projectRepo)
        .ConfigureAwait(false);

      if (creating)
      {
        createImportedFileEvent.MinZoomLevel = result.MinZoomLevel;
        createImportedFileEvent.MaxZoomLevel = result.MaxZoomLevel;
        existing = await projectRepo.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString())
          .ConfigureAwait(false);
      }

      //Need to update zoom levels in Db for both create  and update
      var updateImportedFileEvent = await ImportedFileRequestHelper.UpdateImportedFileInDb(existing, 
          JsonConvert.SerializeObject(importedFileUpsertEvent.FileDescriptor),
          importedFileUpsertEvent.SurveyedUtc, result.MinZoomLevel, result.MaxZoomLevel,
          importedFileUpsertEvent.FileCreatedUtc, importedFileUpsertEvent.FileUpdatedUtc, userEmailAddress,
          log, serviceExceptionHandler, projectRepo)
        .ConfigureAwait(false);

      // if all succeeds, update Db (if not Create) and send create/update to kafka que
      if (!creating) // update
      {
        var messagePayload = JsonConvert.SerializeObject(new { UpdateImportedFileEvent = updateImportedFileEvent });
        producer.Send(kafkaTopicName,
          new List<KeyValuePair<string, string>>
          {
            new KeyValuePair<string, string>(updateImportedFileEvent.ImportedFileUID.ToString(), messagePayload)
          });
      }
      else
      {
        var messagePayload = JsonConvert.SerializeObject(new { CreateImportedFileEvent = createImportedFileEvent });
        producer.Send(kafkaTopicName,
          new List<KeyValuePair<string, string>>
          {
            new KeyValuePair<string, string>(createImportedFileEvent.ImportedFileUID.ToString(), messagePayload)
          });
      }

      return new ImportedFileDescriptorSingleResult(
        (await ImportedFileRequestHelper.GetImportedFileList(importedFileUpsertEvent.Project.ProjectUID, log, userId, projectRepo).ConfigureAwait(false))
        .ToImmutableList()
        .FirstOrDefault(f => f.ImportedFileUid == importedFileUid)
      );
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    protected override void ProcessErrorCodes()
    {
    }
   
  }
}