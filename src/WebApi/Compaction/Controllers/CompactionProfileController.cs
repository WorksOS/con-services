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
using VSS.Productivity3D.WebApi.Models.Compaction.Interfaces;
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
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
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
    public async Task<CompactionProfileResult<CompactionProfileCell>> GetProfileProductionDataSlicer(
      [FromServices] ICompactionProfileResultHelper profileResultHelper,
      [FromQuery] Guid projectUid,
      [FromQuery] double startLatDegrees,
      [FromQuery] double startLonDegrees,
      [FromQuery] double endLatDegrees,
      [FromQuery] double endLonDegrees,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? cutfillDesignUid)
    {
      log.LogInformation("GetProfileProductionDataSlicer: " + Request.QueryString);
      var projectId = GetProjectId(projectUid);

      var settings = CompactionProjectSettings.FromString(
        await projectSettingsProxy.GetProjectSettings(projectUid.ToString(), customHeaders));
      var exludedIds = await GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid);

      //Get production data profile
      var slicerProductionDataProfileRequest = requestFactory.Create<ProductionDataProfileRequestHelper>(r => r
          .ProjectId(projectId)
          .Headers(customHeaders)
          .ProjectSettings(settings)
          .ExcludedIds(exludedIds))
          .CreateProductionDataProfileRequest(
            projectUid, startLatDegrees, startLonDegrees, endLatDegrees, endLonDegrees, filterUid, customerUid, cutfillDesignUid);

      slicerProductionDataProfileRequest.Validate();

      var slicerProductionDataResult = WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionProfileExecutor<CompactionProfileCell>>(logger, raptorClient)
          .Process(slicerProductionDataProfileRequest) as CompactionProfileResult<CompactionProfileCell>
      );

      if (cutfillDesignUid.HasValue)
      {
        //Get design profile
        var slicerDesignProfileRequest = requestFactory.Create<DesignProfileRequestHelper>(r => r
            .ProjectId(projectId)
            .Headers(customHeaders)
            .ProjectSettings(settings)
            .ExcludedIds(exludedIds))
            .SetRaptorClient(raptorClient)
            .CreateDesignProfileRequest(
              projectUid, startLatDegrees, startLonDegrees, endLatDegrees, endLonDegrees, customerUid, cutfillDesignUid.Value, filterUid);

        slicerDesignProfileRequest.Validate();

        var slicerDesignResult = WithServiceExceptionTryExecute(() =>
          RequestExecutorContainerFactory
            .Build<CompactionDesignProfileExecutor<CompactionProfileVertex>>(logger, raptorClient)
            .Process(slicerDesignProfileRequest) as CompactionProfileResult<CompactionProfileVertex>
        );

        //Find the cut-fill elevations for the cell stations from the design vertex elevations
        profileResultHelper.FindCutFillElevations(slicerProductionDataResult, slicerDesignResult);
      }
      return slicerProductionDataResult;
    }

    [ProjectUidVerifier]
    [NotLandFillProjectWithUIDVerifier]
    [Route("api/v2/profiles/design/slicer")]
    [HttpGet]
    public async Task<CompactionProfileResult<CompactionProfileVertex>> GetProfileDesignSlicer(
      [FromQuery] Guid projectUid,
      [FromQuery] double startLatDegrees,
      [FromQuery] double startLonDegrees,
      [FromQuery] double endLatDegrees,
      [FromQuery] double endLonDegrees,
      [FromQuery] Guid importedFileUid,
      [FromQuery] Guid? filterUid = null)
    {
      log.LogInformation("GetProfileDesignSlicer: " + Request.QueryString);

      var projectId = GetProjectId(projectUid);

      var profileRequest = requestFactory.Create<DesignProfileRequestHelper>(async r => r
          .ProjectId(projectId)
          .Headers(customHeaders)
          .ProjectSettings(CompactionProjectSettings.FromString(
            await projectSettingsProxy.GetProjectSettings(projectUid.ToString(), customHeaders)))
          .ExcludedIds(await GetExcludedSurveyedSurfaceIds(fileListProxy, projectUid)))
        .SetRaptorClient(raptorClient)
        .CreateDesignProfileRequest(projectUid, startLatDegrees, startLonDegrees, endLatDegrees, endLonDegrees, customerUid, importedFileUid, filterUid);

      profileRequest.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionDesignProfileExecutor<CompactionProfileVertex>>(logger, raptorClient)
          .Process(profileRequest) as CompactionProfileResult<CompactionProfileVertex>
      );
    }

 
  }
}