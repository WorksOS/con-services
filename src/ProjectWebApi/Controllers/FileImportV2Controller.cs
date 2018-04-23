using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Repositories;
using VSS.TCCFileAccess;
using VSS.MasterData.Project.WebAPI.Common.Internal;
using VSS.MasterData.Project.WebAPI.Factories;
using VSS.MasterData.Proxies.Interfaces;
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

    // PUT: api/v2/projects/{id}/importedfiles
    /// <summary>
    /// TBC Upsert imported file
    ///   1) TBC will already have uploaded to TCC, so read it from there
    ///   2) creates/updates database 
    ///   3) possibly creates/updates file in TCC?
    ///   4) notify RaptorWebAPI.
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
    public async Task<ContractExecutionResult> UpsertImportedFileV2(
      [FromUri] long projectId,
      [FromUri] ImportedFileTbc importedFileTbc)
    {
      importedFileTbc = FileImportV2DataValidator.ValidateUpsertImportedFileRequest(projectId, importedFileTbc);
      log.LogInformation(
        $"UpsertImportedFileV2. projectId {projectId} importedFile: {JsonConvert.SerializeObject(importedFileTbc)}");

      // this also validates that this customer has access to the projectUid
      var project = await GetProject(projectId);

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
        DateTime.UtcNow, DateTime.UtcNow, // todo what should these be?
        fileDescriptor
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

      log.LogInformation(
        $"UpsertImportedFileV2. Completed succesfully. Response: {JsonConvert.SerializeObject(importedFile)}");

      // todo is it LegacyFilId or ImportedFileId identifying the file in raptor/tcc?
      return ReturnLongV2Result.CreateLongV2Result(HttpStatusCode.OK, importedFile.ImportedFileDescriptor.LegacyFileId);
    }
  }
}