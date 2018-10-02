using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.ConfigurationStore;
using VSS.FlowJSHandler;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Project.WebAPI.Controllers.Filters;
using VSS.MasterData.Project.WebAPI.Factories;
using VSS.MasterData.Project.WebAPI.Internal;
using VSS.MasterData.Project.WebAPI.Internal.Extensions;
using VSS.MasterData.Proxies;
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
      IProjectRepository projectRepo, ISubscriptionRepository subscriptionRepo,
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
    [Route("api/v4/importedfile")]
    [HttpGet]
    public ActionResult Upload()
    {
      return new NoContentResult();
    }

    /// <summary>
    /// Upload a file, and do processing synchronously
    /// </summary>
    /// <param name="file"></param>
    /// <param name="projectUid"></param>
    /// <param name="importedFileType"></param>
    /// <param name="dxfUnitsType"></param>
    /// <param name="fileCreatedUtc"></param>
    /// <param name="fileUpdatedUtc"></param>
    /// <param name="surveyedUtc"></param>
    /// <param name="transferProxy"></param>
    /// <returns></returns>
    [Route("api/v4/importedfile")]
    [HttpPost]
    [FlowUpload(Extensions = new[]
    {
      "svl", "dxf", "ttm"
    }, Size = 1000000000)]
    public async Task<ImportedFileDescriptorSingleResult> SyncUpload(FlowFile file,
      [FromQuery] Guid projectUid,
      [FromQuery] ImportedFileType importedFileType,
      [FromQuery] DxfUnitsType dxfUnitsType,
      [FromQuery] DateTime fileCreatedUtc,
      [FromQuery] DateTime fileUpdatedUtc,
      [FromQuery] DateTime? surveyedUtc,
      [FromServices] ITransferProxy transferProxy)
    {
      // Validate the file
      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, 
        projectUid, 
        importedFileType, 
        dxfUnitsType,
        fileCreatedUtc,
        fileUpdatedUtc, userEmailAddress, surveyedUtc);

      log.LogInformation($"SyncUploadV4. file: {file.flowFilename} path {file.path} projectUid {projectUid.ToString()} ImportedFileType: {importedFileType} " +
        $"DxfUnitsType: {dxfUnitsType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}");

      using (var fileStream = System.IO.File.Open(file.path, FileMode.Open, FileAccess.Read))
      {
        return await ImportFile(file.flowFilename, fileStream, projectUid, importedFileType, dxfUnitsType,
          fileCreatedUtc, fileUpdatedUtc, surveyedUtc);
      }
    }

    /// <summary>
    /// POST or PUT Request to upload a file using a background task
    /// </summary>
    /// <returns>Schedule Job Result with a Job ID</returns>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v4/importedfile/background")]
    [HttpPost]
    [HttpPut]
    [FlowUpload(Extensions = new[]
    {
      "svl", "dxf", "ttm"
    }, Size = 1000000000)]
    public async Task<ScheduleJobResult> BackgroundUpload(
      FlowFile file,
      [FromQuery] Guid projectUid,
      [FromQuery] ImportedFileType importedFileType,
      [FromQuery] DxfUnitsType dxfUnitsType,
      [FromQuery] DateTime fileCreatedUtc,
      [FromQuery] DateTime fileUpdatedUtc,
      [FromQuery] DateTime? surveyedUtc,
      [FromServices] ISchedulerProxy scheduler,
      [FromServices] ITransferProxy transferProxy)
    {
      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType, dxfUnitsType, fileCreatedUtc,
        fileUpdatedUtc, userEmailAddress, surveyedUtc);
      log.LogInformation(
        $"BackgroundUploadV4. file: {file.flowFilename} path {file.path} projectUid {projectUid.ToString()} ImportedFileType: {importedFileType} " +
        $"DxfUnitsType: {dxfUnitsType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}");
      
      var s3Path = $"project/importedfile/{Guid.NewGuid()}.dat";
      var fileStream = System.IO.File.Open(file.path, FileMode.Open, FileAccess.Read);
      transferProxy.Upload(fileStream, s3Path);

      var baseUrl = configStore.GetValueString("PROJECT_INTERNAL_BASE_URL");
      // The QueryString will have values in it, so it's safe to add extra queries with the & as opposed to ?, then &
      var callbackUrl = $"{baseUrl}/internal/v4/importedfile{Request.QueryString}";
      callbackUrl += $"&filename={WebUtility.UrlEncode(file.flowFilename)}";
      callbackUrl += $"&awsFilePath={WebUtility.UrlEncode(s3Path)}";

      var request = new ScheduleJobRequest
      {
        Filename = file.flowFilename,
        Method = "GET", // match the internal upload Method
        Url = callbackUrl,
      };
      request.SetStringPayload(string.Empty);

      var headers = Request.Headers.GetCustomHeaders(true);

      return await scheduler.ScheduleBackgroundJob(request, headers);
    }

    /// <summary>
    /// Import a design file
    ///    this creates a link between the file and project in database,
    ///      sends file to TCC
    ///      and notifies RaptorWebApi
    /// </summary>
    /// <param name="filename">The filename for the file being uploaded</param>
    /// <param name="awsFilePath">The location of the file in AWS</param>
    /// <param name="projectUid"></param>
    /// <param name="importedFileType"></param>
    /// <param name="dxfUnitsType">A DXF file units type</param>
    /// <param name="fileUpdatedUtc"></param>
    /// <param name="fileCreatedUtc"></param>
    /// <param name="surveyedUtc"></param>
    /// <remarks>Import a design file for a project, once the file has been uploaded to AWS</remarks>
    [Route("internal/v4/importedfile")]
    [HttpGet]
    public async Task<ImportedFileDescriptorSingleResult> CreateImportedFileV4(
      [FromQuery] string filename,
      [FromQuery] string awsFilePath,
      [FromQuery] Guid projectUid,
      [FromQuery] ImportedFileType importedFileType,
      [FromQuery] DxfUnitsType dxfUnitsType,
      [FromQuery] DateTime fileCreatedUtc,
      [FromQuery] DateTime fileUpdatedUtc,
      [FromQuery] DateTime? surveyedUtc,
      [FromServices] ITransferProxy transferProxy)
    {

      log.LogInformation(
        $"CreateImportedFileV4. filename: {filename} awspath {awsFilePath} projectUid {projectUid.ToString()} ImportedFileType: {importedFileType} " +
        $"DxfUnitsType: {dxfUnitsType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}");

      // Retrieve the stored file from AWS
      var fileResult = await transferProxy.Download(awsFilePath);
      if (fileResult == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 55);
      }

      using (var ms = new MemoryStream())
      {
        // Depending on the size of the file in S3, the stream returned may or may not support seeking
        // Which we need to TCC to know the length of the file (can't find the length, if you can't seek).
        // To solve this, we have to download the entire stream here and copy to memory.
        // Allowing TCC to upload the file.
        // Not the best solution for extra large files, but TCC doesn't support uploading without file size AFAIK
        fileResult.FileStream.CopyTo(ms);
        return await ImportFile(filename, ms, projectUid, importedFileType, dxfUnitsType,
          fileCreatedUtc, fileUpdatedUtc, surveyedUtc);
      }
    }

    /// <summary>
    /// Upsert imported file
    ///   this creates/updates database AND creates/updates file in TCC.
    ///   notify RaptorWebAPI.
    /// </summary>
    [Route("api/v4/importedfile")]
    [Route("internal/v4/importedfile")]
    [HttpPut]
    [ActionName("Upload")]
    [FlowUpload(Extensions = new[]
    {
      "svl", "dxf", "ttm"
    }, Size = 1_000_000_000)]

    public async Task<ImportedFileDescriptorSingleResult> UpsertImportedFileV4(
      FlowFile file,
      [FromQuery] Guid projectUid,
      [FromQuery] ImportedFileType importedFileType,
      [FromQuery] DxfUnitsType dxfUnitsType,
      [FromQuery] DateTime fileCreatedUtc,
      [FromQuery] DateTime fileUpdatedUtc,
      [FromQuery] DateTime? surveyedUtc = null)
    {
      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType, dxfUnitsType, fileCreatedUtc,
        fileUpdatedUtc, userEmailAddress, surveyedUtc);
      log.LogInformation(
        $"UpdateImportedFileV4. file: {JsonConvert.SerializeObject(file)} projectUid {projectUid} ImportedFileType: {importedFileType} DxfUnitsType: {dxfUnitsType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}");

      return await ProcessFile(file.path, projectUid.ToString(), importedFileType, dxfUnitsType, fileCreatedUtc, fileUpdatedUtc, surveyedUtc);
    }

    /// <summary>
    /// Upsert imported file and create or update the database and do the necessary create or update in TCC.
    /// Also notify RaptorWebAPI of the change.
    /// </summary>
    /// <remarks>
    /// Intended for use by 3rd party connected systems that wish to avoid using FlowJS file upload framework.
    /// </remarks>
    [Route("api/v4/importedfile/direct")]
    [HttpPost]
    [DisableFormValueModelBinding]
    [RequestSizeLimit(1_000_000_000)]
    public async Task<ImportedFileDescriptorSingleResult> CreateImportedFileDirectV4(
      Guid projectUid,
      string filename,
      ImportedFileType importedFileType,
      DxfUnitsType dxfUnitsType,
      DateTime fileCreatedUtc,
      DateTime fileUpdatedUtc,
      DateTime? surveyedUtc = null)
    {
      FileImportDataValidator.ValidateUpsertImportedFileRequest(projectUid, importedFileType, dxfUnitsType, fileCreatedUtc,
        fileUpdatedUtc, userEmailAddress, surveyedUtc, filename);

      log.LogInformation(
        $"{nameof(CreateImportedFileDirectV4)}: ProjectUid: `{projectUid}`, Filename: `{filename}` ImportedFileType: `{importedFileType}`, DxfUnitsType: `{dxfUnitsType}`, SurveyedUTC: `{(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}`");

      ValidateFileDoesNotExist(projectUid.ToString(), filename, importedFileType, surveyedUtc);

      if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 58, $"Expected a multipart request, but got '{Request.ContentType}'");
      }

      var targetFilePath = await HttpContext.Request.StreamFile(filename, log);

      var result = await ProcessFile(targetFilePath, projectUid.ToString(), importedFileType, dxfUnitsType, fileCreatedUtc, fileUpdatedUtc, surveyedUtc);

      return result;
    }

    /// <summary>
    /// Delete imported file
    /// </summary>
    /// <remarks>Deletes existing imported file</remarks>
    [Route("api/v4/importedfile")]
    [HttpDelete]
    public async Task<ContractExecutionResult> DeleteImportedFileV4([FromQuery] Guid projectUid,
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
      var deleteImportedFileEvent = await ImportedFileRequestHelper.DeleteImportedFileInDb(projectUid, importedFileUid, serviceExceptionHandler, projectRepo, false).ConfigureAwait(false);

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

    /// <summary>
    /// Common file processing method used by all importedFile endpoints.
    /// </summary>
    private async Task<ImportedFileDescriptorSingleResult> ProcessFile(
      string filePath,
      string projectUid,
      ImportedFileType importedFileType,
      DxfUnitsType dxfUnitsType,
      DateTime fileCreatedUtc,
      DateTime fileUpdatedUtc,
      DateTime? surveyedUtc = null)
    {
      if (!System.IO.File.Exists(filePath))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 55);
      }

      // this also validates that this customer has access to the projectUid
      var project = await ProjectRequestHelper.GetProject(projectUid, customerUid, log, serviceExceptionHandler, projectRepo);

      FileDescriptor fileDescriptor;
      using (var fileStream = new FileStream(filePath, FileMode.Open))
      {
        fileDescriptor = await ProjectRequestHelper.WriteFileToTCCRepository(
                                                     fileStream, customerUid, projectUid, filePath, importedFileType == ImportedFileType.SurveyedSurface,
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
            raptorProxy, null,
            projectRepo, null, fileRepo)
          .ProcessAsync(importedFileUpsertEvent)
      ) as ImportedFileDescriptorSingleResult;

      //  Flow has it attached. Trying to delete results in 'The process cannot access the file '...' because it is being used by another process'
      // System.IO.File.Delete(file.path);

      log.LogInformation(
        $"UpdateImportedFileV4. Completed succesfully. Response: {JsonConvert.SerializeObject(importedFile)}");

      return importedFile;
    }


    /// <summary>
    /// Do the import of a file uploaded directly, or via scheduler 
    /// This can be called by the background upload (file stored in AWS, then re downloaded with scheduler request), or Synchronise upload (file stored locally)
    /// </summary>
    /// <param name="filename">filename if the file</param>
    /// <param name="fileStream">Stream containing the contents of the file</param>
    /// <param name="projectUid"></param>
    /// <param name="importedFileType"></param>
    /// <param name="dxfUnitsType"></param>
    /// <param name="fileCreatedUtc"></param>
    /// <param name="fileUpdatedUtc"></param>
    /// <param name="surveyedUtc"></param>
    /// <returns>Details of the upload file</returns>
    private async Task<ImportedFileDescriptorSingleResult> ImportFile(string filename, Stream fileStream, Guid projectUid, ImportedFileType importedFileType,
      DxfUnitsType dxfUnitsType, DateTime fileCreatedUtc, DateTime fileUpdatedUtc, DateTime? surveyedUtc)
    {
      var importedFileList = await ImportedFileRequestHelper
        .GetImportedFileList(projectUid.ToString(), log, userId, projectRepo).ConfigureAwait(false);
      ImportedFileDescriptor importedFileDescriptor = null;
      if (importedFileList.Count > 0)
        importedFileDescriptor = importedFileList.FirstOrDefault(
          f => string.Equals(f.Name, filename, StringComparison.OrdinalIgnoreCase)
               && f.ImportedFileType == importedFileType
               && (importedFileType != ImportedFileType.SurveyedSurface || f.SurveyedUtc == surveyedUtc));

      if (importedFileDescriptor != null)
      {
        var message = $"CreateImportedFileV4. File: {filename} has already been imported.";
        log.LogError(message);
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 58);
      }

      /*** now making changes, potentially needing rollback ***/
      FileDescriptor fileDescriptor = null;


      fileDescriptor = await ProjectRequestHelper.WriteFileToTCCRepository(
          fileStream, customerUid, projectUid.ToString(), filename, importedFileType == ImportedFileType.SurveyedSurface,
          surveyedUtc, fileSpaceId, log, serviceExceptionHandler, fileRepo)
        .ConfigureAwait(false);
      

      // need to write to Db prior to notifying raptor, as raptor needs the legacyImportedFileID 
      var createImportedFileEvent = await ImportedFileRequestHelper.CreateImportedFileinDb(Guid.Parse(customerUid),
          projectUid,
          importedFileType, dxfUnitsType, filename, surveyedUtc, JsonConvert.SerializeObject(fileDescriptor),
          fileCreatedUtc, fileUpdatedUtc, userEmailAddress,
          log, serviceExceptionHandler, projectRepo)
        .ConfigureAwait(false);

      var project =
        await ProjectRequestHelper.GetProject(projectUid.ToString(), customerUid, log, serviceExceptionHandler,
          projectRepo);

      var result = await ImportedFileRequestHelper.NotifyRaptorAddFile(project.LegacyProjectID, projectUid,
        importedFileType, dxfUnitsType, fileDescriptor,
        createImportedFileEvent.ImportedFileID, createImportedFileEvent.ImportedFileUID, true,
        log, customHeaders, serviceExceptionHandler, raptorProxy, projectRepo).ConfigureAwait(false);
      createImportedFileEvent.MinZoomLevel = result.MinZoomLevel;
      createImportedFileEvent.MaxZoomLevel = result.MaxZoomLevel;
      var existing = await projectRepo.GetImportedFile(createImportedFileEvent.ImportedFileUID.ToString())
        .ConfigureAwait(false);
      //Need to update zoom levels in Db 
      _ = await ImportedFileRequestHelper.UpdateImportedFileInDb(existing, JsonConvert.SerializeObject(fileDescriptor),
          surveyedUtc, result.MinZoomLevel, result.MaxZoomLevel,
          fileCreatedUtc, fileUpdatedUtc, userEmailAddress,
          log, serviceExceptionHandler, projectRepo)
        .ConfigureAwait(false);

      var messagePayload = JsonConvert.SerializeObject(new {CreateImportedFileEvent = createImportedFileEvent});
      producer.Send(kafkaTopicName,
        new List<KeyValuePair<string, string>>
        {
          new KeyValuePair<string, string>(createImportedFileEvent.ImportedFileUID.ToString(), messagePayload)
        });

      // Flow has it attached. Trying to delete results in 'The process cannot access the file '...' because it is being used by another process'
      //System.IO.File.Delete(file.path);

      var importedFile = new ImportedFileDescriptorSingleResult(
        (await ImportedFileRequestHelper.GetImportedFileList(projectUid.ToString(), log, userId, projectRepo)
          .ConfigureAwait(false))
        .ToImmutableList()
        .First(f => f.ImportedFileUid == createImportedFileEvent.ImportedFileUID.ToString())
      );
      log.LogInformation(
        $"CreateImportedFileV4. completed successfully. Response: {JsonConvert.SerializeObject(importedFile)}");
      return importedFile;
    }


    /// <summary>
    /// Validate that the uploaded file doesn't already exist in the database.
    /// Should only be called from create methods where there's an expectation the file isn't already present.
    /// </summary>
    private void ValidateFileDoesNotExist(string projectUid, string filename, ImportedFileType importedFileType, DateTime? surveyedUtc)
    {
      var importedFileList = ImportedFileRequestHelper.GetImportedFileList(projectUid, log, userId, projectRepo)
                                                      .ConfigureAwait(false)
                                                      .GetAwaiter()
                                                      .GetResult();

      ImportedFileDescriptor importedFileDescriptor = null;
      if (importedFileList.Count > 0)
        importedFileDescriptor = importedFileList.FirstOrDefault(
          f => string.Equals(f.Name, filename, StringComparison.OrdinalIgnoreCase)
               && f.ImportedFileType == importedFileType
               && (importedFileType != ImportedFileType.SurveyedSurface || f.SurveyedUtc == surveyedUtc));

      if (importedFileDescriptor != null)
      {
        var message = $"CreateImportedFileV4. File: {filename} has already been imported.";
        log.LogError(message);
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 58);
      }
    }
  }
}
