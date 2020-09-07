using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Project.WebAPI.Common.Helpers;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Project.WebAPI.Factories;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

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
    public FileImportV5TBCController(IConfigurationStore config,
                                     ITransferProxyFactory transferProxyFactory,
                                     IFilterServiceProxy filterServiceProxy, ITRexImportFileProxy tRexImportFileProxy,
                                     IRequestFactory requestFactory)
      : base(config, transferProxyFactory, filterServiceProxy, tRexImportFileProxy, requestFactory)
    { }

    /// <summary>
    /// Called by TBC only.
    /// Upsert imported file
    ///   1) TBC will already have uploaded to TCC, so read it from there
    ///   2) creates/updates database 
    ///   3) copies file in TCC from VSS area to project 
    ///   4) notify TRex WebAPI.
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
    [Route("api/v2/projects/{projectId}/importedfiles")] // TBC has route hardcoded
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

      // this also validates that this customer has access to the projectUid
      var project = await ProjectRequestHelper.GetProjectForCustomer(new Guid(CustomerUid), new Guid(UserId), projectId, Logger, ServiceExceptionHandler, CwsProjectClient, customHeaders);
      var projectUid = project.ProjectId;

      importedFileTbc = FileImportV5TBCDataValidator.ValidateUpsertImportedFileRequest(new Guid(projectUid), importedFileTbc);
      Logger.LogInformation(
        $"{nameof(UpsertImportedFileV5TBC)}: projectId {projectId} projectUid {projectUid} importedFile: {JsonConvert.SerializeObject(importedFileTbc)}");
      
      var fileEntry = await TccHelper.GetFileInfoFromTccRepository(importedFileTbc, Logger, ServiceExceptionHandler, FileRepo);

      await TccHelper.CopyFileWithinTccRepository(importedFileTbc,
        CustomerUid, projectUid, FileSpaceId,
        Logger, ServiceExceptionHandler, FileRepo).ConfigureAwait(false);

      ImportedFileDescriptorSingleResult importedFileResult;
      using (var ms = await TccHelper.GetFileStreamFromTcc(importedFileTbc, Logger, ServiceExceptionHandler, FileRepo))
      {
        importedFileResult = await UpsertFileInternal(importedFileTbc.Name, ms, new Guid(projectUid), 
          importedFileTbc.ImportedFileTypeId,
          importedFileTbc.ImportedFileTypeId == ImportedFileType.Linework
            ? importedFileTbc.LineworkFile.DxfUnitsTypeId
            : DxfUnitsType.Meters,
          fileEntry.createTime, fileEntry.modifyTime,
          importedFileTbc.ImportedFileTypeId == ImportedFileType.SurveyedSurface
            ? importedFileTbc.SurfaceFile.SurveyedUtc
            : (DateTime?)null, schedulerProxy);
      }

      // Automapper maps src.ImportedFileId to LegacyFileId, so this IS the one sent to TRex and used to ref via TCC
      var response = importedFileResult != null
        ? ReturnLongV5Result.CreateLongV5Result(HttpStatusCode.OK, importedFileResult.ImportedFileDescriptor.LegacyFileId)
        : ReturnLongV5Result.CreateLongV5Result(HttpStatusCode.InternalServerError, -1);

      Logger.LogInformation(
        $"{nameof(UpsertImportedFileV5TBC)}: Completed successfully. Response: {response} importedFile: {JsonConvert.SerializeObject(importedFileResult)}");

      return response;
    }


    /// <summary>
    /// Called by TBC only.
    /// Get imported files.
    /// This is the same as V6 but TBC URL cannot be changed hence the V5 version.
    /// </summary>
    [Route("api/v5/projects/{projectId}/importedfiles/{id?}")]
    [Route("api/v2/projects/{projectId}/importedfiles/{id?}")] // TBC has route hardcoded
    [HttpGet]
    public async Task<ImmutableList<DesignDetailV5Result>> GetImportedFilesV5TBC([FromRoute] long projectId, [FromRoute] long? id = null)
    {
      Logger.LogInformation($"{nameof(GetImportedFilesV5TBC)}: projectId {projectId} id {id}");

      // this also validates that this customer has access to the projectUid
      var project = await ProjectRequestHelper.GetProjectForCustomer(new Guid(CustomerUid), new Guid(UserId), projectId, Logger, ServiceExceptionHandler, CwsProjectClient, customHeaders);
      var projectUid = project.ProjectId;

      var files = await ImportedFileRequestDatabaseHelper.GetImportedFileList(projectUid, Logger, UserId, ProjectRepo)
        .ConfigureAwait(false);

      var selected = id.HasValue ? files.Where(x => x.LegacyFileId == id.Value) : files;
      return selected.Select(x => new DesignDetailV5Result { id = x.LegacyFileId, name = x.Name, fileType = (int)x.ImportedFileType, insertUTC = x.ImportedUtc }).ToImmutableList();
    }
  }
}
