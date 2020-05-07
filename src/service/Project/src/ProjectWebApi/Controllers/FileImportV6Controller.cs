using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Extensions;
using VSS.DataOcean.Client;
using VSS.FlowJSHandler;
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
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.Project.Abstractions.Utilities;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// File Import controller v6
  /// </summary>
  public class FileImportV6Controller : FileImportBaseController
  {
    private readonly INotificationHubClient _notificationHubClient;

    /// <summary>
    /// File import controller v6
    /// </summary>
    public FileImportV6Controller(IConfigurationStore config, Func<TransferProxyType, ITransferProxy> persistantTransferProxy,
                                  IFilterServiceProxy filterServiceProxy, ITRexImportFileProxy tRexImportFileProxy,
                                  IRequestFactory requestFactory, INotificationHubClient notificationHubClient)
      : base(config, persistantTransferProxy, filterServiceProxy, tRexImportFileProxy, requestFactory)
    {
      this._notificationHubClient = notificationHubClient;
    }

    /// <summary>
    /// Gets a list of imported files for a project. The list includes files of all types.
    /// </summary>
    [Route("api/v4/importedfiles")] // temporary kludge until ccssscon-219 
    [Route("api/v6/importedfiles")]
    [HttpGet]
    public async Task<ImportedFileDescriptorListResult> GetImportedFilesV6([FromQuery] string projectUid, [FromQuery] bool getProjectCalibrationFiles=false)
    {
      Logger.LogInformation($"{nameof(GetImportedFilesV6)}");
      var result = new ImportedFileDescriptorListResult();
      if (getProjectCalibrationFiles)
      {
        var configResult = await CwsProfileSettingsClient.GetProjectConfigurations(new Guid(projectUid), customHeaders);
        result.ProjectConfigFileDescriptors = configResult.ProjectConfigurationFiles.ToImmutableList();
      }
      else
      {
        result.ImportedFileDescriptors = await ImportedFileRequestDatabaseHelper.GetImportedFileList(projectUid, Logger, userId, ProjectRepo);
      }
      return result;
    }

    /// <summary>
    /// Used as a callback by Flow.JS
    /// </summary>
    [Route("api/v4/importedfile")]
    [Route("api/v6/importedfile")]
    [HttpGet]
    public ActionResult Upload()
    {
      return new NoContentResult();
    }

    /// <summary>
    /// Upload a file, and do processing synchronously
    /// </summary>
    [Route("api/v4/importedfile")]
    [Route("api/v6/importedfile")]
    [HttpPost]
    [FlowUpload(Extensions = new[]
    {
      "svl", "dxf", "ttm", "tif"
    }, Size = 1000000000)]
    public async Task<ImportedFileDescriptorSingleResult> SyncUpload(
      [FromServices] ISchedulerProxy schedulerProxy,
      FlowFile file,
      [FromQuery] Guid projectUid,
      [FromQuery] ImportedFileType importedFileType,
      [FromQuery] DxfUnitsType dxfUnitsType,
      [FromQuery] DateTime fileCreatedUtc,
      [FromQuery] DateTime fileUpdatedUtc,
      [FromQuery] DateTime? surveyedUtc)
    {
      if (importedFileType == ImportedFileType.ReferenceSurface)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 122);
      }

      // Validate the file
      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(
        file, projectUid, importedFileType, dxfUnitsType, fileCreatedUtc, fileUpdatedUtc, userEmailAddress, surveyedUtc, null, null);

      ImportedFileUtils.ValidateEnvironmentVariables(importedFileType, ConfigStore, ServiceExceptionHandler);
      Logger.LogInformation(
        $"{nameof(SyncUpload)}: file: {file.flowFilename} path {file.path} projectUid {projectUid} ImportedFileType: {importedFileType} " +
        $"DxfUnitsType: {dxfUnitsType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}");

      await ValidateFileDoesNotExist(projectUid.ToString(), file.flowFilename, importedFileType, surveyedUtc, null, null);

      ContractExecutionResult importedFileResult;

      using (var fileStream = System.IO.File.Open(file.path, FileMode.Open, FileAccess.Read))
      {
        importedFileResult = await UpsertFileInternal(file.flowFilename, fileStream, projectUid, importedFileType, dxfUnitsType,
          fileCreatedUtc, fileUpdatedUtc, surveyedUtc, schedulerProxy);
      }

      Logger.LogInformation(
        $"{nameof(SyncUpload)}: Completed successfully. Response: {JsonConvert.SerializeObject(importedFileResult)}");

      return importedFileResult as ImportedFileDescriptorSingleResult;
    }

    /// <summary>
    /// POST or PUT Request to upload a file using a background task
    /// </summary>
    /// <returns>Schedule Job Result with a Job ID</returns>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v4/importedfile/background")]
    [Route("api/v6/importedfile/background")]
    [HttpPost]
    [HttpPut]
    [FlowUpload(Extensions = new[]
    {
      "svl", "dxf", "ttm", "tif"
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
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 122);
      }

      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(
        file, projectUid, importedFileType, dxfUnitsType, fileCreatedUtc, fileUpdatedUtc, userEmailAddress, surveyedUtc, null, null);
      Logger.LogInformation(
        $"{nameof(BackgroundUpload)}: file: {file.flowFilename} path {file.path} projectUid {projectUid} ImportedFileType: {importedFileType} " +
        $"DxfUnitsType: {dxfUnitsType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}");

      if (string.Equals(Request.Method, HttpMethod.Post.ToString(), StringComparison.OrdinalIgnoreCase))
      {
        await ValidateFileDoesNotExist(projectUid.ToString(), file.flowFilename, importedFileType, surveyedUtc, null, null);
      }

      var s3Path = $"project/importedfile/{Guid.NewGuid()}.dat";
      var fileStream = System.IO.File.Open(file.path, FileMode.Open, FileAccess.Read);
      var transferProxy = transferProxyFunc(TransferProxyType.Default);
      transferProxy.Upload(fileStream, s3Path);

      var baseUrl = Request.Host.ToUriComponent();

      // The QueryString will have values in it, so it's safe to add extra queries with the & as opposed to ?, then &
      var callbackUrl = $"http://{baseUrl}/internal/v6/importedfile{Request.QueryString}";
      callbackUrl += $"&filename={WebUtility.UrlEncode(file.flowFilename)}&awsFilePath={WebUtility.UrlEncode(s3Path)}";

      Logger.LogInformation($"{nameof(BackgroundUpload)}: baseUrl {callbackUrl}");

      var executionTimeout = ConfigStore.GetValueInt("PEGASUS_EXECUTION_TIMEOUT_MINS", 5) * 60000;//minutes converted to millisecs
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
    [Route("internal/v6/importedfile")]
    [HttpGet]
    public async Task<ImportedFileDescriptorSingleResult> InternalImportedFileV6(

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
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 122);
      }

      ImportedFileUtils.ValidateEnvironmentVariables(importedFileType, ConfigStore, ServiceExceptionHandler);

      ImportedFileDescriptorSingleResult importedFileResult = null;
      var transferProxy = transferProxyFunc(TransferProxyType.Default);
      Logger.LogInformation(
        $"{nameof(InternalImportedFileV6)}:. filename: {filename} awspath {awsFilePath} projectUid {projectUid} ImportedFileType: {importedFileType} " +
        $"DxfUnitsType: {dxfUnitsType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}");

      // Retrieve the stored file from AWS
      var fileResult = await transferProxy.Download(awsFilePath);
      if (fileResult == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 55);
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

      Logger.LogInformation(
          $"{nameof(InternalImportedFileV6)}: Completed successfully. Response: {JsonConvert.SerializeObject(importedFileResult)}");

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
    [Route("api/v6/importedfile")]
    [Route("internal/v6/importedfile")]
    [HttpPut]
    [ActionName("Upload")]
    [FlowUpload(Extensions = new[]
    {
      "svl", "dxf", "ttm", "tif"
    }, Size = 1_000_000_000)]

    public Task<ImportedFileDescriptorSingleResult> UpsertImportedFileV6(
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
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 122);
      }

      FlowJsFileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType, dxfUnitsType, fileCreatedUtc,
        fileUpdatedUtc, userEmailAddress, surveyedUtc, null, null);

      ImportedFileUtils.ValidateEnvironmentVariables(importedFileType, ConfigStore, ServiceExceptionHandler);

      Logger.LogInformation(
        $"{nameof(UpsertImportedFileV6)}: file: {JsonConvert.SerializeObject(file)} projectUid {projectUid} ImportedFileType: {importedFileType} DxfUnitsType: {dxfUnitsType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}");

      return UpsertFile(file.path, file.flowFilename, projectUid.ToString(), importedFileType, dxfUnitsType, fileCreatedUtc, fileUpdatedUtc, surveyedUtc, schedulerProxy, uploadToTcc);
    }

    /// <summary>
    /// Upsert imported file and create or update the database and do the necessary create or update in TCC.
    /// Also notify RaptorWebAPI of the change.
    /// </summary>
    /// <remarks>
    /// Intended for use by 3rd party connected systems that wish to avoid using FlowJS file upload framework.
    /// </remarks>
    [Route("api/v4/importedfile/direct")]
    [Route("api/v6/importedfile/direct")]
    [HttpPost]
    [DisableFormValueModelBinding]
    [RequestSizeLimit(1_000_000_000)]
    public async Task<ImportedFileDescriptorSingleResult> CreateImportedFileDirectV6(
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
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 122);
      }

      FileImportDataValidator.ValidateUpsertImportedFileRequest(projectUid, importedFileType, dxfUnitsType, fileCreatedUtc,
        fileUpdatedUtc, userEmailAddress, surveyedUtc, filename, null, null);
      ImportedFileUtils.ValidateEnvironmentVariables(importedFileType, ConfigStore, ServiceExceptionHandler);
      Logger.LogInformation(
        $"{nameof(CreateImportedFileDirectV6)}: ProjectUID: `{projectUid}`, Filename: `{filename}` ImportedFileType: `{importedFileType}`, DxfUnitsType: `{dxfUnitsType}`, SurveyedUTC: `{(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}`");

      //When debugging locally using Postman, remove this check so can do an update
      await ValidateFileDoesNotExist(projectUid.ToString(), filename, importedFileType, surveyedUtc, null, null);

      if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 58, $"Expected a multipart request, but got '{Request.ContentType}'");
      }

      var tempFilePath = await HttpContext.Request.StreamFile(Guid.NewGuid().ToString(), Logger);

      var result = await UpsertFile(tempFilePath, filename, projectUid.ToString(), importedFileType, dxfUnitsType, fileCreatedUtc, fileUpdatedUtc, surveyedUtc, schedulerProxy);

      return result;
    }

    /// <summary>
    /// Delete imported file
    ///    For Trex gateway, projectSvc stores the design file in S3 (currently)
    ///      As of this writing, the file will remain there, even after deletion
    /// </summary>
    /// <remarks>Deletes existing imported file</remarks>
    [Route("api/v4/importedfile")]
    [Route("api/v6/importedfile")]
    [HttpDelete]
    public async Task<ContractExecutionResult> DeleteImportedFileV6(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? importedFileUid, // for 3dpm imported files
      [FromQuery] ImportedFileType? importedFileType, // for CWS project configuration files
      [FromQuery] string filename, // for CWS project configuration files
      [FromServices] IPegasusClient pegasusClient,
      [FromServices] IWebClientWrapper webClient)
    {
      Logger.LogInformation($"{nameof(DeleteImportedFileV6)}: projectUid {projectUid} importedFileUid: {importedFileUid} importedFileType: {importedFileType}");

      await ValidateProjectId(projectUid.ToString());

      if (importedFileType.HasValue && CwsConfigFileHelper.isCwsFileType(importedFileType.Value))
      {
        await CwsConfigFileHelper.DeleteFileFromCws(projectUid, importedFileType.Value, filename, CwsDesignClient, 
          CwsProfileSettingsClient, ServiceExceptionHandler, webClient, customHeaders);
        Logger.LogInformation(
          $"{nameof(DeleteImportedFileV6)}: Completed successfully. projectUid {projectUid} importedFileType: {importedFileType}");
        return new ContractExecutionResult();
      }
      else
      {
        var importedFiles = await ImportedFileRequestDatabaseHelper.GetImportedFiles(projectUid.ToString(), Logger, ProjectRepo).ConfigureAwait(false);
        ImportedFile existing = null;
        if (importedFiles.Count > 0)
          existing = importedFiles.FirstOrDefault(f => f.ImportedFileUid == importedFileUid.ToString());

        if (existing == null)
        {
          ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 56);
          return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "shouldn't get here"); // to keep compiler happy
        }

        ImportedFileUtils.ValidateEnvironmentVariables(existing.ImportedFileType, ConfigStore, ServiceExceptionHandler);

        var deleteImportedFile = new DeleteImportedFile(
          projectUid, existing.ImportedFileType, JsonConvert.DeserializeObject<FileDescriptor>(existing.FileDescriptor),
          Guid.Parse(existing.ImportedFileUid), existing.ImportedFileId, existing.LegacyImportedFileId,
          DataOceanRootFolderId, existing.SurveyedUtc);

        var result = await WithServiceExceptionTryExecuteAsync(() =>
          RequestExecutorContainerFactory
            .Build<DeleteImportedFileExecutor>(
              LoggerFactory, ConfigStore, ServiceExceptionHandler, customerUid, userId, userEmailAddress, customHeaders,
              productivity3dV2ProxyNotification: Productivity3dV2ProxyNotification, persistantTransferProxy: persistantTransferProxy, filterServiceProxy: filterServiceProxy, tRexImportFileProxy: tRexImportFileProxy,
              projectRepo: ProjectRepo, fileRepo: FileRepo, dataOceanClient: DataOceanClient, authn: Authorization, pegasusClient: pegasusClient)
            .ProcessAsync(deleteImportedFile)
        );
        await _notificationHubClient.Notify(new ProjectChangedNotification(projectUid));

        Logger.LogInformation(
          $"{nameof(DeleteImportedFileV6)}: Completed successfully. projectUid {projectUid} importedFileUid: {importedFileUid}");
        return result;
      }
    }

    /// <summary>
    /// Common file processing method used by all importedFile endpoints.
    /// </summary>
    private async Task<ImportedFileDescriptorSingleResult> UpsertFile(
      string tmpFilePath,
      string filename,
      string projectUid,
      ImportedFileType importedFileType,
      DxfUnitsType dxfUnitsType,
      DateTime fileCreatedUtc,
      DateTime fileUpdatedUtc,
      DateTime? surveyedUtc,
      ISchedulerProxy schedulerProxy,
      bool uploadToTcc = true)
    {
      if (!System.IO.File.Exists(tmpFilePath))
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 55);
      }

      using (var fileStream = new FileStream(tmpFilePath, FileMode.Open))
      {
        return await UpsertFileInternal(filename, fileStream, Guid.Parse(projectUid), importedFileType, dxfUnitsType,
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
      ImportedFileDescriptorSingleResult importedFile = null;

      if (CwsConfigFileHelper.isCwsFileType(importedFileType))
      { 
        Logger.LogInformation($"{nameof(UpsertFileInternal)}. Found a CWS file type");
        // Only save to CWS. 3dpm doesn't use these files.
        //TODO: handle errors from CWS
        var result = await CwsConfigFileHelper.SaveFileToCws(projectUid, filename, 
          fileStream, importedFileType, CwsDesignClient, CwsProfileSettingsClient, customHeaders);
        importedFile = new ImportedFileDescriptorSingleResult(result);
        Logger.LogInformation(
          $"{nameof(UpsertFileInternal)}: Create/Update completed successfully. Response: {JsonConvert.SerializeObject(importedFile)}");
      }
      else
      {
        var existing = await ImportedFileRequestDatabaseHelper
          .GetImportedFileForProject
          (projectUid.ToString(), filename, importedFileType, surveyedUtc,
            Logger, ProjectRepo, offset, parentUid)
          .ConfigureAwait(false);

        var creating = existing == null;

        Logger.LogInformation(
          creating
            ? $"{nameof(UpsertFileInternal)}. file doesn't exist already in DB: {filename} projectUid {projectUid} ImportedFileType: {importedFileType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())} parentUid {parentUid} offset: {offset}"
            : $"{nameof(UpsertFileInternal)}. file exists already in DB. Will be updated: {JsonConvert.SerializeObject(existing)}");

        if (importedFileType == ImportedFileType.GeoTiff)
        {
          uploadToTcc = false;
        }

        FileDescriptor fileDescriptor = null;

        var importedFileUid = creating ? Guid.NewGuid() : Guid.Parse(existing.ImportedFileUid);
        var dataOceanFileName = DataOceanFileUtil.DataOceanFileName(filename,
          importedFileType == ImportedFileType.SurveyedSurface || importedFileType == ImportedFileType.GeoTiff,
          importedFileUid, surveyedUtc);

        if (importedFileType == ImportedFileType.ReferenceSurface)
        {
          //FileDescriptor not used for reference surface but validation requires values
          fileDescriptor = FileDescriptor.CreateFileDescriptor("Not applicable", "Not applicable", filename);
        }
        else if (importedFileType == ImportedFileType.GeoTiff)
        {
          //save copy to DataOcean      
          await DataOceanHelper.WriteFileToDataOcean(
            fileStream, DataOceanRootFolderId, customerUid, projectUid.ToString(), dataOceanFileName,
            Logger, ServiceExceptionHandler, DataOceanClient, Authorization, importedFileUid, ConfigStore);

          fileDescriptor = FileDescriptor.CreateFileDescriptor(
            FileSpaceId,
            $"/{customerUid}/{projectUid}",
            filename);
        }
        else
        {
          if (UseTrexGatewayDesignImport && IsDesignFileType(importedFileType))
          {
            fileDescriptor = ProjectRequestHelper.WriteFileToS3Repository(
              fileStream, projectUid.ToString(), filename,
              importedFileType == ImportedFileType.SurveyedSurface, surveyedUtc,
              Logger, ServiceExceptionHandler, persistantTransferProxy);
          }

          if (UseRaptorGatewayDesignImport)
          {
            if (uploadToTcc)
            {
              fileDescriptor = await TccHelper.WriteFileToTCCRepository(
                  fileStream, customerUid, projectUid.ToString(), filename,
                  importedFileType == ImportedFileType.SurveyedSurface,
                  surveyedUtc, FileSpaceId, Logger, ServiceExceptionHandler, FileRepo)
                .ConfigureAwait(false);
            }
            // This whole uploadToTCC workflow is strictly only for the TCC -> DataOcean migration.
            else
            {
              Logger.LogDebug($"{nameof(UpsertFileInternal)}. Opted out of uploading to TCC, constructing pseudo fileDescriptor.");

              var tccFileName = Path.GetFileName(filename);
              if (importedFileType == ImportedFileType.SurveyedSurface && surveyedUtc != null)
              {
                tccFileName = tccFileName.IncludeSurveyedUtcInName(surveyedUtc.Value);
              }

              fileDescriptor = FileDescriptor.CreateFileDescriptor(
                FileSpaceId,
                $"/{customerUid}/{projectUid}",
                tccFileName);
            }

            //save copy to DataOcean      
            await DataOceanHelper.WriteFileToDataOcean(
                fileStream, DataOceanRootFolderId, customerUid, projectUid.ToString(), dataOceanFileName,
                Logger, ServiceExceptionHandler, DataOceanClient, Authorization, importedFileUid, ConfigStore)
              .ConfigureAwait(false);
          }
        }

        if (creating)
        {
          var createImportedFile = new CreateImportedFile(
            projectUid, filename, fileDescriptor, importedFileType, surveyedUtc, dxfUnitsType,
            fileCreatedUtc, fileUpdatedUtc, DataOceanRootFolderId, parentUid, offset, importedFileUid, dataOceanFileName);

          importedFile = await WithServiceExceptionTryExecuteAsync(() =>
            RequestExecutorContainerFactory
              .Build<CreateImportedFileExecutor>(
                LoggerFactory, ConfigStore, ServiceExceptionHandler, customerUid, userId, userEmailAddress, customHeaders,
                productivity3dV2ProxyNotification: Productivity3dV2ProxyNotification, productivity3dV2ProxyCompaction: Productivity3dV2ProxyCompaction,
                persistantTransferProxy: persistantTransferProxy, tRexImportFileProxy: tRexImportFileProxy,
                projectRepo: ProjectRepo, fileRepo: FileRepo, dataOceanClient: DataOceanClient, authn: Authorization, schedulerProxy: schedulerProxy)
              .ProcessAsync(createImportedFile)
          ) as ImportedFileDescriptorSingleResult;

          Logger.LogInformation(
            $"{nameof(UpsertFileInternal)}: Create completed successfully. Response: {JsonConvert.SerializeObject(importedFile)}");
        }
        else
        {
          // this also validates that this customer has access to the projectUid
          var project = await ProjectRequestHelper.GetProject(projectUid.ToString(), customerUid, Logger, ServiceExceptionHandler, ProjectRepo);

          var importedFileUpsertEvent = new UpdateImportedFile(
            projectUid, project.ShortRaptorProjectId, importedFileType,
            (importedFileType == ImportedFileType.SurveyedSurface || importedFileType == ImportedFileType.GeoTiff)
              ? surveyedUtc
              : null,
            dxfUnitsType, fileCreatedUtc, fileUpdatedUtc, fileDescriptor,
            Guid.Parse(existing?.ImportedFileUid), existing.ImportedFileId,
            DataOceanRootFolderId, offset, dataOceanFileName);

          importedFile = await WithServiceExceptionTryExecuteAsync(() =>
            RequestExecutorContainerFactory
              .Build<UpdateImportedFileExecutor>(
                LoggerFactory, ConfigStore, ServiceExceptionHandler, customerUid, userId, userEmailAddress, customHeaders,
                productivity3dV2ProxyNotification: Productivity3dV2ProxyNotification, productivity3dV2ProxyCompaction: Productivity3dV2ProxyCompaction,
                tRexImportFileProxy: tRexImportFileProxy,
                projectRepo: ProjectRepo, fileRepo: FileRepo, dataOceanClient: DataOceanClient, authn: Authorization, schedulerProxy: schedulerProxy)
              .ProcessAsync(importedFileUpsertEvent)
          ) as ImportedFileDescriptorSingleResult;

          Logger.LogInformation(
            $"{nameof(UpsertFileInternal)}: Update completed successfully. Response: {JsonConvert.SerializeObject(importedFile)}");
        }
      }

      await _notificationHubClient.Notify(new ProjectChangedNotification(projectUid));

      return importedFile;
    }


    /// <summary>
    /// Validate that the uploaded file doesn't already exist in the database.
    /// Should only be called from create methods where there's an expectation the file isn't already present.
    /// </summary>
    private async Task ValidateFileDoesNotExist(string projectUid, string filename, ImportedFileType importedFileType, DateTime? surveyedUtc, Guid? parentUid, double? offset)
    {
      var fileExists = false;
      if (CwsConfigFileHelper.isCwsFileType(importedFileType))
      {
        var existingFile = await CwsConfigFileHelper.GetCwsFile(new Guid( projectUid), filename, importedFileType, CwsProfileSettingsClient, customHeaders);
        fileExists = existingFile != null;
      }
      else
      {
        var importedFileList = ImportedFileRequestDatabaseHelper.GetImportedFileList(projectUid, Logger, userId, ProjectRepo)
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
        fileExists = importedFileDescriptor != null;
      }
      if (fileExists)
      {
        var message = $"{nameof(ValidateFileDoesNotExist)}: File: {filename} has already been imported.";
        Logger.LogError(message);
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, importedFileType == ImportedFileType.ReferenceSurface ? 121 : 58);
      }
    }

    /// <summary>
    /// Import a reference surface
    /// </summary>
    [Route("api/v6/importedfile/referencesurface")]
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
      Logger.LogInformation($"{nameof(CreateReferenceSurface)}: projectUid {projectUid} filename: {filename} parentUid: {parentUid} offset: {offset}");

      await ValidateProjectId(projectUid.ToString());

      ImportedFileUtils.ValidateEnvironmentVariables(ImportedFileType.ReferenceSurface, ConfigStore, ServiceExceptionHandler);

      //Check parent design does exist
      var importedFiles = await ImportedFileRequestDatabaseHelper.GetImportedFileList(projectUid.ToString(), Logger, userId, ProjectRepo);
      var parent = importedFiles.FirstOrDefault(i => i.ImportedFileUid == parentUid.ToString());
      if (parent == null)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 120);
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
      await ValidateFileDoesNotExist(projectUid.ToString(), filename, ImportedFileType.ReferenceSurface, null, parentUid, offset);

      var importedFileResult = await UpsertFileInternal(filename, null, projectUid, ImportedFileType.ReferenceSurface, DxfUnitsType.Meters,
        fileCreatedUtc, fileUpdatedUtc, null, schedulerProxy, parentUid, offset);

      //If parent design is deactivated then deactivate reference surface
      if (!parent.IsActivated)
      {
        var filesToUpdate = new Dictionary<Guid, bool>();
        filesToUpdate.Add(new Guid(importedFileResult.ImportedFileDescriptor.ImportedFileUid), false);
        await DoActivationAndNotification(projectUid.ToString(), filesToUpdate);
        importedFiles = await ImportedFileRequestDatabaseHelper.GetImportedFileList(projectUid.ToString(), Logger, userId, ProjectRepo);
        importedFileResult.ImportedFileDescriptor = importedFiles.SingleOrDefault(i =>
          i.ImportedFileUid == importedFileResult.ImportedFileDescriptor.ImportedFileUid);
      }

      Logger.LogInformation(
        $"{nameof(CreateReferenceSurface)}: Completed successfully. Response: {JsonConvert.SerializeObject(importedFileResult)}");

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
      const double imperialFeetToMetres = 0.3048;
      const double usFeetToMetres = 0.304800609601;

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
          displayOffset = offset / imperialFeetToMetres;
          unitsString = "ft";
          break;
        case UnitsTypeEnum.US:
          displayOffset = offset / usFeetToMetres;
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
    [Route("api/v6/importedfiles")]
    [HttpPut]
    public async Task<IActionResult> UpdateImportedFileActivationStateV6(string projectUid, [FromBody] ActivatedImportFilesRequest request)
    {
      Logger.LogInformation($"{nameof(UpdateImportedFileActivationStateV6)}:");

      await ValidateProjectId(projectUid).ConfigureAwait(false);

      if (request == null)
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 40);

      var fileIds = string.Join(",", request.ImportedFileDescriptors.Select(x => x.ImportedFileUid));
      if (string.IsNullOrEmpty(fileIds))
      {
        return Ok(new { Code = HttpStatusCode.BadRequest, Message = "Request contains no imported file IDs." });
      }

      Logger.LogInformation($"{nameof(UpdateImportedFileActivationStateV6)}: projectUid: {projectUid}, fileUids: {fileIds}");

      var importedFiles = await ImportedFileRequestDatabaseHelper.GetImportedFileList(projectUid, Logger, userId, ProjectRepo).ConfigureAwait(false);
      if (!importedFiles.Any())
      {
        Logger.LogInformation($"{nameof(UpdateImportedFileActivationStateV6)}: Attempt to set file activation state when project contains no files");

        return Ok(new { Code = HttpStatusCode.BadRequest, Message = "Project contains no imported files." });
      }

      var filesToUpdate = new Dictionary<Guid, bool>();

      foreach (var activatedFileDescriptor in request.ImportedFileDescriptors)
      {
        var existingFile = importedFiles.FirstOrDefault(f => f.ImportedFileUid == activatedFileDescriptor.ImportedFileUid);
        if (existingFile == null)
        {
          Logger.LogError(
            $"{nameof(UpdateImportedFileActivationStateV6)}: File doesn't exist. projectUid {projectUid}, fileUid: {activatedFileDescriptor.ImportedFileUid}");
          continue;
        }

        if (existingFile.ImportedFileType == ImportedFileType.ReferenceSurface)
        {
          Logger.LogError(
            $"{nameof(UpdateImportedFileActivationStateV6)}: Attempt to set file activation on a reference surface. projectUid {projectUid}, fileUid: {activatedFileDescriptor.ImportedFileUid}");
          continue;
        }

        if (existingFile.IsActivated == activatedFileDescriptor.IsActivated)
        {
          Logger.LogDebug(
            $"{nameof(UpdateImportedFileActivationStateV6)}: File activation state is already set to {existingFile.IsActivated}. No changes required. {existingFile.ImportedFileUid}");
          continue;
        }

        Logger.LogInformation(
          $"{nameof(UpdateImportedFileActivationStateV6)}: File queued for updating: {JsonConvert.SerializeObject(existingFile)}");
        filesToUpdate.Add(new Guid(activatedFileDescriptor.ImportedFileUid), activatedFileDescriptor.IsActivated);

        //If user is activating or deactivating a design which has reference surfaces, do as a group
        if (existingFile.ImportedFileType == ImportedFileType.DesignSurface)
        {
          var children = importedFiles
            .Where(f => f.ParentUid.HasValue && f.ParentUid.ToString() == existingFile.ImportedFileUid).ToList();
          if (children.Count > 0)
          {
            Logger.LogInformation(
              $"{nameof(UpdateImportedFileActivationStateV6)}: Setting file activation state of reference surfaces for design {existingFile.ImportedFileUid}");
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
        Logger.LogInformation($"{nameof(UpdateImportedFileActivationStateV6)}: No files eligible for activation state change.");

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
      var notificationTask = _notificationHubClient.Notify(new ProjectChangedNotification(Guid.Parse(projectUid)));
      var raptorTask = NotifyRaptorUpdateFile(new Guid(projectUid), dbUpdateResult);

      await Task.WhenAll(notificationTask, raptorTask);
    }

    #endregion fileActivation
  }
}
