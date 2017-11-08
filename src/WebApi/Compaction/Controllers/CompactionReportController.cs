using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VSS.Productivity3D.Common.Interfaces;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.WebApi.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for report requests
  /// </summary>
  /// 
  [ResponseCache(Duration = 180, VaryByQueryKeys = new[] { "*" })]
  public class CompactionReportController : BaseController 
  {
    #region privates

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
/*
    /// <summary>
    /// For retrieving user preferences
    /// </summary>
    private readonly IPreferenceProxy prefProxy;
*/
    /// <summary>
    /// The request factory
    /// </summary>
    private readonly IProductionDataRequestFactory requestFactory;

    #endregion

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="raptorClient">The raptor client</param>
    /// <param name="logger">The logger.</param>
    /// <param name="exceptionHandler">The exception handler.</param>
    /// <param name="configStore">Configuration store.</param>
    /// <param name="fileListProxy">The file list proxy.</param>
    /// <param name="projectSettingsProxy">The project settings proxy.</param>
    /// <param name="filterServiceProxy">The filter service proxy.</param>
    /// <param name="settingsManager">The compaction settings manager.</param>
    /// <param name="requestFactory">The request factory.</param>
    public CompactionReportController(IASNodeClient raptorClient, ILoggerFactory logger, IServiceExceptionHandler exceptionHandler, IConfigurationStore configStore,
      IFileListProxy fileListProxy, IProjectSettingsProxy projectSettingsProxy, IFilterServiceProxy filterServiceProxy, ICompactionSettingsManager settingsManager,
      IProductionDataRequestFactory requestFactory) : 
      base(logger.CreateLogger<BaseController>(), exceptionHandler, configStore, fileListProxy, projectSettingsProxy, filterServiceProxy, settingsManager)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      log = logger.CreateLogger<CompactionReportController>();
      this.requestFactory = requestFactory;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <param name="filterUid">The filter UID to apply to the report results</param>
    /// <param name="reportElevation">Exclude/include Elevation data in the report.</param>
    /// <param name="reportCMV">Exclude/include CMV data in the report.</param>
    /// <param name="reportMDP">Exclude/include MDP data in the report.</param>
    /// <param name="reportPassCount">Exclude/include Pass Count data in the report.</param>
    /// <param name="reportTemperature">Exclude/include Temperature data in the report.</param>
    /// <param name="reportCutFill">Exclude/include Cut/Fill data in the report.</param>
    /// <param name="cutfillDesignUid">The cut/fill design file unique identifier if Cut/Fill data is included in the report.</param>
    /// <param name="gridInterval">The grif spacing interval for the sampled points.</param>
    /// <param name="gridReportOption">Grid report option. Whether it is defined automatically or by user specified parameters.</param>
    /// <param name="startNorthing">The Northing ordinate of the location to start gridding from.</param>
    /// <param name="startEasting">The Easting ordinate of the location to start gridding from.</param>
    /// <param name="endNorthing">The Northing ordinate of the location to end gridding at.</param>
    /// <param name="endEasting">The Easting ordinate of the location to end gridding at.</param>
    /// <param name="azimuth">The orientation of the grid, expressed in radians</param>
    /// <returns>An instance of the ContractExecutionResult class.</returns>
    /// 
    [Route("api/v2/report/grid")]
    [HttpGet]
    public async Task<CompactionReportGridResult> GetReporGrid(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] bool reportElevation,
      [FromQuery] bool reportCMV,
      [FromQuery] bool reportMDP,
      [FromQuery] bool reportPassCount,
      [FromQuery] bool reportTemperature,
      [FromQuery] bool reportCutFill,
      [FromQuery] Guid? cutfillDesignUid,
      [FromQuery] double? gridInterval,
      [FromQuery] GridReportOption gridReportOption,
      [FromQuery] double startNorthing,
      [FromQuery] double startEasting,
      [FromQuery] double endNorthing,
      [FromQuery] double endEasting,
      [FromQuery] double azimuth)
    {
      log.LogInformation("GetReporGrid: " + Request.QueryString);

      var projectId = GetProjectId(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var cutFillDesign = await GetDesignDescriptor(projectUid, cutfillDesignUid, true);
      var projectSettings = await GetProjectSettings(projectUid);
      //var userPreferences = await GetUserPreferences();

      var reportGridRequest = await requestFactory.Create<CompactionReportGridRequestHelper>(r => r
        .ProjectId(projectId)
        .Headers(CustomHeaders)
        .ProjectSettings(projectSettings)
        .Filter(filter))
      .SetRaptorClient(raptorClient)
      .CreateCompactionReportGridRequest(
        reportElevation,
        reportCMV,
        reportMDP,
        reportPassCount,
        reportTemperature,
        reportCutFill,
        cutFillDesign,
        gridInterval,
        gridReportOption,
        startNorthing,
        startEasting,
        endNorthing,
        endEasting,
        azimuth
      );

      reportGridRequest.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionReportGridExecutor>(logger, raptorClient, null, ConfigStore)
          .Process(reportGridRequest) as CompactionReportGridResult
      );
    }
/*
    /// <summary>
    /// Get user preferences
    /// </summary>
    /// <returns></returns>
    private async Task<UserPreferenceData> GetUserPreferences()
    {
      var userPreferences = await prefProxy.GetUserPreferences(customHeaders);
      if (userPreferences == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Failed to retrieve preferences for current user"));
      }
      return userPreferences;
    }
*/
  }
}
