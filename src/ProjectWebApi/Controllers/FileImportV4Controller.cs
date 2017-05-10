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
  public class FileImportV4Controller : FileImportBaseController
  {

    public FileImportV4Controller(IKafka producer, IRepository<IProjectEvent> projectRepo,
      IConfigurationStore store, IRaptorProxy raptorProxy, IFileRepository fileRepo, ILoggerFactory logger)
      : base(producer, projectRepo, store, raptorProxy, fileRepo, logger)
    {
      this.userEmailAddress = TIDAuthentication.EmailAddress;
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

      var fileDescriptor = UpsertImportedFile(1 /* create */, file, customerUid.ToString(), projectUid,
        importedFileType, fileCreatedUtc, fileUpdatedUtc, surveyedUtc);

      // only if all succeeds then 
      //   write to Db and 
      //   write new CreateImportedFileEvent to kafka que
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

      //System.IO.File.Delete(file.path); todo should/can we delete temp file?
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

      var fileDescriptor = UpsertImportedFile(2 /* upsert */, file, customerUid.ToString(), projectUid,
        importedFileType, fileCreatedUtc, fileUpdatedUtc, surveyedUtc);

      // only if all succeeds then 
      //   write to Db and 
      //   write new CreateImportedFileEvent to kafka que
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
        .First(f => f.ImportedFileUid == importedFileUid)
      );
      log.LogInformation(
        $"UpdateImportedFileV4. Completed succesfully. Response: {JsonConvert.SerializeObject(importedFile)}");

      //System.IO.File.Delete(file.path); todo should/can we delete temp file?
      return importedFile;
    }

    // todo delete
  }
}



