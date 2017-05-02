using System;
using System.Collections.Generic;
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
using NodaTime.Extensions;
using ProjectWebApiCommon.Models;
using ProjectWebApiCommon.ResultsHandling;
using ProjectWebApiCommon.Utilities;
using Repositories;
using VSS.GenericConfiguration;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSP.MasterData.Project.WebAPI.Controllers.V4
{
    public class FileImportV4Controller : ProjectBaseController
    {
        public FileImportV4Controller(IKafka producer, IRepository<IProjectEvent> projectRepo,
            IRepository<ISubscriptionEvent> subscriptionsRepo, IConfigurationStore store, ISubscriptionProxy subsProxy,
            IGeofenceProxy geofenceProxy, IRaptorProxy raptorProxy, ILoggerFactory logger)
            : base(producer, projectRepo, subscriptionsRepo, store, subsProxy, geofenceProxy, raptorProxy, logger)
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
        ///    this creates a link between the file and project AND sends file to TCC/Raptor
        /// </summary>
        /// <param name="importedFileRequest">CreateImportedFileRequest model</param>
        /// <remarks>Import a design file for a project</remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>
        [Route("api/v4/importedfile")]
        [HttpPost]
        [ActionName("Upload")]
        [FlowUpload("svl")]
        public async Task<ImportedFileDescriptorSingleResult> CreateImportedFileV4(FlowFile file, [FromUri] DateTime? surveyedSurfaceTime = null) //and other fields
        {
            if (file == null)
            {
                throw new ServiceException(HttpStatusCode.InternalServerError,
                    new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                        "Missing imported file request"));
            }
            log.LogInformation(
                $"CreateImportedFileV4. importedFileRequest: {JsonConvert.SerializeObject(file)}");

            // todo

          /*  var response = new ImportedFileDescriptorSingleResult(
                AutoMapperUtility.Automapper.Map<ImportedFileDescriptor>
                    (await GetImportedFileList(importedFileRequest.ProjectUID.ToString()).ConfigureAwait(false))
                //.ToImmutableList()
                //.FirstOrDefault(p => p.Name == importedFileRequest.Name)
            );
            log.LogInformation(
                $"CreateImportedFileV4. completed succesfully. Response: {JsonConvert.SerializeObject(response)}");
            return response;*/
            return null;
        }

        // PUT: api/v4/importedfile
        /// <summary>
        /// Update imported file
        /// </summary>
        /// <param name="importedFileRequest">UpdateImportedFileRequest model</param>
        /// <remarks>Updates existing imported file</remarks>
        /// <response code="200">Ok</response>
        /// <response code="400">Bad request</response>
        [Route("api/v4/importedfile")]
        [HttpPut]
        public async Task<ImportedFileDescriptorSingleResult> UpdateImportedFileV4(
            [FromBody] UpdateImportedFileRequest importedFileRequest)
        {
            if (importedFileRequest == null)
            {
                throw new ServiceException(HttpStatusCode.InternalServerError,
                    new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                        "Missing imported file request"));
            }
            log.LogInformation(
                $"UpdateImportedFileV4. importedFileRequest: {JsonConvert.SerializeObject(importedFileRequest)}");

            // todo

            var response = new ImportedFileDescriptorSingleResult(
                AutoMapperUtility.Automapper.Map<ImportedFileDescriptor>
                    (await GetImportedFileList(importedFileRequest.ProjectUID.ToString()).ConfigureAwait(false))
                //.ToImmutableList()
                //.FirstOrDefault(p => p.Name == importedFileRequest.Name)
            );
            log.LogInformation(
                $"CreateImportedFileV4. completed succesfully. Response: {JsonConvert.SerializeObject(response)}");
            return response;
        }


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

