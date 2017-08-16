using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VSS.Common.ResultsHandling;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Controllers;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Helpers;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.Productivity3D.WebApi.ProductionData.Controllers;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for summary and details requests
  /// </summary>
  [ResponseCache(Duration = 180, VaryByQueryKeys = new[] { "*" })]
  public class CompactionProfileController : BaseController
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// For getting list of imported files for a project
    /// </summary>
    private readonly IFileListProxy fileListProxy;

    /// <summary>
    /// For getting project settings for a project
    /// </summary>
    private readonly IProjectSettingsProxy projectSettingsProxy;

    /// <summary>
    /// The request factory
    /// </summary>
    private readonly IProductionDataRequestFactory requestFactory;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="raptorClient">The raptor client.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="fileListProxy">The file list proxy.</param>
    /// <param name="projectSettingsProxy">The project settings proxy.</param>
    /// <param name="requestFactory">The request factory.</param>
    /// <param name="exceptionHandler">The exception handler.</param>
    public CompactionProfileController(IASNodeClient raptorClient, ILoggerFactory logger,
      IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy,
      IProductionDataRequestFactory requestFactory, IServiceExceptionHandler exceptionHandler) : base(logger.CreateLogger<BaseController>(), exceptionHandler)
    {
      this.logger = logger;
      log = logger.CreateLogger<ProfileProductionDataController>();
      this.fileListProxy = fileListProxy;
      this.projectSettingsProxy = projectSettingsProxy;
      this.requestFactory = requestFactory;
      this.raptorClient = raptorClient;
    }

    /// <summary>
    /// Posts a profile production data request to a Raptor's data model/project.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startLatDegrees">Start profileLine Lat</param>
    /// <param name="startLonDegrees">Start profileLine Lon</param>
    /// <param name="endLatDegrees">End profileLine Lat</param>
    /// <param name="endLonDegrees">End profileLine Lon</param>
    /// <param name="filterUid">Filter Id</param>
    /// <param name="cutfillDesignUid">Design UID</param>
    /// <returns>
    /// Returns JSON structure wtih operation result as profile calculations <see cref="ContractExecutionResult"/>
    /// </returns>
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [Route("api/v2/profiles/productiondata/slicer")]
    [HttpGet]
    public async Task<ProfileResult> GetProfileProductionDataSlicer(
      [FromQuery] Guid projectUid,
      [FromQuery] double startLatDegrees,
      [FromQuery] double startLonDegrees,
      [FromQuery] double endLatDegrees,
      [FromQuery] double endLonDegrees,
      [FromQuery] Guid filterUid,
      [FromQuery] Guid? cutfillDesignUid
    )
    {
      log.LogInformation("GetProfileProduction: " + Request.QueryString);
      var projectId = GetProjectId(projectUid);

      var slicerProfileResult = requestFactory.Create<SliceProfileDataRequestHelper>(async r => r
          .ProjectId(projectId)
          .Headers(customHeaders)
          .ProjectSettings(CompactionProjectSettings.FromString(
            await projectSettingsProxy.GetProjectSettings(projectUid.ToString(), customHeaders)))
          .ExcludedIds(await GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid)))
        .CreateSlicerProfileRequest(projectUid, startLatDegrees, startLonDegrees, endLatDegrees, endLonDegrees,
          filterUid, customerUid, cutfillDesignUid);

      slicerProfileResult.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<ProfileProductionDataExecutor>(logger, raptorClient)
          .Process(slicerProfileResult) as ProfileResult
      );
    }

    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier] // Is this required?
    [Route("api/v2/profiles/productiondata/design")]
    [HttpGet]
    public async Task<ProfileResult> GetProfileProductionDataDesign(
      [FromQuery] Guid projectUid,
      [FromQuery] double startLatDegrees,
      [FromQuery] double startLonDegrees,
      [FromQuery] double endLatDegrees,
      [FromQuery] double endLonDegrees,
      [FromQuery] Guid importedFileUid,
      [FromQuery] int importedFileTypeId,
      [FromQuery] Guid filterUid,
      [FromQuery] Guid? cutfillDesignUid)
    {
      log.LogInformation("GetDesignProduction: " + Request.QueryString);

      var projectId = GetProjectId(projectUid);

      var profileResult = requestFactory.Create<DesignProfileDataRequestHelper>(async r => r
          .ProjectId(projectId)
          .Headers(customHeaders)
          .ProjectSettings(CompactionProjectSettings.FromString(
            await projectSettingsProxy.GetProjectSettings(projectUid.ToString(), customHeaders)))
          .ExcludedIds(await GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid)))
        .SetRaptorClient(raptorClient)
        .CreateDesignProfileResponse(projectUid, startLatDegrees, startLonDegrees, endLatDegrees, endLonDegrees, customerUid, importedFileUid, importedFileTypeId, filterUid, cutfillDesignUid);

      profileResult.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<DesignProfileProductionDataExecutor>(logger, raptorClient)
          .Process(profileResult) as ProfileResult
      );
    }
  }
}