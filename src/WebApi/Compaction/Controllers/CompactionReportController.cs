using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.Models.Reports;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Controller for getting Raptor production data for report requests
  /// </summary>
  [ResponseCache(Duration = 900, VaryByQueryKeys = new[] { "*" })]
  public class CompactionReportController : BaseController<CompactionReportController>
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// The request factory
    /// </summary>
    private readonly IProductionDataRequestFactory requestFactory;

    /// <summary>
    /// For retrieving user preferences
    /// </summary>
    private readonly IPreferenceProxy prefProxy;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionReportController(IASNodeClient raptorClient, IConfigurationStore configStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager, IProductionDataRequestFactory requestFactory, IPreferenceProxy prefProxy) :
      base(configStore, fileListProxy, settingsManager)
    {
      this.raptorClient = raptorClient;
      this.requestFactory = requestFactory;
      this.prefProxy = prefProxy;
    }

    /// <summary>
    /// Returns a Grid Report for the Project constrained by the input parameters.
    /// </summary>
    /// <param name="projectUid">The project unique identifier.</param>
    /// <param name="filterUid">The filter UID to apply to the report results</param>
    /// <param name="reportElevation">Exclude/include Elevation data in the report.</param>
    /// <param name="reportCmv">Exclude/include CMV data in the report.</param>
    /// <param name="reportMdp">Exclude/include MDP data in the report.</param>
    /// <param name="reportPassCount">Exclude/include Pass Count data in the report.</param>
    /// <param name="reportTemperature">Exclude/include Temperature data in the report.</param>
    /// <param name="reportCutFill">Exclude/include Cut/Fill data in the report.</param>
    /// <param name="cutfillDesignUid">The cut/fill design file unique identifier if Cut/Fill data is included in the report.</param>
    /// <param name="gridInterval">The grid spacing interval for the sampled points.</param>
    /// <param name="gridReportOption">Grid report option. Whether it is defined automatically or by user specified parameters.</param>
    /// <param name="startNorthing">The Northing ordinate of the location to start gridding from.</param>
    /// <param name="startEasting">The Easting ordinate of the location to start gridding from.</param>
    /// <param name="endNorthing">The Northing ordinate of the location to end gridding at.</param>
    /// <param name="endEasting">The Easting ordinate of the location to end gridding at.</param>
    /// <param name="azimuth">The orientation of the grid, expressed in radians</param>
    /// <returns>An instance of the <see cref="ContractExecutionResult"/> class.</returns>
    [Route("api/v2/report/grid")]
    [HttpGet]
    public async Task<CompactionReportResult> GetReportGrid(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] bool reportElevation,
      [FromQuery] bool reportCmv,
      [FromQuery] bool reportMdp,
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
      Log.LogInformation("GetReportGrid: " + Request.QueryString);

      var projectId = await GetLegacyProjectId(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var cutFillDesign = await GetAndValidateDesignDescriptor(projectUid, cutfillDesignUid, true);
      var projectSettings = await GetProjectSettingsTargets(projectUid);

      var reportGridRequest = requestFactory.Create<CompactionReportGridRequestHelper>(r => r
          .ProjectId(projectId)
          .Headers(CustomHeaders)
          .ProjectSettings(projectSettings)
          .Filter(filter))
        .CreateCompactionReportGridRequest(
          reportElevation,
          reportCmv,
          reportMdp,
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
          .Build<CompactionReportGridExecutor>(LoggerFactory, raptorClient, null, ConfigStore)
          .Process(reportGridRequest) as CompactionReportResult
      );
    }

    /// <summary>
    /// Get station offset report data for the given filter and design files.
    /// </summary>
    /// <returns>Returns the station offset report results as JSON.</returns>
    [Route("api/v2/report/stationoffset")]
    [HttpGet]
    public async Task<CompactionReportResult> GetStationOffsetReport(
      [FromQuery] Guid projectUid,
      [FromQuery] bool reportElevation,
      [FromQuery] bool reportCmv,
      [FromQuery] bool reportMdp,
      [FromQuery] bool reportPassCount,
      [FromQuery] bool reportTemperature,
      [FromQuery] bool reportCutFill,
      [FromQuery] Guid filterUid,
      [FromQuery] Guid? cutfillDesignUid,
      [FromQuery] Guid? alignmentUid,
      [FromQuery] double crossSectionInterval,
      [FromQuery] double startStation,
      [FromQuery] double endStation,
      [FromQuery] double[] offsets)
    {
      Log.LogInformation("GetStationOffset: " + Request.QueryString);

      var project = await (User as RaptorPrincipal)?.GetProject(projectUid);
      var filter = await GetCompactionFilter(projectUid, filterUid);
      var cutFillDesignDescriptor = await GetAndValidateDesignDescriptor(projectUid, cutfillDesignUid);
      var alignmentDescriptor = await GetAndValidateDesignDescriptor(projectUid, alignmentUid);
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var userPreferences = (User as RaptorPrincipal).IsApplication ? new UserPreferenceData() : await GetUserPreferences();

      // Add 0.0 value to the offsets array, remove any duplicates and sort contents by ascending order...
      var updatedOffsets = offsets?.AddZeroDistinctSortBy();

      var reportRequest = requestFactory.Create<CompactionReportStationOffsetRequestHelper>(r => r
        .ProjectId(project.LegacyProjectId)
        .Headers(CustomHeaders)
        .ProjectSettings(projectSettings)
        .Filter(filter))
      .CreateRequest(
        reportElevation,
        reportCmv,
        reportMdp,
        reportPassCount,
        reportTemperature,
        reportCutFill,
        cutFillDesignDescriptor,
        alignmentDescriptor,
        crossSectionInterval,
        startStation,
        endStation,
        updatedOffsets,
        userPreferences,
        project.ProjectTimeZone);

      reportRequest.Validate();

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainerFactory
          .Build<CompactionReportStationOffsetExecutor>(LoggerFactory, raptorClient, null, ConfigStore)
          .Process(reportRequest) as CompactionReportResult
      );
    }

    private async Task<UserPreferenceData> GetUserPreferences()
    {
      var userPreferences = await prefProxy.GetUserPreferences(this.CustomHeaders);
      if (userPreferences == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
            "Failed to retrieve preferences for current user"));
      }
      return userPreferences;
    }
  }
}
