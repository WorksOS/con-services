using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.DataOcean.Client;
using VSS.FlowJSHandler;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
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
using VSS.Pegasus.Client;
using VSS.Productivity.Push.Models.Notifications.Changes;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Project.Abstractions.Extensions;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;
using VSS.TCCFileAccess;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// File Import controller v4
  /// </summary>
  public class FileImportV4Controller : FileImportBaseController
  {
    private readonly INotificationHubClient notificationHubClient;

    /// <summary>
    /// File import controller v4
    /// </summary>
    public FileImportV4Controller(IKafka producer,
      IConfigurationStore store, ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler,
      IRaptorProxy raptorProxy, Func<TransferProxyType, ITransferProxy> persistantTransferProxy,
      IFilterServiceProxy filterServiceProxy, ITRexImportFileProxy tRexImportFileProxy,
      IProjectRepository projectRepo, ISubscriptionRepository subscriptionRepo,
      IFileRepository fileRepo, IRequestFactory requestFactory, IDataOceanClient dataOceanClient,
      ITPaaSApplicationAuthentication authn, INotificationHubClient notificationHubClient)
      : base(producer, store, loggerFactory, serviceExceptionHandler,
        raptorProxy, persistantTransferProxy, filterServiceProxy, tRexImportFileProxy,
        projectRepo, subscriptionRepo, fileRepo, requestFactory, dataOceanClient, authn)
    {
      this.notificationHubClient = notificationHubClient;
    }

    /// <summary>
    /// Gets a list of imported files for a project. The list includes files of all types.
    /// </summary>
    [Route("api/v4/importedfiles")]
    [HttpGet]
    public async Task<ImportedFileDescriptorListResult> GetImportedFilesV4([FromQuery] string projectUid)
    {
      logger.LogInformation("GetImportedFilesV4");

      return new ImportedFileDescriptorListResult
      {
        ImportedFileDescriptors = await ImportedFileRequestDatabaseHelper.GetImportedFileList(projectUid, logger, userId, projectRepo).ConfigureAwait(false)
      };
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
    [Route("api/v4/importedfile")]
    [HttpPost]
    [FlowUpload(Extensions = new[]
    {
      "svl", "dxf", "ttm"
    }, Size = 1000000000)]
    public async Task<ImportedFileDescriptorSingleResult> SyncUpload(
      [FromServices] ISchedulerProxy schedulerProxy,
      FlowFile file,
      [FromQuery] Guid projectUid,
      [FromQuery] ImportedFileType importedFileType,
      [FromQuery] DxfUnitsType dxfUnitsType,
      [FromQuery] DateTime fileCreatedUtc,
      [FromQuery] DateTime fileUpdatedUtc,
      [FromQuery] DateTime? surveyedUtc
    )
    {
      if (importedFileType == ImportedFileType.ReferenceSurface)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 122);
      }

      // Validate the file
      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(
        file, projectUid, importedFileType, dxfUnitsType, fileCreatedUtc, fileUpdatedUtc, userEmailAddress, surveyedUtc, null, null);

      ImportedFileUtils.ValidateEnvironmentVariables(importedFileType, configStore, serviceExceptionHandler);
      logger.LogInformation(
        $"SyncUploadV4. file: {file.flowFilename} path {file.path} projectUid {projectUid.ToString()} ImportedFileType: {importedFileType} " +
        $"DxfUnitsType: {dxfUnitsType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}");

      ValidateFileDoesNotExist(projectUid.ToString(), file.flowFilename, importedFileType, surveyedUtc, null, null);

      ContractExecutionResult importedFileResult;

      using (var fileStream = System.IO.File.Open(file.path, FileMode.Open, FileAccess.Read))
      {
        importedFileResult = await UpsertFileInternal(file.flowFilename, fileStream, projectUid, importedFileType, dxfUnitsType,
          fileCreatedUtc, fileUpdatedUtc, surveyedUtc, schedulerProxy);
      }

      logger.LogInformation(
        $"SyncUploadV4. Completed successfully. Response: {JsonConvert.SerializeObject(importedFileResult)}");

      return importedFileResult as ImportedFileDescriptorSingleResult;
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
      [FromServices] Func<TransferProxyType, ITransferProxy> transferProxyFunc)
    {
      if (importedFileType == ImportedFileType.ReferenceSurface)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 122);
      }

      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(
        file, projectUid, importedFileType, dxfUnitsType, fileCreatedUtc, fileUpdatedUtc, userEmailAddress, surveyedUtc, null, null);
      logger.LogInformation(
        $"BackgroundUploadV4. file: {file.flowFilename} path {file.path} projectUid {projectUid.ToString()} ImportedFileType: {importedFileType} " +
        $"DxfUnitsType: {dxfUnitsType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}");

      if (string.Equals(Request.Method, HttpMethod.Post.ToString(), StringComparison.OrdinalIgnoreCase))
      {
        ValidateFileDoesNotExist(projectUid.ToString(), file.flowFilename, importedFileType, surveyedUtc, null, null);
      }

      var s3Path = $"project/importedfile/{Guid.NewGuid()}.dat";
      var fileStream = System.IO.File.Open(file.path, FileMode.Open, FileAccess.Read);
      var transferProxy = transferProxyFunc(TransferProxyType.Default);
      transferProxy.Upload(fileStream, s3Path);

      var baseUrl = configStore.GetValueString("PROJECT_INTERNAL_BASE_URL");
      // The QueryString will have values in it, so it's safe to add extra queries with the & as opposed to ?, then &
      var callbackUrl = $"{baseUrl}/internal/v4/importedfile{Request.QueryString}";
      callbackUrl += $"&filename={WebUtility.UrlEncode(file.flowFilename)}&awsFilePath={WebUtility.UrlEncode(s3Path)}";

      var executionTimeout = configStore.GetValueInt("PEGASUS_EXECUTION_TIMEOUT_MINS", 5) * 60000;//minutes converted to millisecs
      var request = new ScheduleJobRequest
      {
        Filename = file.flowFilename,
        Method = "GET", // match the internal upload Method
        Url = callbackUrl,
        Timeout = executionTimeout
      };
      request.SetStringPayload(string.Empty);

      var headers = Request.Headers.GetCustomHeaders();

      return await scheduler.ScheduleBackgroundJob(request, headers);
    }

    /// <summary>
    /// Do the import of a file via scheduler 
    /// This can be called by the background upload (file stored in AWS, then re downloaded with scheduler request)
    /// </summary>
    /// <param name="filename">The filename for the file being uploaded</param>
    /// <param name="awsFilePath">The location of the file in AWS</param>
    /// <param name="projectUid"></param>
    /// <param name="importedFileType"></param>
    /// <param name="dxfUnitsType">A DXF file units type</param>
    /// <param name="fileUpdatedUtc"></param>
    /// <param name="fileCreatedUtc"></param>
    /// <param name="surveyedUtc"></param>
    /// <param name="transferProxyFunc"></param>
    /// <param name="schedulerProxy"></param>
    /// <remarks>Import a design file for a project, once the file has been uploaded to AWS</remarks>
    [Route("internal/v4/importedfile")]
    [HttpGet]
    public async Task<ImportedFileDescriptorSingleResult> InternalImportedFileV4(

      [FromQuery] string filename,
      [FromQuery] string awsFilePath,
      [FromQuery] Guid projectUid,
      [FromQuery] ImportedFileType importedFileType,
      [FromQuery] DxfUnitsType dxfUnitsType,
      [FromQuery] DateTime fileCreatedUtc,
      [FromQuery] DateTime fileUpdatedUtc,
      [FromQuery] DateTime? surveyedUtc,
      [FromServices] Func<TransferProxyType, ITransferProxy> transferProxyFunc,
      [FromServices] ISchedulerProxy schedulerProxy)
    {
      if (importedFileType == ImportedFileType.ReferenceSurface)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 122);
      }

      ImportedFileUtils.ValidateEnvironmentVariables(importedFileType, configStore, serviceExceptionHandler);

      ImportedFileDescriptorSingleResult importedFileResult = null;
      var transferProxy = transferProxyFunc(TransferProxyType.Default);
      logger.LogInformation(
        $"InternalImportedFileV4. filename: {filename} awspath {awsFilePath} projectUid {projectUid.ToString()} ImportedFileType: {importedFileType} " +
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

        importedFileResult = await UpsertFileInternal(filename, ms, projectUid, importedFileType, dxfUnitsType,
          fileCreatedUtc, fileUpdatedUtc, surveyedUtc, schedulerProxy);
      }

      logger.LogInformation(
          $"InternalImportedFileV4. Completed successfully. Response: {JsonConvert.SerializeObject(importedFileResult)}");

      return importedFileResult;
    }


    /// <summary>
    /// Upsert imported file
    ///   this creates/updates database AND creates/updates file in TCC.
    ///   notify RaptorWebAPI.
    /// </summary>
    // I don't believe this endpoint is used anymore
    [Route("api/v4/importedfile")]
    [Route("internal/v4/importedfile")]
    [HttpPut]
    [ActionName("Upload")]
    [FlowUpload(Extensions = new[]
    {
      "svl", "dxf", "ttm"
    }, Size = 1_000_000_000)]

    public Task<ImportedFileDescriptorSingleResult> UpsertImportedFileV4(
      [FromServices] ISchedulerProxy schedulerProxy,
      FlowFile file,
      [FromQuery] Guid projectUid,
      [FromQuery] ImportedFileType importedFileType,
      [FromQuery] DxfUnitsType dxfUnitsType,
      [FromQuery] DateTime fileCreatedUtc,
      [FromQuery] DateTime fileUpdatedUtc,
      [FromQuery] DateTime? surveyedUtc = null,
      [FromQuery] bool uploadToTcc = true)
    {
      if (importedFileType == ImportedFileType.ReferenceSurface)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 122);
      }

      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType, dxfUnitsType, fileCreatedUtc,
        fileUpdatedUtc, userEmailAddress, surveyedUtc, null, null);

      ImportedFileUtils.ValidateEnvironmentVariables(importedFileType, configStore, serviceExceptionHandler);

      logger.LogInformation(
        $"{nameof(UpsertImportedFileV4)}. file: {JsonConvert.SerializeObject(file)} projectUid {projectUid} ImportedFileType: {importedFileType} DxfUnitsType: {dxfUnitsType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}");
      
      return UpsertFile(file.path, projectUid.ToString(), importedFileType, dxfUnitsType, fileCreatedUtc, fileUpdatedUtc, surveyedUtc, schedulerProxy, uploadToTcc);
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
      [FromServices] ISchedulerProxy schedulerProxy,
      Guid projectUid,
      string filename,
      ImportedFileType importedFileType,
      DxfUnitsType dxfUnitsType,
      DateTime fileCreatedUtc,
      DateTime fileUpdatedUtc,
      DateTime? surveyedUtc = null)
    {
      if (importedFileType == ImportedFileType.ReferenceSurface)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 122);
      }

      FileImportDataValidator.ValidateUpsertImportedFileRequest(projectUid, importedFileType, dxfUnitsType, fileCreatedUtc,
        fileUpdatedUtc, userEmailAddress, surveyedUtc, filename, null, null);
      ImportedFileUtils.ValidateEnvironmentVariables(importedFileType, configStore, serviceExceptionHandler);
      logger.LogInformation(
        $"{nameof(CreateImportedFileDirectV4)}: ProjectUid: `{projectUid}`, Filename: `{filename}` ImportedFileType: `{importedFileType}`, DxfUnitsType: `{dxfUnitsType}`, SurveyedUTC: `{(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}`");

      //When debugging locally using Postman, remove this check so can do an update
      ValidateFileDoesNotExist(projectUid.ToString(), filename, importedFileType, surveyedUtc, null, null);

      if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 58, $"Expected a multipart request, but got '{Request.ContentType}'");
      }

      var targetFilePath = await HttpContext.Request.StreamFile(filename, logger);

      var result = await UpsertFile(targetFilePath, projectUid.ToString(), importedFileType, dxfUnitsType, fileCreatedUtc, fileUpdatedUtc, surveyedUtc, schedulerProxy);

      return result;
    }

    /// <summary>
    /// Delete imported file
    ///    For Trex gateway, projectSvc stores the design file in S3 (currently)
    ///      As of this writing, the file will remain there, even after deletion
    /// </summary>
    /// <remarks>Deletes existing imported file</remarks>
    [Route("api/v4/importedfile")]
    [HttpDelete]
    public async Task<ContractExecutionResult> DeleteImportedFileV4(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid importedFileUid,
      [FromServices] IPegasusClient pegasusClient)
    {
      logger.LogInformation($"DeleteImportedFileV4. projectUid {projectUid} importedFileUid: {importedFileUid}");

      await ValidateProjectId(projectUid.ToString());

      var importedFiles = await ImportedFileRequestDatabaseHelper.GetImportedFiles(projectUid.ToString(), logger, projectRepo).ConfigureAwait(false);
      ImportedFile existing = null;
      if (importedFiles.Count > 0)
        existing = importedFiles.FirstOrDefault(f => f.ImportedFileUid == importedFileUid.ToString());

      if (existing == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 56);
        return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "shouldn't get here"); // to keep compiler happy
      }
      ImportedFileUtils.ValidateEnvironmentVariables(existing.ImportedFileType, configStore, serviceExceptionHandler);

      var deleteImportedFile = DeleteImportedFile.CreateDeleteImportedFile(
        projectUid, existing.ImportedFileType, JsonConvert.DeserializeObject<FileDescriptor>(existing.FileDescriptor),
        Guid.Parse(existing.ImportedFileUid), existing.ImportedFileId, existing.LegacyImportedFileId, DataOceanRootFolder);

      var result = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<DeleteImportedFileExecutor>(
            loggerFactory, configStore, serviceExceptionHandler, customerUid, userId, userEmailAddress, customHeaders,
            producer, kafkaTopicName, raptorProxy, null, persistantTransferProxy, filterServiceProxy, tRexImportFileProxy,
            projectRepo, null, fileRepo, null, null, dataOceanClient, authn, null, pegasusClient)
          .ProcessAsync(deleteImportedFile)
      );

      await notificationHubClient.Notify(new ProjectChangedNotification(projectUid));

      logger.LogInformation(
        $"DeleteImportedFileV4. Completed successfully. projectUid {projectUid} importedFileUid: {importedFileUid}");
      return result;
    }

    /// <summary>
    /// Common file processing method used by all importedFile endpoints.
    /// </summary>
    private async Task<ImportedFileDescriptorSingleResult> UpsertFile(
       string filePath,
       string projectUid,
       ImportedFileType importedFileType,
       DxfUnitsType dxfUnitsType,
       DateTime fileCreatedUtc,
       DateTime fileUpdatedUtc,
       DateTime? surveyedUtc,
       ISchedulerProxy schedulerProxy,
       bool uploadToTcc = true)
    {
      if (!System.IO.File.Exists(filePath))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 55);
      }

      using (var fileStream = new FileStream(filePath, FileMode.Open))
      {
        return await UpsertFileInternal(Path.GetFileName(filePath), fileStream, Guid.Parse(projectUid), importedFileType, dxfUnitsType,
          fileCreatedUtc, fileUpdatedUtc, surveyedUtc, schedulerProxy, uploadToTcc: uploadToTcc);
      }
    }

    /// <summary>
    /// Common file processing method used by all importedFile endpoints.
    /// </summary>
    private async Task<ImportedFileDescriptorSingleResult> UpsertFileInternal(
      string filename,
      Stream fileStream,
      Guid projectUid,
      ImportedFileType importedFileType,
      DxfUnitsType dxfUnitsType,
      DateTime fileCreatedUtc,
      DateTime fileUpdatedUtc,
      DateTime? surveyedUtc,
      ISchedulerProxy schedulerProxy,
      Guid? parentUid = null,
      double? offset = null,
      bool uploadToTcc = true)
    {
      var existing = await ImportedFileRequestDatabaseHelper
        .GetImportedFileForProject
          (projectUid.ToString(), filename, importedFileType, surveyedUtc,
           logger, projectRepo, offset, parentUid)
        .ConfigureAwait(false);

      bool creating = existing == null;
      logger.LogInformation(
        creating
          ? $"UpdateImportedFileExecutor. file doesn't exist already in DB: {filename} projectUid {projectUid} ImportedFileType: {importedFileType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())} parentUid {parentUid} offset: {offset}"
          : $"UpdateImportedFileExecutor. file exists already in DB. Will be updated: {JsonConvert.SerializeObject(existing)}");

      ImportedFileDescriptorSingleResult importedFile;

      FileDescriptor fileDescriptor = null;

      if (importedFileType == ImportedFileType.ReferenceSurface)
      {
        //FileDescriptor not used for reference surface but validation requires values
        fileDescriptor = FileDescriptor.CreateFileDescriptor("Not applicable", "Not applicable", filename);
      }
      else
      {
        if (UseTrexGatewayDesignImport && IsDesignFileType(importedFileType))
        {
          fileDescriptor = ProjectRequestHelper.WriteFileToS3Repository(
            fileStream, projectUid.ToString(), filename,
            importedFileType == ImportedFileType.SurveyedSurface, surveyedUtc,
            logger, serviceExceptionHandler, persistantTransferProxy);
        }

        if (UseRaptorGatewayDesignImport)
        {
          if (uploadToTcc)
          {
            fileDescriptor = await TccHelper.WriteFileToTCCRepository(
                fileStream, customerUid, projectUid.ToString(), filename,
                importedFileType == ImportedFileType.SurveyedSurface,
                surveyedUtc, FileSpaceId, logger, serviceExceptionHandler, fileRepo)
              .ConfigureAwait(false);
          }

          //save copy to DataOcean      
          await DataOceanHelper.WriteFileToDataOcean(
              fileStream, DataOceanRootFolder, customerUid, projectUid.ToString(), filename,
              importedFileType == ImportedFileType.SurveyedSurface,
              surveyedUtc, logger, serviceExceptionHandler, dataOceanClient, authn)
            .ConfigureAwait(false);
        }
      }

      if (creating)
      {
        var createImportedFile = CreateImportedFile.Create(
          projectUid, filename, fileDescriptor, importedFileType, surveyedUtc, dxfUnitsType, fileCreatedUtc, fileUpdatedUtc, DataOceanRootFolder, parentUid, offset);

        importedFile = await WithServiceExceptionTryExecuteAsync(() =>
          RequestExecutorContainerFactory
            .Build<CreateImportedFileExecutor>(
              loggerFactory, configStore, serviceExceptionHandler, customerUid, userId, userEmailAddress, customHeaders,
              producer, kafkaTopicName, raptorProxy, null, persistantTransferProxy, null, tRexImportFileProxy,
              projectRepo, null, fileRepo, null, null, dataOceanClient, authn, schedulerProxy)
            .ProcessAsync(createImportedFile)
        ) as ImportedFileDescriptorSingleResult;

        logger.LogInformation(
          $"UpdateImportedFileV4. Create completed successfully. Response: {JsonConvert.SerializeObject(importedFile)}");
      }
      else
      {
        // this also validates that this customer has access to the projectUid
        var project = await ProjectRequestHelper.GetProject(projectUid.ToString(), customerUid, logger, serviceExceptionHandler, projectRepo);

        var importedFileUpsertEvent = UpdateImportedFile.Create(
          projectUid, project.LegacyProjectID, importedFileType,
          importedFileType == ImportedFileType.SurveyedSurface
            ? surveyedUtc
            : null,
          dxfUnitsType, fileCreatedUtc, fileUpdatedUtc, fileDescriptor,
          Guid.Parse(existing?.ImportedFileUid), existing.ImportedFileId,
          DataOceanRootFolder, offset
        );

        importedFile = await WithServiceExceptionTryExecuteAsync(() =>
          RequestExecutorContainerFactory
            .Build<UpdateImportedFileExecutor>(
              loggerFactory, configStore, serviceExceptionHandler, customerUid, userId, userEmailAddress, customHeaders,
              producer, kafkaTopicName, raptorProxy, null, null, null, tRexImportFileProxy,
              projectRepo, null, fileRepo, null, null, dataOceanClient, authn, schedulerProxy)
            .ProcessAsync(importedFileUpsertEvent)
        ) as ImportedFileDescriptorSingleResult;

        logger.LogInformation(
          $"UpdateImportedFileV4. Update completed successfully. Response: {JsonConvert.SerializeObject(importedFile)}");
      }

      await notificationHubClient.Notify(new ProjectChangedNotification(projectUid));

      return importedFile;
    }


    /// <summary>
    /// Validate that the uploaded file doesn't already exist in the database.
    /// Should only be called from create methods where there's an expectation the file isn't already present.
    /// </summary>
    private void ValidateFileDoesNotExist(string projectUid, string filename, ImportedFileType importedFileType, DateTime? surveyedUtc, Guid? parentUid, double? offset)
    {
      var importedFileList = ImportedFileRequestDatabaseHelper.GetImportedFileList(projectUid, logger, userId, projectRepo)
        .ConfigureAwait(false)
        .GetAwaiter()
        .GetResult();

      ImportedFileDescriptor importedFileDescriptor = null;
      if (importedFileList.Count > 0)
      {
        if (importedFileType == ImportedFileType.ReferenceSurface)
        {
          importedFileDescriptor = importedFileList.FirstOrDefault(
            f => f.ImportedFileType == ImportedFileType.ReferenceSurface &&
                 f.ParentUid == parentUid && f.Offset.EqualsToNearestMillimeter(offset));
        }
        else
        {
          importedFileDescriptor = importedFileList.FirstOrDefault(
            f => string.Equals(f.Name, filename, StringComparison.OrdinalIgnoreCase)
                 && f.ImportedFileType == importedFileType
                 && (importedFileType != ImportedFileType.SurveyedSurface || f.SurveyedUtc == surveyedUtc));
        }
      }

      if (importedFileDescriptor != null)
      {
        var message = $"CreateImportedFileDirectV4. File: {filename} has already been imported.";
        logger.LogError(message);
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, importedFileType == ImportedFileType.ReferenceSurface ? 121 : 58);
      }
    }

    /// <summary>
    /// Import a reference surface
    /// </summary>
    [Route("api/v4/importedfile/referencesurface")]
    [HttpPost]
    public async Task<ContractExecutionResult> CreateReferenceSurface(
      [FromQuery] Guid projectUid,
      [FromQuery] string filename,
      [FromQuery] DateTime fileCreatedUtc,
      [FromQuery] DateTime fileUpdatedUtc,
      [FromQuery] Guid parentUid,
      [FromQuery] double offset,
      [FromServices] ISchedulerProxy schedulerProxy,
      [FromServices] IPreferenceProxy prefProxy)
    {
      logger.LogInformation($"CreateReferenceSurface. projectUid {projectUid} filename: {filename} parentUid: {parentUid} offset: {offset}");

      await ValidateProjectId(projectUid.ToString());

      ImportedFileUtils.ValidateEnvironmentVariables(ImportedFileType.ReferenceSurface, configStore, serviceExceptionHandler);

      //Check parent design does exist
      var importedFiles = await ImportedFileRequestDatabaseHelper.GetImportedFileList(projectUid.ToString(), logger, userId, projectRepo);
      var parent = importedFiles.FirstOrDefault(i => i.ImportedFileUid == parentUid.ToString());
      if (parent == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 120);
      }

      //Fill in file name if not provided
      if (string.IsNullOrEmpty(filename))
      {
        filename = await DefaultReferenceSurfaceName(prefProxy, offset, Path.GetFileNameWithoutExtension(parent.Name));
      }

      //Validate parameters
      FileImportDataValidator.ValidateUpsertImportedFileRequest(projectUid, ImportedFileType.ReferenceSurface, DxfUnitsType.Meters, fileCreatedUtc,
        fileUpdatedUtc, userEmailAddress, null, filename, parentUid, offset);

      //Check reference surface does not exist
      ValidateFileDoesNotExist(projectUid.ToString(), filename, ImportedFileType.ReferenceSurface, null, parentUid, offset);

      var importedFileResult = await UpsertFileInternal(filename, null, projectUid, ImportedFileType.ReferenceSurface, DxfUnitsType.Meters,
        fileCreatedUtc, fileUpdatedUtc, null, schedulerProxy, parentUid, offset);

      //If parent design is deactivated then deactivate reference surface
      if (!parent.IsActivated)
      {
        var filesToUpdate = new Dictionary<Guid, bool>();
        filesToUpdate.Add(new Guid(importedFileResult.ImportedFileDescriptor.ImportedFileUid), false);
        await DoActivationAndNotification(projectUid.ToString(), filesToUpdate);
        importedFiles = await ImportedFileRequestDatabaseHelper.GetImportedFileList(projectUid.ToString(), logger, userId, projectRepo);
        importedFileResult.ImportedFileDescriptor = importedFiles.SingleOrDefault(i =>
          i.ImportedFileUid == importedFileResult.ImportedFileDescriptor.ImportedFileUid);
      }

      logger.LogInformation(
        $"CreateReferenceSurface. Completed successfully. Response: {JsonConvert.SerializeObject(importedFileResult)}");

      return importedFileResult;
    }

    /// <summary>
    /// Construct the default reference surface name
    /// e.g. if the parent design file is bob.ttm, the offset 1.5 meters and the user units meters then the reference surface name is "bob +1.5m"
    /// </summary>
    /// <param name="prefProxy"></param>
    /// <param name="offset"></param>
    /// <param name="parentName"></param>
    /// <returns></returns>
    private async Task<string> DefaultReferenceSurfaceName(IPreferenceProxy prefProxy, double offset, string parentName)
    {
      const double ImperialFeetToMetres = 0.3048;
      const double USFeetToMetres = 0.304800609601;

      var displayOffset = offset;
      var unitsString = string.Empty;
      var userPreferences = await prefProxy.GetUserPreferences(Request.Headers.GetCustomHeaders());
      switch (userPreferences.Units.UnitsType())
      {
        case UnitsTypeEnum.Metric:
          displayOffset = offset;
          unitsString = "m";
          break;
        case UnitsTypeEnum.Imperial:
          displayOffset = offset / ImperialFeetToMetres;
          unitsString = "ft";
          break;
        case UnitsTypeEnum.US:
          displayOffset = offset / USFeetToMetres;
          unitsString = "ft";
          break;
      }
      var sign = offset > 0 ? "+" : "-";
      displayOffset = Math.Abs(displayOffset);
      return $"{parentName} {sign}{displayOffset:F3}{unitsString}";
    }

    #region fileActivation

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
      logger.LogInformation("ActivateFiles");

      await ValidateProjectId(projectUid).ConfigureAwait(false);

      if (request == null)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 40);

      var fileIds = string.Join(",", request.ImportedFileDescriptors.Select(x => x.ImportedFileUid));
      if (string.IsNullOrEmpty(fileIds))
      {
        return Ok(new { Code = HttpStatusCode.BadRequest, Message = "Request contains no imported file IDs." });
      }

      logger.LogInformation($"{functionId}. projectUid: {projectUid}, fileUids: {fileIds}");

      var importedFiles = await ImportedFileRequestDatabaseHelper.GetImportedFileList(projectUid, logger, userId, projectRepo).ConfigureAwait(false);
      if (!importedFiles.Any())
      {
        logger.LogInformation($"{functionId}. Attempt to set file activation state when project contains no files");

        return Ok(new { Code = HttpStatusCode.BadRequest, Message = "Project contains no imported files." });
      }

      var filesToUpdate = new Dictionary<Guid, bool>();

      foreach (var activatedFileDescriptor in request.ImportedFileDescriptors)
      {
        var existingFile = importedFiles.FirstOrDefault(f => f.ImportedFileUid == activatedFileDescriptor.ImportedFileUid);
        if (existingFile == null)
        {
          logger.LogError(
            $"{functionId}. File doesn't exist. projectUid {projectUid}, fileUid: {activatedFileDescriptor.ImportedFileUid}");
          continue;
        }

        if (existingFile.ImportedFileType == ImportedFileType.ReferenceSurface)
        {
          logger.LogError(
            $"{functionId}. Attempt to set file activation on a reference surface. projectUid {projectUid}, fileUid: {activatedFileDescriptor.ImportedFileUid}");
          continue;
        }

        if (existingFile.IsActivated == activatedFileDescriptor.IsActivated)
        {
          logger.LogDebug(
            $"{functionId}. File activation state is already set to {existingFile.IsActivated}. No changes required. {existingFile.ImportedFileUid}");
          continue;
        }

        logger.LogInformation(
          $"{functionId}. File queued for updating: {JsonConvert.SerializeObject(existingFile)}");
        filesToUpdate.Add(new Guid(activatedFileDescriptor.ImportedFileUid), activatedFileDescriptor.IsActivated);

        //If user is activating or deactivating a design which has reference surfaces, do as a group
        if (existingFile.ImportedFileType == ImportedFileType.DesignSurface)
        {
          var children = importedFiles
            .Where(f => f.ParentUid.HasValue && f.ParentUid.ToString() == existingFile.ImportedFileUid).ToList();
          if (children.Count > 0)
          {
            logger.LogInformation(
              $"{functionId}. Setting file activation state of reference surfaces for design {existingFile.ImportedFileUid}");
            foreach (var child in children)
            {
              if (child.IsActivated != activatedFileDescriptor.IsActivated)
                filesToUpdate.Add(new Guid(child.ImportedFileUid), activatedFileDescriptor.IsActivated);
            }
          }
        }
      }

      if (!filesToUpdate.Any())
      {
        logger.LogInformation($"{functionId}. No files eligible for activation state change.");

        return Ok(new { Code = HttpStatusCode.OK, Message = "Success" });
      }

      try
      {
        await DoActivationAndNotification(projectUid, filesToUpdate);

        return Ok(new { Code = HttpStatusCode.OK, Message = "Success" });
      }
      catch (Exception exception)
      {
        return new JsonResult(new { Code = HttpStatusCode.InternalServerError, exception.GetBaseException().Message });
      }
    }

    private async Task DoActivationAndNotification(string projectUid, Dictionary<Guid, bool> filesToUpdate)
    {
      var dbUpdateResult = await SetFileActivatedState(projectUid, filesToUpdate);
      var notificationTask = notificationHubClient.Notify(new ProjectChangedNotification(Guid.Parse(projectUid)));
      var raptorTask = NotifyRaptorUpdateFile(new Guid(projectUid), dbUpdateResult);

      await Task.WhenAll(notificationTask, raptorTask);
    }

    #endregion fileActivation
  }
}
