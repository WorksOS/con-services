using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using FlowUploadFilter;
using Microsoft.Extensions.Logging;
using KafkaConsumer.Kafka;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProjectWebApi.Filters;
using ProjectWebApiCommon.Models;
using ProjectWebApiCommon.ResultsHandling;
using Repositories;
using Repositories.DBModels;
using TCCFileAccess;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSP.MasterData.Project.WebAPI.Controllers.V4
{
  /// <summary>
  /// File Import controller v4
  /// </summary>
  public class FileImportV4Controller : FileImportBaseController
  {

    /// <summary>
    /// File import controller v4
    /// </summary>
    /// <param name="producer"></param>
    /// <param name="projectRepo"></param>
    /// <param name="store"></param>
    /// <param name="raptorProxy"></param>
    /// <param name="fileRepo"></param>
    /// <param name="logger"></param>
    public FileImportV4Controller(IKafka producer, IRepository<IProjectEvent> projectRepo,
      IConfigurationStore store, IRaptorProxy raptorProxy, IFileRepository fileRepo, ILoggerFactory logger)
      : base(producer, projectRepo, store, raptorProxy, fileRepo, logger)
    {
      this.userEmailAddress = TIDAuthentication.EmailAddress;
      fileSpaceId = store.GetValueString("TCCFILESPACEID");
      if (string.IsNullOrEmpty(fileSpaceId))
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.TCCConfigurationError,
            "Unable to obtain TCC fileSpaceId"));
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
      return new ImportedFileDescriptorListResult()
      {
        ImportedFileDescriptors = await GetImportedFileList(projectUid).ConfigureAwait(false)
      };
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
    /// <param name="fileUpdatedUtc"></param>
    /// <param name="fileCreatedUtc"></param>
    /// <param name="surveyedUtc"></param>
    /// <remarks>Import a design file for a project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v4/importedfile")]
    [HttpPost]
    [ActionName("Upload")]
    [FlowUpload("svl")]
    public async Task<ImportedFileDescriptorSingleResult> CreateImportedFileV4(FlowFile file,
      [FromUri] Guid projectUid, [FromUri] ImportedFileType importedFileType,
      [FromUri] DateTime fileCreatedUtc, [FromUri] DateTime fileUpdatedUtc,
      [FromUri] DateTime? surveyedUtc = null)
    {

      var customerUid = ((User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;

      FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType, fileCreatedUtc,
        fileUpdatedUtc, userEmailAddress, surveyedUtc);
      log.LogInformation(
        $"CreateImportedFileV4. file: {JsonConvert.SerializeObject(file)} projectUid {projectUid.ToString()} ImportedFileType: {importedFileType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}");

      if (!System.IO.File.Exists(file.path))
      {
        var message = $"CreateImportedFileV4. The uploaded file {file.path} is not accessible.";
        log.LogError(message);
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, message));
      }

      //validate customer-project relationship. if it fails, exception will be thrown from within the method
      var project = await GetProject(projectUid.ToString()).ConfigureAwait(false);

      var importedFileList = await GetImportedFileList(projectUid.ToString()).ConfigureAwait(false);
      ImportedFileDescriptor importedFileDescriptor = null;
      if (importedFileList.Count > 0)
        importedFileDescriptor = importedFileList.FirstOrDefault(f => string.Equals(f.Name, file.flowFilename, StringComparison.OrdinalIgnoreCase)
                                                             && f.ImportedFileType == importedFileType
                                                             && (
                                                               (importedFileType == ImportedFileType.SurveyedSurface &&
                                                                f.SurveyedUtc == surveyedUtc) ||
                                                               (importedFileType != ImportedFileType.SurveyedSurface)
                                                             ));
      if (importedFileDescriptor != null)
      {
        var message = $"CreateImportedFileV4. File: {file.flowFilename} has already been imported.";
        log.LogError(message);
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.IncorrectRequestedData, message));
      }

      // write file to TCC, returning filespaceID; path and filename which identifies it uniquely in TCC
      var fileDescriptor = await WriteFileToRepository(customerUid, projectUid.ToString(), file.path,
          importedFileType, surveyedUtc)
        .ConfigureAwait(false);

      // todo await NotifyRaptorAddFile(project.LegacyProjectID, projectUid, fileDescriptor).ConfigureAwait(false);

      // if all succeeds, write to Db and kafka que
      CreateImportedFileEvent createImportedFileEvent = await CreateImportedFile(Guid.Parse(customerUid), projectUid,
          importedFileType, file.flowFilename, surveyedUtc, JsonConvert.SerializeObject(fileDescriptor),
          fileCreatedUtc, fileUpdatedUtc, userEmailAddress)
        .ConfigureAwait(false);

      var importedFile = new ImportedFileDescriptorSingleResult(
        (await GetImportedFileList(projectUid.ToString()).ConfigureAwait(false))
        .ToImmutableList()
        .First(f => f.ImportedFileUid == createImportedFileEvent.ImportedFileUID.ToString())
      );
      log.LogInformation(
        $"CreateImportedFileV4. completed succesfully. Response: {JsonConvert.SerializeObject(importedFile)}");

      // todo? System.IO.File.Delete(file.path); 
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
    /// <param name="fileCreatedUtc"></param>
    /// <param name="fileUpdatedUtc"></param>
    /// <param name="surveyedUtc"></param>
    /// <remarks>Updates and Imported design file for a project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v4/importedfile")]
    [HttpPut]
    [ActionName("Upload")]
    [FlowUpload("svl")]
    public async Task<ImportedFileDescriptorSingleResult> UpdateImportedFileV4(FlowFile file,
      [FromUri] Guid projectUid, [FromUri] ImportedFileType importedFileType,
      [FromUri] DateTime fileCreatedUtc, [FromUri] DateTime fileUpdatedUtc,
      [FromUri] DateTime? surveyedUtc = null)
    {
      var customerUid = ((User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
      FileImportDataValidator.ValidateUpsertImportedFileRequest(file, projectUid, importedFileType, fileCreatedUtc,
        fileUpdatedUtc, userEmailAddress, surveyedUtc);
      log.LogInformation(
        $"CreateImportedFileV4. file: {JsonConvert.SerializeObject(file)} projectUid {projectUid.ToString()} ImportedFileType: {importedFileType} surveyedUtc {(surveyedUtc == null ? "N/A" : surveyedUtc.ToString())}");

      if (!System.IO.File.Exists(file.path))
      {
        var error = $"CreateImportedFileV4. The uploaded file {file.path} is not accessible.";
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, error));
      }

      //validate customer-project relationship. if it fails, exception will be thrown from within the method
      var project = await GetProject(projectUid.ToString()).ConfigureAwait(false);

      var importedFileList = await GetImportedFileList(projectUid.ToString()).ConfigureAwait(false);
      ImportedFileDescriptor importedFileDescriptor = null;
      if (importedFileList.Count > 0)
      {
        importedFileDescriptor = importedFileList.FirstOrDefault(f => string.Equals(f.Name, file.flowFilename, StringComparison.OrdinalIgnoreCase)
                                                             && f.ImportedFileType == importedFileType
                                                             && (
                                                               (importedFileType == ImportedFileType.SurveyedSurface &&
                                                                f.SurveyedUtc == surveyedUtc) ||
                                                               (importedFileType != ImportedFileType.SurveyedSurface)
                                                             ));
      }

      // write file to TCC, returning filespaceID; path and filename which identifies it uniquely in TCC
      // this may be a create or update, so ok if it already exists in our DB
      var fileDescriptor = await WriteFileToRepository(customerUid, projectUid.ToString(), file.path,
          importedFileType,
          surveyedUtc)
        .ConfigureAwait(false);

      // todo await NotifyRaptorAddFile(project.LegacyProjectID, projectUid, fileDescriptor).ConfigureAwait(false);

      // if all succeeds, write to Db and kafka que
      var importedFileUid = importedFileDescriptor?.ImportedFileUid;
      if (importedFileDescriptor == null)
      {
        var createImportedFileEvent = await CreateImportedFile(Guid.Parse(customerUid), projectUid,
            importedFileType, file.flowFilename, surveyedUtc, JsonConvert.SerializeObject(fileDescriptor),
            fileCreatedUtc, fileUpdatedUtc, userEmailAddress)
          .ConfigureAwait(false);
        importedFileUid = createImportedFileEvent.ImportedFileUID.ToString();
      }
      else
        await UpdateImportedFile(importedFileDescriptor, JsonConvert.SerializeObject(fileDescriptor), surveyedUtc,
            fileUpdatedUtc, userEmailAddress)
          .ConfigureAwait(false);

      var importedFile = new ImportedFileDescriptorSingleResult(
        (await GetImportedFileList(projectUid.ToString()).ConfigureAwait(false))
        .ToImmutableList()
        .FirstOrDefault(f => f.ImportedFileUid == importedFileUid)
      );
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
      [FromUri] Guid importedFileUid)
    {
      log.LogInformation($"DeleteImportedFileV4. projectUid {projectUid} importedFileUid: {importedFileUid}");

      //validate customer-project relationship. if it fails, exception will be thrown from within the method
      var project = await GetProject(projectUid.ToString()).ConfigureAwait(false);

      var importedFiles = await GetImportedFiles(projectUid.ToString()).ConfigureAwait(false);
      ImportedFile importedFile = null;
      if (importedFiles.Count > 0)
        importedFile = importedFiles.FirstOrDefault(f => f.ImportedFileUid == importedFileUid.ToString());
      if (importedFile == null)
      {
        var error = $"DeleteImportedFileV4. projectUid {projectUid} importedFileUid: {importedFileUid} doesn't exist";
        log.LogError(error);
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, error));
      }

      await DeleteFileFromRepository(JsonConvert.DeserializeObject<FileDescriptor>(importedFile.FileDescriptor)).ConfigureAwait(false);
      
      // todo await NotifyRaptorDeleteFile(projectUid, importedFile.FileDescriptor).ConfigureAwait(false);

      await DeleteImportedFile(projectUid, importedFileUid).ConfigureAwait(false);

      log.LogInformation(
        $"DeleteImportedFileV4. Completed succesfully. ProjectUid {projectUid} importedFileUid: {importedFileUid}");
      return new ContractExecutionResult();
    }
  }
}



