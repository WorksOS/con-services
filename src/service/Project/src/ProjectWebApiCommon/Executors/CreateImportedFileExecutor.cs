using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
  /// Do the import of a file uploaded directly, or via scheduler 
  /// This can be called by the background upload
  ///      (file stored in TEMPRORAY vs-exports- bucket in AWS, then re downloaded with scheduler request),
  ///      or Synchronise upload (file stored locally)
  ///
  /// For Raptor, the file is stored on TCC and notified to Raptor via a 3dp notification (1 for add/update)
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
  public class CreateImportedFileExecutor : RequestExecutorContainer
  {

    /// <summary>
    /// Adds file via Raptor and/or Trex
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a ContractExecutionResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var createimportedfile = item as CreateImportedFile;
      if (createimportedfile == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 68);
        return new ContractExecutionResult(); // keeps compiler happy
      }

      bool.TryParse(configStore.GetValueString("ENABLE_TREX_GATEWAY_DESIGNIMPORT", "FALSE"),
        out var useTrexGatewayDesignImport);
      bool.TryParse(configStore.GetValueString("ENABLE_RAPTOR_GATEWAY_DESIGNIMPORT", "TRUE"),
        out var useRaptorGatewayDesignImport);
      var isDesignFileType = createimportedfile.ImportedFileType == ImportedFileType.DesignSurface ||
                             createimportedfile.ImportedFileType == ImportedFileType.SurveyedSurface;

      // need to write to Db prior to notifying raptor, as raptor needs the legacyImportedFileID 
      var createImportedFileEvent = await ImportedFileRequestDatabaseHelper.CreateImportedFileinDb(
          Guid.Parse(customerUid),
          createimportedfile.ProjectUid,
          createimportedfile.ImportedFileType, createimportedfile.DxfUnitsType, createimportedfile.FileName,
          createimportedfile.SurveyedUtc, JsonConvert.SerializeObject(createimportedfile.FileDescriptor),
          createimportedfile.FileCreatedUtc, createimportedfile.FileUpdatedUtc, userEmailAddress,
          log, serviceExceptionHandler, projectRepo)
        .ConfigureAwait(false);

      if (useTrexGatewayDesignImport && isDesignFileType)
      {
        // todoJeannie
        var result = await ImportedFileRequestHelper.NotifyTRexAddFile(createimportedfile.ProjectUid,
          createimportedfile.ImportedFileType, createimportedfile.FileName, createImportedFileEvent.ImportedFileUID,
          createimportedfile.SurveyedUtc,
          log, customHeaders, serviceExceptionHandler).ConfigureAwait(false);
      }

      if (useRaptorGatewayDesignImport)
      {
        var project =
          await ProjectRequestHelper.GetProject(createimportedfile.ProjectUid.ToString(), customerUid, log,
            serviceExceptionHandler,
            projectRepo);

        var addFileResult = await ImportedFileRequestHelper.NotifyRaptorAddFile(project.LegacyProjectID,
          createimportedfile.ProjectUid,
          createimportedfile.ImportedFileType, createimportedfile.DxfUnitsType, createimportedfile.FileDescriptor,
          createImportedFileEvent.ImportedFileID, createImportedFileEvent.ImportedFileUID, true,
          log, customHeaders, serviceExceptionHandler, raptorProxy, projectRepo).ConfigureAwait(false);
        createImportedFileEvent.MinZoomLevel = addFileResult.MinZoomLevel;
        createImportedFileEvent.MaxZoomLevel = addFileResult.MaxZoomLevel;

        var existing = await projectRepo.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString())
          .ConfigureAwait(false);

        //Need to update zoom levels in Db (Raptor - todo is this still needed?)  
        _ = await ImportedFileRequestDatabaseHelper.UpdateImportedFileInDb(existing,
            JsonConvert.SerializeObject(createimportedfile.FileDescriptor),
            createimportedfile.SurveyedUtc, createImportedFileEvent.MinZoomLevel, createImportedFileEvent.MaxZoomLevel,
            createimportedfile.FileCreatedUtc, createimportedfile.FileUpdatedUtc, userEmailAddress,
            log, serviceExceptionHandler, projectRepo)
          .ConfigureAwait(false);
      }

      var messagePayload = JsonConvert.SerializeObject(new {CreateImportedFileEvent = createImportedFileEvent});
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>
        {
          new KeyValuePair<string, string>(createImportedFileEvent.ImportedFileUID.ToString(), messagePayload)
        });

      var importedFile = new ImportedFileDescriptorSingleResult(
        (await ImportedFileRequestDatabaseHelper
          .GetImportedFileList(createimportedfile.ProjectUid.ToString(), log, userId, projectRepo)
          .ConfigureAwait(false))
        .ToImmutableList()
        .First(f => f.ImportedFileUid == createImportedFileEvent.ImportedFileUID.ToString())
      );

      log.LogInformation(
        $"CreateImportedFileV4. completed successfully. Response: {JsonConvert.SerializeObject(importedFile)}");
      return importedFile;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

  }
}