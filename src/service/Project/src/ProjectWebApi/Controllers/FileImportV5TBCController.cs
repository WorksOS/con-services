using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.DataOcean.Client;
using VSS.MasterData.Project.WebAPI.Common.Executors;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Project.WebAPI.Factories;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// File Import controller v5 for TBC CGen interface
  /// </summary>
  public class FileImportV5TBCController : FileImportBaseController
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public FileImportV5TBCController(IConfigurationStore configStore, Func<TransferProxyType, ITransferProxy> persistantTransferProxy,
      IFilterServiceProxy filterServiceProxy, ITRexImportFileProxy tRexImportFileProxy,
      IRequestFactory requestFactory)
      : base(configStore, persistantTransferProxy, filterServiceProxy, tRexImportFileProxy, requestFactory)
    { }

    // PUT: api/v5/projects/{id}/importedfiles
    /// <summary>
    /// TBC Upsert imported file
    ///   1) TBC will already have uploaded to TCC, so read it from there
    ///   2) creates/updates database 
    ///   3) copies file in TCC from VSS area to project 
    ///   4) notify RaptorWebAPI.
    ///   5) Note that MobileLinework imports are ignored, i.e. just return HttpStatusCode.OK 
    /// Footprint must remain the same as CGen:
    ///   PUT /t/trimble.com/vss-projectmonitoring/1.0/api/v5/projects/6960/importedfiles HTTP/1.1
    ///   Body: {"ImportedFileTypeID":1,"AlignmentFile":null,"SurfaceFile":{"SurveyedUTC":"2018-03-21T20:18:13.9631923Z"},"LineworkFile":null,"MassHaulPlanFile":null,"FileSpaceID":"u927f3be6-7987-4944-898f-42a088da94f2","Path":"/BC Data/Sites/Test  Create/Designs/TBC","Name":"Cell 9 inter 092717 switchback 112917.ttm","CreatedUTC":"2018-04-11T00:22:11.0266872Z"}
    ///   Response: HttpStatusCode.OK
    ///            {"id":6964} 
    ///   This US only handles happy path. ServiceExceptions will be mapped in a future US.
    /// </summary>
    /// <remarks>Updates and Imported design file for a project</remarks>
    /// <response code="200">Ok</response>
    [Route("api/v5/projects/{projectId}/importedfiles")]
    [HttpPut]
    public async Task<ReturnLongV5Result> UpsertImportedFileV5TBC(
      [FromRoute] long projectId,
      [FromBody] ImportedFileTbc importedFileTbc,
      [FromServices] ISchedulerProxy schedulerProxy)
    {
      // MobileLinework .kml/.kmz files are sent along with linework files
      //     we need to suppress any error and return as if all ok.
      //     however we won't have a LegacyFileId to return - hmmm hope Business centre ignores this
      if (importedFileTbc.ImportedFileTypeId == ImportedFileType.MobileLinework)
      {
        Logger.LogInformation(
          $"{nameof(UpsertImportedFileV5TBC)}: Ignore MobileLinework from BusinessCentre. projectId {projectId} importedFile: {JsonConvert.SerializeObject(importedFileTbc)}");

        return ReturnLongV5Result.CreateLongV5Result(HttpStatusCode.OK, -1);
      }

      importedFileTbc = FileImportV5TBCDataValidator.ValidateUpsertImportedFileRequest(projectId, importedFileTbc);
      Logger.LogInformation(
        $"{nameof(UpsertImportedFileV5TBC)}: projectId {projectId} importedFile: {JsonConvert.SerializeObject(importedFileTbc)}");

      ImportedFileUtils.ValidateEnvironmentVariables(importedFileTbc.ImportedFileTypeId, ConfigStore, ServiceExceptionHandler);

      // this also validates that this customer has access to the projectUid
      var project = await GetProject(projectId);

      var fileEntryTask = TccHelper.GetFileInfoFromTccRepository(importedFileTbc, Logger, ServiceExceptionHandler, FileRepo);

      var fileDescriptor = await TccHelper.CopyFileWithinTccRepository(importedFileTbc,
        customerUid, project.ProjectUID, FileSpaceId,
        Logger, ServiceExceptionHandler, FileRepo).ConfigureAwait(false);

      Stream memStream = null;
      try
      {
        // TRex needs a copy of design file in S3. Will BusinessCenter survive until Trex switchover?
        if (UseTrexGatewayDesignImport && IsDesignFileType(importedFileTbc.ImportedFileTypeId))
        {
          memStream = await TccHelper.GetFileStreamFromTcc(importedFileTbc, Logger, ServiceExceptionHandler, FileRepo);

          fileDescriptor = ProjectRequestHelper.WriteFileToS3Repository(
            memStream, project.ProjectUID, importedFileTbc.Name,
            importedFileTbc.ImportedFileTypeId == ImportedFileType.SurveyedSurface,
            importedFileTbc.ImportedFileTypeId == ImportedFileType.SurveyedSurface
              ? importedFileTbc.SurfaceFile.SurveyedUtc
              : (DateTime?)null,
            Logger, ServiceExceptionHandler, persistantTransferProxy);
        }

        var existing = await ImportedFileRequestDatabaseHelper
                             .GetImportedFileForProject
                             (project.ProjectUID, importedFileTbc.Name, importedFileTbc.ImportedFileTypeId, null,
                              Logger, ProjectRepo, 0, null)
                             .ConfigureAwait(false);
        bool creating = existing == null;
        Logger.LogInformation(
          creating
            ? $"{nameof(UpsertImportedFileV5TBC)}: file doesn't exist already in DB: {importedFileTbc.Name} projectUid {project.ProjectUID} ImportedFileType: {importedFileTbc.ImportedFileTypeId}"
            : $"{nameof(UpsertImportedFileV5TBC)}: file exists already in DB. Will be updated: {JsonConvert.SerializeObject(existing)}");

        var importedFileUid = creating ? Guid.NewGuid().ToString() : existing.ImportedFileUid;
        var dataOceanFileName = DataOceanFileUtil.DataOceanFileName(importedFileTbc.Name,
          importedFileTbc.ImportedFileTypeId == ImportedFileType.SurveyedSurface || importedFileTbc.ImportedFileTypeId == ImportedFileType.GeoTiff,
          importedFileUid.ToString(), importedFileTbc.SurfaceFile?.SurveyedUtc);

        if (memStream == null)
        {
          memStream = await TccHelper.GetFileStreamFromTcc(importedFileTbc, Logger, ServiceExceptionHandler, FileRepo);
        }
        await DataOceanHelper.WriteFileToDataOcean(
          memStream, DataOceanRootFolderId, customerUid, project.ProjectUID, dataOceanFileName,
          Logger, ServiceExceptionHandler, DataOceanClient, Authorization, importedFileUid.ToString(), ConfigStore);

        var fileEntry = await fileEntryTask;
        ImportedFileDescriptorSingleResult importedFile;
        if (creating)
        {
          var createImportedFile = new CreateImportedFile(project.ProjectUID, importedFileTbc.Name,
            fileDescriptor,
            importedFileTbc.ImportedFileTypeId,
            importedFileTbc.SurfaceFile?.SurveyedUtc,
            importedFileTbc.ImportedFileTypeId == ImportedFileType.Linework
              ? importedFileTbc.LineworkFile.DxfUnitsTypeId
              : DxfUnitsType.Meters,
            fileEntry.createTime, fileEntry.modifyTime,
            DataOceanRootFolderId, null, 0, importedFileUid, dataOceanFileName);

          importedFile = await WithServiceExceptionTryExecuteAsync(() =>
            RequestExecutorContainerFactory
              .Build<CreateImportedFileExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
                customerUid, userId, userEmailAddress, customHeaders,
                productivity3dV2ProxyNotification: Productivity3dV2ProxyNotification, productivity3dV2ProxyCompaction: Productivity3dV2ProxyCompaction,
                persistantTransferProxy: persistantTransferProxy, tRexImportFileProxy: tRexImportFileProxy,
                projectRepo: ProjectRepo, fileRepo: FileRepo, dataOceanClient: DataOceanClient, authn: Authorization, schedulerProxy: schedulerProxy)
              .ProcessAsync(createImportedFile)
          ) as ImportedFileDescriptorSingleResult;

          Logger.LogInformation(
            $"{nameof(UpsertImportedFileV5TBC)}: Create completed succesfully. Response: {JsonConvert.SerializeObject(importedFile)}");
        }
        else
        {
          var importedFileUpsertEvent = new UpdateImportedFile
          (
            project.ProjectUID, project.ShortRaptorProjectId, importedFileTbc.ImportedFileTypeId,
            importedFileTbc.ImportedFileTypeId == ImportedFileType.SurveyedSurface
              ? importedFileTbc.SurfaceFile.SurveyedUtc
              : (DateTime?)null,
            importedFileTbc.ImportedFileTypeId == ImportedFileType.Linework
              ? importedFileTbc.LineworkFile.DxfUnitsTypeId
              : DxfUnitsType.Meters,
            fileEntry.createTime, fileEntry.modifyTime,
            fileDescriptor, existing.ImportedFileUid, existing.ImportedFileId,
            DataOceanRootFolderId, 0, dataOceanFileName
          );

          importedFile = await WithServiceExceptionTryExecuteAsync(() =>
            RequestExecutorContainerFactory
              .Build<UpdateImportedFileExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
                customerUid, userId, userEmailAddress, customHeaders,
                productivity3dV2ProxyNotification: Productivity3dV2ProxyNotification, productivity3dV2ProxyCompaction: Productivity3dV2ProxyCompaction,
                tRexImportFileProxy: tRexImportFileProxy,
                projectRepo: ProjectRepo, fileRepo: FileRepo, dataOceanClient: DataOceanClient, authn: Authorization, schedulerProxy: schedulerProxy)
              .ProcessAsync(importedFileUpsertEvent)
          ) as ImportedFileDescriptorSingleResult;
        }

        // Automapper maps src.ImportedFileId to LegacyFileId, so this IS the one sent to Raptor and used to ref via TCC
        var response = importedFile?.ImportedFileDescriptor != null
          ? ReturnLongV5Result.CreateLongV5Result(HttpStatusCode.OK, importedFile.ImportedFileDescriptor.LegacyFileId)
          : ReturnLongV5Result.CreateLongV5Result(HttpStatusCode.InternalServerError, -1);

        Logger.LogInformation(
          $"{nameof(UpsertImportedFileV5TBC)}: Completed successfully. Response: {response} importedFile: {JsonConvert.SerializeObject(importedFile)}");

        return response;
      }
      finally
      {
        memStream?.Dispose();

      }
    }


    // GET: api/v5/importedfiles
    /// <summary>
    /// TBC Get imported files.
    /// This is the same as V6 but TBC URL cannot be changed hence the V5 version.
    /// </summary>
    [Route("api/v5/projects/{projectId}/importedfiles/{id?}")]
    [HttpGet]
    public async Task<ImmutableList<DesignDetailV5Result>> GetImportedFilesV5TBC([FromRoute] long projectId, [FromRoute] long? id = null)
    {
      Logger.LogInformation($"{nameof(GetImportedFilesV5TBC): Starting}");

      var project = await GetProject(projectId);

      var files = await ImportedFileRequestDatabaseHelper.GetImportedFileList(project.ProjectUID, Logger, userId, ProjectRepo)
        .ConfigureAwait(false);

      var selected = id.HasValue ? files.Where(x => x.LegacyFileId == id.Value) : files;
      return selected.Select(x => new DesignDetailV5Result { id = x.LegacyFileId, name = x.Name, fileType = (int)x.ImportedFileType, insertUTC = x.ImportedUtc }).ToImmutableList();
    }
  }
}
