using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Controllers;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Helpers;
using VSS.Productivity3D.WebApiModels.Compaction.Executors;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for summary and details requests
  /// </summary>
  //Turn off caching until settings caching problem resolved
  //[ResponseCache(Duration = 180, VaryByQueryKeys = new[] { "*" })]
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
    /// The request factory
    /// </summary>
    private readonly IProductionDataRequestFactory requestFactory;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="raptorClient">The raptor client.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="configStore">Configuration store</param>/// 
    /// <param name="fileListProxy">The file list proxy.</param>
    /// <param name="projectSettingsProxy">The project settings proxy.</param>
    /// <param name="settingsManager">Compaction settings manager</param>
    /// <param name="requestFactory">The request factory.</param>
    /// <param name="exceptionHandler">The exception handler.</param>
    /// <param name="filterServiceProxy">Filter service proxy</param>
    public CompactionProfileController(IASNodeClient raptorClient, ILoggerFactory logger, IConfigurationStore configStore,
      IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy, ICompactionSettingsManager settingsManager,
      IProductionDataRequestFactory requestFactory, IServiceExceptionHandler exceptionHandler, IFilterServiceProxy filterServiceProxy) : 
      base (logger.CreateLogger<BaseController>(),exceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      log = logger.CreateLogger<CompactionProfileController>();
      this.requestFactory = requestFactory;
    }

    /// <summary>
    /// Posts a profile production data request to a Raptor's data model/project.
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="startLatDegrees">Start profileLine Lat</param>
    /// <param name="startLonDegrees">Start profileLine Lon</param>
    /// <param name="endLatDegrees">End profileLine Lat</param>
    /// <param name="endLonDegrees">End profileLine Lon</param>
    /// <param name="filterUid">Filter UID</param>
    /// <param name="cutfillDesignUid">Design UID</param>
    /// <returns>
    /// Returns JSON structure wtih operation result as profile calculations <see cref="ContractExecutionResult"/>
    /// </returns>
    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [Route("api/v2/profiles/productiondata/slicer")]
    [HttpGet]
    public async Task<CompactionProfileResult> GetProfileProductionDataSlicer(
      [FromQuery] Guid projectUid,
      [FromQuery] double startLatDegrees,
      [FromQuery] double startLonDegrees,
      [FromQuery] double endLatDegrees,
      [FromQuery] double endLonDegrees,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? cutfillDesignUid
    )
    {
      log.LogInformation("GetProductionDataSlice: " + Request.QueryString);
      var projectId = GetProjectId(projectUid);

      var slicerProfileResult = requestFactory.Create<CompositeCompositeProfileDataRequestHelper>(async r => r
          .ProjectId(projectId)
          .Headers(customHeaders)
          .ProjectSettings(CompactionProjectSettings.FromString(
            await projectSettingsProxy.GetProjectSettings(projectUid.ToString(), customHeaders)))
          .ExcludedIds(await GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid)))
        .CreateCompositeProfileRequest(projectUid, startLatDegrees, startLonDegrees, endLatDegrees, endLonDegrees,
          filterUid, customerUid, cutfillDesignUid);

      slicerProfileResult.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionProfileExecutor>(logger, raptorClient)
          .Process(slicerProfileResult) as CompactionProfileResult
      );
    }

    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [Route("api/v2/profiles/design")]
    [HttpGet]
    public async Task<CompactionProfileResult> GetProfileProductionDataDesign(
      [FromQuery] Guid projectUid,
      [FromQuery] double startLatDegrees,
      [FromQuery] double startLonDegrees,
      [FromQuery] double endLatDegrees,
      [FromQuery] double endLonDegrees,
      [FromQuery] Guid importedFileUid,
      [FromQuery] Guid? filterUid = null)
    {
      log.LogInformation("GetDesignProfileSlice: " + Request.QueryString);

      var projectId = GetProjectId(projectUid);

      var profileResult = requestFactory.Create<DesignProfileDataRequestHelper>(async r => r
          .ProjectId(projectId)
          .Headers(customHeaders)
          .ProjectSettings(CompactionProjectSettings.FromString(
            await projectSettingsProxy.GetProjectSettings(projectUid.ToString(), customHeaders)))
          .ExcludedIds(await GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid)))
        .SetRaptorClient(raptorClient)
        .CreateDesignProfileRequest(projectUid, startLatDegrees, startLonDegrees, endLatDegrees, endLonDegrees, customerUid, importedFileUid, filterUid);

      profileResult.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionDesignProfileExecutor>(logger, raptorClient)
          .Process(profileResult) as CompactionProfileResult
      );
    }
  }
}