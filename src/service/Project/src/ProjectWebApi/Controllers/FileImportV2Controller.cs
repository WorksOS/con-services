using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Project.WebAPI.Factories;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// File Import controller v2 for TBC CGen interface
  /// </summary>
  public class FileImportV2Controller : FileImportBaseController
  {
    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="producer"></param>
    /// <param name="projectRepo"></param>
    /// <param name="store"></param>
    /// <param name="raptorProxy"></param>
    /// <param name="subscriptionRepo"></param>
    /// <param name="fileRepo"></param>
    /// <param name="logger"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="requestFactory"></param>
    public FileImportV2Controller(IKafka producer,
      IConfigurationStore store, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler,
      IRaptorProxy raptorProxy,
      IProjectRepository projectRepo, ISubscriptionRepository subscriptionRepo,
      IFileRepository fileRepo, IRequestFactory requestFactory)
      : base(producer, store, logger, logger.CreateLogger<FileImportV2Controller>(), serviceExceptionHandler,
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

    // PUT: api/v2/projects/{id}/importedfiles
    /// <summary>
    /// TBC Upsert imported file
    ///   1) TBC will already have uploaded to TCC, so read it from there
    ///   2) creates/updates database 
    ///   3) possibly creates/updates file in TCC?
    ///   4) notify RaptorWebAPI.
    ///   5) Note that MobileLinework imports are ignored, i.e. just return HttpStatusCode.OK 
    /// Footprint must remain the same as CGen:
    ///   PUT /t/trimble.com/vss-projectmonitoring/1.0/api/v2/projects/6960/importedfiles HTTP/1.1
    ///   Body: {"ImportedFileTypeID":1,"AlignmentFile":null,"SurfaceFile":{"SurveyedUTC":"2018-03-21T20:18:13.9631923Z"},"LineworkFile":null,"MassHaulPlanFile":null,"FileSpaceID":"u927f3be6-7987-4944-898f-42a088da94f2","Path":"/BC Data/Sites/Test  Create/Designs/TBC","Name":"Cell 9 inter 092717 switchback 112917.ttm","CreatedUTC":"2018-04-11T00:22:11.0266872Z"}
    ///   Response: HttpStatusCode.OK
    ///            {"id":6964} 
    ///   This US only handles happy path. ServiceExceptions will be mapped in a future US.
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="importedFileTbc"></param>
    /// <remarks>Updates and Imported design file for a project</remarks>
    /// <response code="200">Ok</response>
    [Route("api/v2/projects/{projectId}/importedfiles")]
    [HttpPut]
    public async Task<ReturnLongV2Result> UpsertImportedFileV2(
      [FromRoute] long projectId,
      [FromBody] ImportedFileTbc importedFileTbc)
    {
      // MobileLinework .kml/.kmz files are sent along with linework files
      //     we need to suppress any error and return as if all ok.
      //     however we won't have a LegacyFileId to return - hmmm hope Business centre ignores this
      if (importedFileTbc.ImportedFileTypeId == ImportedFileType.MobileLinework)
      {
        log.LogInformation(
          $"UpsertImportedFileV2. Ignore MobileLinework from BusinessCentre. projectId {projectId} importedFile: {JsonConvert.SerializeObject(importedFileTbc)}");

        return ReturnLongV2Result.CreateLongV2Result(HttpStatusCode.OK, -1);
      }

      importedFileTbc = FileImportV2DataValidator.ValidateUpsertImportedFileRequest(projectId, importedFileTbc);
      log.LogInformation(
        $"UpsertImportedFileV2. projectId {projectId} importedFile: {JsonConvert.SerializeObject(importedFileTbc)}");

      // this also validates that this customer has access to the projectUid
      var project = await GetProject(projectId);

      var fileEntry = await ImportedFileRequestHelper.GetFileInfoFromTccRepository(importedFileTbc,
        fileSpaceId, log, serviceExceptionHandler, fileRepo).ConfigureAwait(false);

      var fileDescriptor = await ImportedFileRequestHelper.CopyFileWithinTccRepository(importedFileTbc,
        customerUid, project.ProjectUID, fileSpaceId,
        log, serviceExceptionHandler, fileRepo).ConfigureAwait(false);

      var importedFileUpsertEvent = ImportedFileUpsertEvent.CreateImportedFileUpsertEvent
      (
        project, importedFileTbc.ImportedFileTypeId,
        importedFileTbc.ImportedFileTypeId == ImportedFileType.SurveyedSurface
          ? importedFileTbc.SurfaceFile.SurveyedUtc
          : (DateTime?) null,
        importedFileTbc.ImportedFileTypeId == ImportedFileType.Linework
          ? importedFileTbc.LineworkFile.DxfUnitsTypeId
          : DxfUnitsType.Meters,
        fileEntry.createTime, fileEntry.modifyTime,
        fileDescriptor
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

      // Automapper maps src.ImportedFileId to LegacyFileId, so this IS the one sent to Raptor and used to ref via TCC
      var response = importedFile?.ImportedFileDescriptor != null 
        ? ReturnLongV2Result.CreateLongV2Result(HttpStatusCode.OK, importedFile.ImportedFileDescriptor.LegacyFileId) 
        : ReturnLongV2Result.CreateLongV2Result(HttpStatusCode.InternalServerError, -1);

      log.LogInformation(
        $"UpsertImportedFileV2. Completed succesfully. Response: {response} importedFile: {JsonConvert.SerializeObject(importedFile)}");
      
      return response;
    }

    // GET: api/v2/importedfiles
    /// <summary>
    /// TBC Get imported files.
    /// This is the same as V4 but TBC URL cannot be changed hence the V2 version.
    /// </summary>
    [Route("api/v2/projects/{projectId}/importedfiles/{id?}")]
    [HttpGet]
    public async Task<ImmutableList<DesignDetailV2Result>> GetImportedFilesV2([FromRoute] long projectId, [FromRoute] long? id = null)
    {
      log.LogInformation("GetImportedFilesV2");

      var project = await GetProject(projectId);

      var files = await ImportedFileRequestHelper.GetImportedFileList(project.ProjectUID, log, userId, projectRepo)
        .ConfigureAwait(false);

      var selected = id.HasValue ? files.Where(x => x.LegacyFileId == id.Value) : files;
      return selected.Select(x => new DesignDetailV2Result{id = x.LegacyFileId, name = x.Name,  fileType = (int)x.ImportedFileType, insertUTC = x.ImportedUtc}).ToImmutableList();
      
    }
  }
}