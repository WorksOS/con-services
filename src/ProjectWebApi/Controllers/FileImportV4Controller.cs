using System;
using System.Collections.Immutable;
using System.IO;
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
using ProjectWebApiCommon.Utilities;
using Repositories;
using Repositories.DBModels;
using TCCFileAccess;
using TCCFileAccess.Models;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.Raptor.Service.Common.Interfaces;

namespace VSP.MasterData.Project.WebAPI.Controllers.V4
{
    public class FileImportV4Controller : ProjectBaseController
    {

        public FileImportV4Controller(IKafka producer, IRepository<IProjectEvent> projectRepo,
            IRepository<ISubscriptionEvent> subscriptionsRepo, IConfigurationStore store, ISubscriptionProxy subsProxy,
            IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy, IFileRepository fileRepo, ILoggerFactory logger)
            : base(producer, projectRepo, subscriptionsRepo, store, subsProxy, geofenceProxy, raptorProxy, fileRepo, logger)
        {
        }

        /// <summary>
        /// Used as a callback by Flow.JS
        /// </summary>
        /// <returns></returns>
        [Route("api/v4/importedfiles")]
        [HttpGet]
        public ActionResult Upload()
        {
            return new NoContentResult();
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

        // POST: api/v4/importedfile
      /// <summary>
      /// Import a design file
      ///    this creates a link between the file and project in database AND sends file to TCC
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
      [ActionName("Upload")]
      [FlowUpload("svl")]
      public async Task<ImportedFileDescriptorSingleResult> CreateImportedFileV4(FlowFile file,
        [FromUri] Guid projectUid, ImportedFileType importedFileType, DateTime? surveyedSurfaceUtc = null)
      {

        //if (file == null)
        //{
        //  throw new ServiceException(HttpStatusCode.InternalServerError,
        //    new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
        //      "Missing imported file request"));
        //}
        //log.LogInformation(
        //  $"CreateImportedFileV4. file: {JsonConvert.SerializeObject(file)} projectUid {projectUid.ToString()} ImportedFileType: {importedFileType} surveyedSurfaceUtc {surveyedSurfaceUtc}");

        //// by the time the file arrives here, the file exists locally as follows. todo validate?

        //// validate customer-project relationship
        //var project = await GetProject(projectUid.ToString()).ConfigureAwait(false);
        //if (project == null)
        //{
        //  log.LogError($"Customer doesn't have access to {projectUid.ToString()}");
        //  throw new ServiceException(HttpStatusCode.Forbidden,
        //    new ContractExecutionResult(ContractExecutionStatesEnum.IncorrectRequestedData,
        //      @"No access to the project or project {projectUid.Tostring()} does not exist."));
        //}

        //// get the TCC super users filePath
        //var fileSpaceId = store.GetValueString("TCC_SUPER_FILESPACEID");
        //var org = new Organization()
        //{
        //  filespaceId = fileSpaceId,
        //  orgDisplayName = "",
        //  orgId = "",
        //  orgTitle = "",
        //  shortName = ""
        //};
        //var dirResult = await fileRepo.GetFolders(org, DateTime.MinValue, "/").ConfigureAwait(false);
        //if (dirResult?.entries == null || dirResult.entries.Length < 1)
        //{
        //  throw new ServiceException(HttpStatusCode.InternalServerError,
        //    new ContractExecutionResult(ContractExecutionStatesEnum.TCCConfigurationError,
        //      string.Format("@Unable to locate folder structure from TCC for {0}", "whatever")));
        //}
        //// todo find the folder necessary - will there only be 1 for super user?
        //var thePath = dirResult.entries[0].entryName;
        //var fileStream = new FileStream("appsettings.json", FileMode.Open);
        //var ccPutFileResult = await fileRepo.PutFile(org, thePath, file.flowFilename, fileStream, fileStream.Length)
        //  .ConfigureAwait(false);

        //if (ccPutFileResult?.success == "true")
        //{
        //  throw new ServiceException(HttpStatusCode.InternalServerError,
        //    new ContractExecutionResult(ContractExecutionStatesEnum.TCCConfigurationError,
        //      "@Unable to put file to TCC"));
        //}

        //// todo determine the TCC file description needed to a) store in DB and b) sent to raptorWebAPI for Raptor
        //// store in db as json including 3 parts. How to send to RaptorWebAPI? Have ProjectMDM model with 3 parts
        //var TCCPath = ccPutFileResult.path;

        //// todo is this allowed/required?
        //var filePath = Path.Combine(file.path, file.flowFilename);
        //System.IO.File.Delete(filePath);


        //// todo write new CreateProjectFileImportEvent to kafka que

        ////var response = new ImportedFileDescriptorSingleResult(
        ////      AutoMapperUtility.Automapper.Map<ImportedFileDescriptor>
        ////          (await GetImportedFileList(projectUid.ToString()).ConfigureAwait(false))
        ////          //.Where(p => p.Name == file.flowFilename).FirstOrDefault()
        ////  );
        ////  log.LogInformation(
        ////      $"CreateImportedFileV4. completed succesfully. Response: {JsonConvert.SerializeObject(response)}");
        ////  return response;
        return null;
      }

      //// PUT: api/v4/importedfile
        ///// <summary>
        ///// Update imported file
        ///// </summary>
        ///// <param name="importedFileRequest">UpdateImportedFileRequest model</param>
        ///// <remarks>Updates existing imported file</remarks>
        ///// <response code="200">Ok</response>
        ///// <response code="400">Bad request</response>
        //[Route("api/v4/importedfile")]
        //[HttpPut]
        //public async Task<ImportedFileDescriptorSingleResult> UpdateImportedFileV4(
        //    [FromBody] UpdateImportedFileRequest importedFileRequest)
        //{
        //    if (importedFileRequest == null)
        //    {
        //        throw new ServiceException(HttpStatusCode.InternalServerError,
        //            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
        //                "Missing imported file request"));
        //    }
        //    log.LogInformation(
        //        $"UpdateImportedFileV4. importedFileRequest: {JsonConvert.SerializeObject(importedFileRequest)}");

        //    // todo

        //    var response = new ImportedFileDescriptorSingleResult(
        //        AutoMapperUtility.Automapper.Map<ImportedFileDescriptor>
        //            (await GetImportedFileList(importedFileRequest.ProjectUID.ToString()).ConfigureAwait(false))
        //        //.ToImmutableList()
        //        //.FirstOrDefault(p => p.Name == importedFileRequest.Name)
        //    );
        //    log.LogInformation(
        //        $"CreateImportedFileV4. completed succesfully. Response: {JsonConvert.SerializeObject(response)}");
        //    return response;
        //}


        #region private

        /// <summary>
        /// Gets the imported file list for a project
        /// </summary>
        /// <returns></returns>
        private async Task<ImmutableList<ImportedFileDescriptor>> GetImportedFileList(string projectUid)
        {
            var customerUid = ((this.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
            log.LogInformation("CustomerUID=" + customerUid + " and user=" + User + " and projectUid=" + projectUid);
            var importedFiles = (await projectService.GetImportedFiles(projectUid).ConfigureAwait(false))
                .ToImmutableList();

            log.LogInformation($"ImportedFile list contains {importedFiles.Count()} importedFiles");

            var importedFileList = importedFiles.Select(importedFile => new ImportedFileDescriptor()
                {
                    ProjectUid = importedFile.ProjectUid,
                    ImportedFileUid = importedFile.ImportedFileUid,
                    CustomerUid = importedFile.CustomerUid,
                    ImportedFileType = importedFile.ImportedFileType,
                    Name = importedFile.Name,
                    SurveyedUtc = importedFile.SurveyedUtc
                })
                .ToImmutableList();

            return importedFileList;
        }

    #endregion private
  }
}

