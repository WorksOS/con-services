using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.FlowJSHandler;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Project.WebAPI.Factories;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// File Import controller v4
  /// </summary>
  public class FileImportV4Controller : FileImportBaseController
  {
    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;


    /// <summary>
    /// File import controller v4
    /// </summary>
    /// <param name="producer"></param>
    /// <param name="store"></param>
    /// <param name="logger"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="raptorProxy"></param>
    /// <param name="projectRepo"></param>
    /// <param name="requestFactory"></param>
    /// <param name="subscriptionRepo"></param>
    /// <param name="fileRepo"></param>
    public FileImportV4Controller(IKafka producer,
      IConfigurationStore store, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler,
      IRaptorProxy raptorProxy, 
      IProjectRepository projectRepo,  ISubscriptionRepository subscriptionRepo, 
      IFileRepository fileRepo, IRequestFactory requestFactory)
      : base(producer, store, logger, logger.CreateLogger<FileImportV4Controller>(), serviceExceptionHandler, 
        raptorProxy,
        projectRepo, subscriptionRepo, fileRepo, requestFactory)
    {
      this.logger = logger;
      fileSpaceId = store.GetValueString("TCCFILESPACEID");
      if (string.IsNullOrEmpty(fileSpaceId))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 48);
      }
    }

    // GET: api/v4/importedfiles
    /// <summary>
    /// Gets a list of imported files for a project. The list includes files of all types.
    /// </summary>
    /// <returns>A list of files</returns>
    [Route("api/v4/importedfiles")]
    [HttpGet]
    public async Task<ImportedFileDescriptorListResult> GetImportedFilesV4([FromQuery] string projectUid)
    {
      log.LogInformation("GetImportedFilesV4");

      return new ImportedFileDescriptorListResult
      {
        ImportedFileDescriptors = await ImportedFileRequestHelper.GetImportedFileList(projectUid, log, userId, projectRepo).ConfigureAwait(false)
      };
    }

    /// <summary>
    /// Sets activated state on one or more imported files.
    /// </summary>
    /// <param name="projectUid">Project identifier</param>
    /// <param name="request">Collection of file Uids to set the activated state on</param>
    [Route("api/v4/importedfiles")]
    [HttpPut]
    public async Task<IActionResult> UpdateImportedFileActivationStateV4(string projectUid, [FromBody] ActivatedImportFilesRequest request)
    {
      const string functionId = "SetImportedFileActivatedStateV4";
      log.LogInformation("ActivateFiles");

      await ValidateProjectId(projectUid).ConfigureAwait(false);

      if (request == null)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 40);

      var fileIds = string.Join(",", request.ImportedFileDescriptors.Select(x => x.ImportedFileUid));
      if (string.IsNullOrEmpty(fileIds))
      {
        return Ok(new { Code = HttpStatusCode.BadRequest, Message = "Request contains no imported file IDs." });
      }

      log.LogInformation($"{functionId}. projectUid: {projectUid}, fileUids: {fileIds}");

      var importedFiles = await ImportedFileRequestHelper.GetImportedFileList(projectUid, log, userId, projectRepo).ConfigureAwait(false);
      if (!importedFiles.Any())
      {
        log.LogInformation($"{functionId}. Attempt to set file activation state when project contains no files");

        return Ok(new { Code = HttpStatusCode.BadRequest, Message = "Project contains no imported files." });
      }

      var filesToUpdate = new Dictionary<Guid, bool>();

      foreach (var activatedFileDescriptor in request.ImportedFileDescriptors)
      {
        var existingFile = importedFiles.FirstOrDefault(f => f.ImportedFileUid == activatedFileDescriptor.ImportedFileUid);
        if (existingFile == null)
        {
          log.LogError(
            $"{functionId}. File doesn't exist. projectUid {projectUid}, fileUid: {activatedFileDescriptor.ImportedFileUid}");
          continue;
        }

        if (existingFile.IsActivated == activatedFileDescriptor.IsActivated)
        {
          log.LogDebug(
            $"{functionId}. File activation state is already set to {existingFile.IsActivated}. No changes required. {existingFile.ImportedFileUid}");
          continue;
        }

        log.LogInformation(
          $"{functionId}. File queued for updating: {JsonConvert.SerializeObject(existingFile)}");
        filesToUpdate.Add(new Guid(activatedFileDescriptor.ImportedFileUid), activatedFileDescriptor.IsActivated);
      }

      if (!filesToUpdate.Any())
      {
        log.LogInformation($"{functionId}. No files eligible for activation state change.");

        return Ok(new { Code = HttpStatusCode.OK, Message = "Success" });
      }

      try
      {
        var dbUpdateResult = await SetFileActivatedState(projectUid, filesToUpdate);
        await NotifyRaptorUpdateFile(new Guid(projectUid), dbUpdateResult).ConfigureAwait(false);

        return Ok(new { Code = HttpStatusCode.OK, Message = "Success" });
      }
      catch (Exception exception)
      {
        return new JsonResult(new { Code = HttpStatusCode.InternalServerError, exception.GetBaseException().Message });
      }
    }

    /// <summary>
    /// Used as a callback by Flow.JS
    /// </summary>
    /// <returns></returns>
    [Route("api/v4/importedfile")]
    [HttpGet]
    public ActionResult Upload()
    {
      return new NoContentResult();
    }

    // POST: api/v4/importedfile
    /// <summary>
    /// Import a design file
    ///    this creates a link between the file and project in database,
    ///      sends file to TCC
    ///      and notifies RaptorWebApi
    /// </summary>
    /// <param name="file"></param>
    /// <param name="projectUid"></param>
    /// <param name="importedFileType"></param>
    /// <param name="dxfUnitsType">A DXF file units type</param>
    /// <param name="fileUpdatedUtc"></param>
    /// <param name="fileCreatedUtc"></param>
    /// <param name="surveyedUtc"></param>
    /// <remarks>Import a design file for a project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v4/importedfile")]
    [HttpPost]
    [ActionName("Upload")]
    [FlowUpload(Extensions = new[]
    {
      "svl", "dxf", "ttm"
    }, Size = 1000000000)]

    public async Task<ImportedFileDescriptorSingleResult> CreateImportedFileV4(
      FlowFile file,
      [FromUri] Guid projectUid, 
      [FromUri] ImportedFileType importedFileType,
      [FromUri] DxfUnitsType dxfUnitsType,
      [FromUri] DateTime fileCreatedUtc, 
      [FromUri] DateTime fileUpdatedUtc,
      [FromUri] DateTime? surveyedUtc = null)
    {
      FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType, dxfUnitsType, fileCreatedUtc,
        fileUpdatedUtc, userEmailAddress, surveyedUtc);
      log.LogInformation(
        $"CreateImportedFileV4. file: {file.flowFilename} path {file.path} projectUid {projectUid.ToString()} ImportedFileType: {importedFileType} " +
        $"DxfUnitsType: {dxfUnitsType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}");

      if (!System.IO.File.Exists(file.path))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 55);
      }

      var project = await ProjectRequestHelper.GetProject(projectUid.ToString(), customerUid, log, serviceExceptionHandler, projectRepo);

      var importedFileList = await ImportedFileRequestHelper.GetImportedFileList(projectUid.ToString(), log, userId, projectRepo).ConfigureAwait(false);
      ImportedFileDescriptor importedFileDescriptor = null;
      if (importedFileList.Count > 0)
        importedFileDescriptor = importedFileList.FirstOrDefault(
          f => string.Equals(f.Name, file.flowFilename, StringComparison.OrdinalIgnoreCase)
               && f.ImportedFileType == importedFileType
               && (importedFileType != ImportedFileType.SurveyedSurface || f.SurveyedUtc == surveyedUtc));

      if (importedFileDescriptor != null)
      {
        var message = $"CreateImportedFileV4. File: {file.flowFilename} has already been imported.";
        log.LogError(message);
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 58);
      }

      /*** now making changes, potentially needing rollback ***/
      FileDescriptor fileDescriptor = null;
      using (var fileStream = new FileStream(file.path, FileMode.Open))
      {
        fileDescriptor = await ProjectRequestHelper.WriteFileToTCCRepository(
          fileStream, customerUid, projectUid.ToString(), file.path, importedFileType == ImportedFileType.SurveyedSurface, 
          surveyedUtc, fileSpaceId, log, serviceExceptionHandler, fileRepo)
          .ConfigureAwait(false);
      }

      // need to write to Db prior to notifying raptor, as raptor needs the legacyImportedFileID 
      CreateImportedFileEvent createImportedFileEvent = await ImportedFileRequestHelper.CreateImportedFileinDb(Guid.Parse(customerUid), projectUid,
          importedFileType, dxfUnitsType, file.flowFilename, surveyedUtc, JsonConvert.SerializeObject(fileDescriptor),
          fileCreatedUtc, fileUpdatedUtc, userEmailAddress,
          log, serviceExceptionHandler, projectRepo)
        .ConfigureAwait(false);

      var result = await ImportedFileRequestHelper.NotifyRaptorAddFile(project.LegacyProjectID, projectUid, importedFileType, dxfUnitsType, fileDescriptor,
        createImportedFileEvent.ImportedFileID, createImportedFileEvent.ImportedFileUID, true,
        log, customHeaders, serviceExceptionHandler, raptorProxy, projectRepo).ConfigureAwait(false);
      createImportedFileEvent.MinZoomLevel = result.MinZoomLevel;
      createImportedFileEvent.MaxZoomLevel = result.MaxZoomLevel;
      var existing = await projectRepo.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString())
        .ConfigureAwait(false);
      //Need to update zoom levels in Db 
      var updateImportedFileEvent = await ImportedFileRequestHelper.UpdateImportedFileInDb(existing, JsonConvert.SerializeObject(fileDescriptor),
          surveyedUtc, result.MinZoomLevel, result.MaxZoomLevel,
          fileCreatedUtc, fileUpdatedUtc, userEmailAddress,
          log, serviceExceptionHandler, projectRepo)
        .ConfigureAwait(false);

      var messagePayload = JsonConvert.SerializeObject(new { CreateImportedFileEvent = createImportedFileEvent });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>
        {
          new KeyValuePair<string, string>(createImportedFileEvent.ImportedFileUID.ToString(), messagePayload)
        });

      // Flow has it attached. Trying to delete results in 'The process cannot access the file '...' because it is being used by another process'
      //System.IO.File.Delete(file.path);

      var importedFile = new ImportedFileDescriptorSingleResult(
        (await ImportedFileRequestHelper.GetImportedFileList(projectUid.ToString(), log, userId, projectRepo).ConfigureAwait(false))
        .ToImmutableList()
        .First(f => f.ImportedFileUid == createImportedFileEvent.ImportedFileUID.ToString())
      );
      log.LogInformation(
        $"CreateImportedFileV4. completed succesfully. Response: {JsonConvert.SerializeObject(importedFile)}");
      return importedFile;
    }

    // PUT: api/v4/importedfile
    /// <summary>
    /// Upsert imported file
    ///   this creates/updates database AND creates/updates file in TCC.
    ///   notify RaptorWebAPI.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="projectUid"></param>
    /// <param name="importedFileType"></param>
    /// <param name="dxfUnitsType">A DXF file units type</param>
    /// <param name="fileCreatedUtc"></param>
    /// <param name="fileUpdatedUtc"></param>
    /// <param name="surveyedUtc"></param>
    /// <remarks>Updates and Imported design file for a project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v4/importedfile")]
    [HttpPut]
    [ActionName("Upload")]
    [FlowUpload(Extensions = new[]
    {
      "svl", "dxf", "ttm"
    }, Size = 1000000000)]

    public async Task<ImportedFileDescriptorSingleResult> UpsertImportedFileV4(
      FlowFile file,
      [FromUri] Guid projectUid, 
      [FromUri] ImportedFileType importedFileType,
      [FromUri] DxfUnitsType dxfUnitsType,
      [FromUri] DateTime fileCreatedUtc, 
      [FromUri] DateTime fileUpdatedUtc,
      [FromUri] DateTime? surveyedUtc = null)
    {
      FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType, dxfUnitsType, fileCreatedUtc,
        fileUpdatedUtc, userEmailAddress, surveyedUtc);
      log.LogInformation(
        $"UpdateImportedFileV4. file: {JsonConvert.SerializeObject(file)} projectUid {projectUid} ImportedFileType: {importedFileType} DxfUnitsType: {dxfUnitsType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}");

      if (!System.IO.File.Exists(file.path))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 55);
      }

      // this also validates that this customer has access to the projectUid
      var project = await ProjectRequestHelper.GetProject(projectUid.ToString(), customerUid, log, serviceExceptionHandler, projectRepo);

      FileDescriptor fileDescriptor = null;
      using (var fileStream = new FileStream(file.path, FileMode.Open))
      {
        fileDescriptor = await ProjectRequestHelper.WriteFileToTCCRepository(
          fileStream, customerUid, projectUid.ToString(), file.path, importedFileType == ImportedFileType.SurveyedSurface, 
          surveyedUtc, fileSpaceId, log, serviceExceptionHandler, fileRepo)
          .ConfigureAwait(false);
      }

      var importedFileUpsertEvent = ImportedFileUpsertEvent.CreateImportedFileUpsertEvent(
         project, importedFileType,
          importedFileType == ImportedFileType.SurveyedSurface
                        ? surveyedUtc : null,
          dxfUnitsType, fileCreatedUtc, fileUpdatedUtc, fileDescriptor
        );

        var importedFile = await WithServiceExceptionTryExecuteAsync(() =>
          RequestExecutorContainerFactory
            .Build<UpsertImportedFileExecutor>(logger, configStore, serviceExceptionHandler,
              customerUid, userId, userEmailAddress, customHeaders,
              producer, kafkaTopicName,
              null, raptorProxy, null,
              projectRepo, null, fileRepo)
            .ProcessAsync(importedFileUpsertEvent)
        ) as ImportedFileDescriptorSingleResult;

      //  Flow has it attached. Trying to delete results in 'The process cannot access the file '...' because it is being used by another process'
      // System.IO.File.Delete(file.path);

      log.LogInformation(
        $"UpdateImportedFileV4. Completed succesfully. Response: {JsonConvert.SerializeObject(importedFile)}");

      return importedFile;
    }

    // DELETE: api/v4/importedfile
    /// <summary>
    /// Delete imported file
    /// </summary>
    /// <remarks>Deletes existing imported file</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v4/importedfile")]
    [HttpDelete]
    public async Task<ContractExecutionResult> DeleteImportedFileV4([FromUri] Guid projectUid,
      [FromQuery] Guid importedFileUid)
    {
      log.LogInformation($"DeleteImportedFileV4. projectUid {projectUid} importedFileUid: {importedFileUid}");

      await ValidateProjectId(projectUid.ToString());

      var importedFiles = await ImportedFileRequestHelper.GetImportedFiles(projectUid.ToString(), log, projectRepo).ConfigureAwait(false);
      ImportedFile importedFile = null;
      if (importedFiles.Count > 0)
        importedFile = importedFiles.FirstOrDefault(f => f.ImportedFileUid == importedFileUid.ToString());
      if (importedFile == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 56);
      }

      // DB change must be made before raptorProxy.DeleteFile is called as it calls back here to get list of Active files
      var deleteImportedFileEvent = await ImportedFileRequestHelper.DeleteImportedFileInDb(projectUid, importedFileUid,serviceExceptionHandler, projectRepo, false).ConfigureAwait(false);

      await NotifyRaptorDeleteFile(projectUid, importedFile.ImportedFileType, Guid.Parse(importedFile.ImportedFileUid), importedFile.FileDescriptor, importedFile.ImportedFileId, importedFile.LegacyImportedFileId)
        .ConfigureAwait(false);

      await DeleteFileFromTCCRepository(JsonConvert.DeserializeObject<FileDescriptor>(importedFile.FileDescriptor), projectUid, importedFileUid)
        .ConfigureAwait(false);
      
      var messagePayload = JsonConvert.SerializeObject(new { DeleteImportedFileEvent = deleteImportedFileEvent });
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>
        {
          new KeyValuePair<string, string>(deleteImportedFileEvent.ImportedFileUID.ToString(), messagePayload)
        });
      log.LogInformation(
        $"DeleteImportedFileV4. Completed succesfully. projectUid {projectUid} importedFileUid: {importedFileUid}");
      return new ContractExecutionResult();
    }
  }
}