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
  public class FileImportV4Controller : FileImportBaseController
  {

    public FileImportV4Controller(IKafka producer, IRepository<IProjectEvent> projectRepo,
      IConfigurationStore store, IRaptorProxy raptorProxy, IFileRepository fileRepo, ILoggerFactory logger)
      : base(producer, projectRepo, store, raptorProxy, fileRepo, logger)
    {
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
    public ActionResult CreateUpload()
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
    /// <param name="surveyedSurfaceUtc"></param>
    /// <remarks>Import a design file for a project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v4/importedfile")]
    [HttpPost]
    [ActionName("CreateUpload")]
    [FlowUpload("svl")]
    public async Task<ImportedFileDescriptorSingleResult> CreateImportedFileV4(FlowFile file,
      [FromUri] Guid projectUid, [FromUri] ImportedFileType importedFileType,
      [FromUri] DateTime? surveyedSurfaceUtc = null)
    {
      // todo change ImportedFileType to same as nhOPEnums
      var customerUid = ((User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;

      FileImportDataValidator.ValidateImportedFileRequest(file, projectUid, importedFileType, surveyedSurfaceUtc);
      log.LogInformation(
        $"CreateImportedFileV4. file: {JsonConvert.SerializeObject(file)} projectUid {projectUid.ToString()} ImportedFileType: {importedFileType} surveyedSurfaceUtc {(surveyedSurfaceUtc == null ? "N/A" : surveyedSurfaceUtc.ToString())}");

      if (!System.IO.File.Exists(file.path))
      {
        var error = string.Format("CreateImportedFileV4. The uploaded file {0} is not accessible.", file.path);
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, error));
      }

      //validate customer-project relationship. if it fails, exception will be thrown from within the method
      var project = await GetProject(projectUid.ToString()).ConfigureAwait(false);

      var importedFileList = await GetImportedFileList(projectUid.ToString()).ConfigureAwait(false);
      var existing = importedFileList.First(f => f.Name == file.flowFilename
                                                 && f.ImportedFileType == importedFileType
                                                 && (
                                                   (importedFileType == ImportedFileType.SurveyedSurface &&
                                                    f.SurveyedUtc == surveyedSurfaceUtc) ||
                                                   (importedFileType != ImportedFileType.SurveyedSurface)
                                                 ));
      if (existing != null)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            @"CreateImportedFileV4. File: {file.flowName} has already been imported."));


      // write file to TCC, returning filespaceID; path and filename which identifies it uniquely in TCC
      var fileDescriptor = await WriteFileToRepository(customerUid, projectUid.ToString(), file.path, importedFileType,
        surveyedSurfaceUtc);

      await NotifyRaptorAddFile(projectUid.ToString(), fileDescriptor);

      // only if all succeeds then 
      //   write to Db and 
      //   write new CreateImportedFileEvent to kafka que
      DateTime createdUserDate = DateTime.UtcNow; // endpoint param UI obtains from File properties // todo
      string importedBy = "whoever"; // free form name entered by user // todo
      CreateImportedFileEvent createImportedFileEvent = await CreateImportedFile(Guid.Parse(customerUid), projectUid,
          importedFileType, file.flowFilename, surveyedSurfaceUtc, JsonConvert.SerializeObject(fileDescriptor),
          createdUserDate, importedBy)
        .ConfigureAwait(false);

      var importedFile = new ImportedFileDescriptorSingleResult(
        (await GetImportedFileList(projectUid.ToString()).ConfigureAwait(false))
        .ToImmutableList()
        .First(f => f.ImportedFileUid == createImportedFileEvent.ImportedFileUID.ToString())
      );
      log.LogInformation(
        $"CreateImportedFileV4. completed succesfully. Response: {JsonConvert.SerializeObject(importedFile)}");

      //System.IO.File.Delete(file.path); todo should/can we delete temp file?
      return importedFile;
    }



    /// <summary>
    /// Used as a callback by Flow.JS
    /// </summary>
    /// <returns></returns>
    [Route("api/v4/importedfile")]
    [HttpGet]
    public ActionResult UpdateUpload()
    {
      return new NoContentResult();
    }

    // PUT: api/v4/importedfile
    /// <summary>
    /// Update imported file
    ///   this updates database AND updates file in TCC.
    ///   no need to notify RaptorWebAPI.
    /// note that a changed to the surveyedSurfaceUtc would come via a CreateImportedFile as it changes the filename etc.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="projectUid"></param>
    /// <param name="importedFileType"></param>
    /// <param name="surveyedSurfaceUtc"></param>
    /// <remarks>Updates and Imported design file for a project</remarks>
    /// <response code="200">Ok</response>
    /// <response code="400">Bad request</response>
    [Route("api/v4/importedfile")]
    [HttpPut]
    [ActionName("UpdateUpload")]
    [FlowUpload("svl")]
    public async Task<ImportedFileDescriptorSingleResult> UpdateImportedFileV4(FlowFile file,
      [FromUri] Guid projectUid, [FromUri] ImportedFileType importedFileType,
      [FromUri] DateTime? surveyedSurfaceUtc = null)
    {
      // todo change ImportedFileType to same as nhOPEnums
      var customerUid = ((User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;

      FileImportDataValidator.ValidateImportedFileRequest(file, projectUid, importedFileType, surveyedSurfaceUtc);
      log.LogInformation(
        $"UpdateImportedFileV4. file: {JsonConvert.SerializeObject(file)} projectUid {projectUid.ToString()} ImportedFileType: {importedFileType} surveyedSurfaceUtc {(surveyedSurfaceUtc == null ? "N/A" : surveyedSurfaceUtc.ToString())}");

      if (!System.IO.File.Exists(file.path))
      {
        var error = string.Format("UpdateImportedFileV4. The uploaded file {0} is not accessible.", file.path);
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, error));
      }

      //validate customer-project relationship. if it fails, exception will be thrown from within the method
      var project = await GetProject(projectUid.ToString()).ConfigureAwait(false);

      var importedFileList = await GetImportedFileList(projectUid.ToString()).ConfigureAwait(false);
      var existing = importedFileList.First(f => f.Name == file.flowFilename
                                                 && f.ImportedFileType == importedFileType
                                                 && (
                                                   (importedFileType == ImportedFileType.SurveyedSurface &&
                                                    f.SurveyedUtc == surveyedSurfaceUtc) ||
                                                   (importedFileType != ImportedFileType.SurveyedSurface)
                                                 ));
      if (existing == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            @"UpdateImportedFileV4. File: {file.flowName} is not known to ProjectMDM."));


      // update file in TCC 
      // todo will this method be changed to a) do exists check and b) us a seperate updateFile method or parameter?
      var fileDescriptor = await WriteFileToRepository(customerUid, projectUid.ToString(), file.path, importedFileType,
        surveyedSurfaceUtc);

      // only if all succeeds then 
      //   update Db and 
      //   write new UpdateImportedFileEvent to kafka que
      UpdateImportedFileEvent updateImportedFileEvent =
        await UpdateImportedFile(existing, JsonConvert.SerializeObject(fileDescriptor)).ConfigureAwait(false);

      var importedFile = new ImportedFileDescriptorSingleResult(
        (await GetImportedFileList(projectUid.ToString()).ConfigureAwait(false))
        .ToImmutableList()
        .First(f => f.ImportedFileUid == existing.ImportedFileUid.ToString())
      );
      log.LogInformation(
        $"UpdateImportedFileV4. Completed succesfully. Response: {JsonConvert.SerializeObject(importedFile)}");

      //System.IO.File.Delete(file.path); todo should/can we delete temp file?
      return importedFile;
    }
  }
}



