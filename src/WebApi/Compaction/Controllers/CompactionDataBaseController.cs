using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Common base for Detail and Summary service controllers.
  /// </summary>
  public class CompactionDataBaseController : BaseController<CompactionDataBaseController>
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    protected readonly IASNodeClient RaptorClient;

    /// <summary>
    /// The request factory
    /// </summary>
    protected readonly IProductionDataRequestFactory RequestFactory;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionDataBaseController(IASNodeClient raptorClient, IConfigurationStore configStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager, IProductionDataRequestFactory requestFactory)
      : base(configStore, fileListProxy, settingsManager)
    {
      RaptorClient = raptorClient;
      RequestFactory = requestFactory;
    }

    /// <summary>
    /// Short-circuit cache time for Archived projects.
    /// </summary>
    protected async Task SetCacheControlPolicy(Guid projectUid)
    {
      var project = await ((RaptorPrincipal)User).GetProject(projectUid);
      if (!project.IsArchived)
      {
        return;
      }

      const string days365 = "31536000";
      Response.Headers["Cache-Control"] = $"public,max-age={days365}";
    }

    /// <summary>
    /// Creates an instance of the CMVRequest class and populate it with data.
    /// </summary>
    protected async Task<CMVRequest> GetCmvRequest(Guid projectUid, Guid? filterUid, bool isCustomCMVTargets = false)
    {
      var projectSettings = await GetProjectSettingsTargets(projectUid);

      CMVSettings cmvSettings = !isCustomCMVTargets ?
        SettingsManager.CompactionCmvSettings(projectSettings) :
        SettingsManager.CompactionCmvSettingsEx(projectSettings);

      LiftBuildSettings liftSettings = SettingsManager.CompactionLiftBuildSettings(projectSettings);

      var filter = await GetCompactionFilter(projectUid, filterUid);
      var projectId = await GetLegacyProjectId(projectUid);
      return CMVRequest.CreateCMVRequest(projectId, null, cmvSettings, liftSettings, filter, -1, null, null, null, isCustomCMVTargets);
    }

    /// <summary>
    /// Creates an instance of the PassCounts class and populate it with data.
    /// </summary>
    protected async Task<PassCounts> GetPassCountRequest(Guid projectUid, Guid? filterUid, bool isSummary)
    {
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      PassCountSettings passCountSettings = isSummary ? null : SettingsManager.CompactionPassCountSettings(projectSettings);
      LiftBuildSettings liftSettings = SettingsManager.CompactionLiftBuildSettings(projectSettings);

      var filter = await GetCompactionFilter(projectUid, filterUid);
      var projectId = await GetLegacyProjectId(projectUid);
      return PassCounts.CreatePassCountsRequest(projectId, null, passCountSettings, liftSettings, filter, -1, null, null, null);
    }

    /// <summary>
    /// Tests if there is overlapping data in Raptor 
    /// </summary>
    protected async Task<bool> ValidateFilterAgainstProjectExtents(Guid projectUid, Guid? filterUid)
    {
      Log.LogInformation("GetProjectStatistics: " + Request.QueryString);

      //No filter - so proceed further
      if (!filterUid.HasValue)
        return true;

      var excludedIds = await GetExcludedSurveyedSurfaceIds(projectUid);
      var projectId = await GetLegacyProjectId(projectUid);

      ProjectStatisticsRequest request = ProjectStatisticsRequest.CreateStatisticsParameters(projectId, excludedIds?.ToArray());
      request.Validate();
      try
      {
        var projectExtents =
          RequestExecutorContainerFactory.Build<ProjectStatisticsExecutor>(LoggerFactory, RaptorClient)
                                         .Process(request) as ProjectStatisticsResult;

        //No data in Raptor - stop
        if (projectExtents == null)
          return false;

        var filter = await GetCompactionFilter(projectUid, filterUid);

        //No filter dates defined - project extents requested. Proceed further
        if (filter.StartUtc == null && filter.EndUtc == null)
          return true;

        //Do we have intersecting dates? True if yes
        if (filter.StartUtc != null && filter.EndUtc != null)
          return projectExtents.startTime <= filter.EndUtc && filter.StartUtc <= projectExtents.endTime;

        //Handle 'as-at' dates where StartUTC is null but EndUTC is not null
        if (filter.StartUtc == null && filter.EndUtc != null)
          return projectExtents.startTime <= filter.EndUtc;

        //All other cases - proceed further
        return true;
      }
      catch
      {
        //Some exception - do not proceed further
        return false;
      }
    }
  }
}
