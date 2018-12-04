using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Factories.ProductionData;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApi.Models.Report.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Common base for Detail and Summary service controllers.
  /// </summary>
  public class CompactionDataBaseController : BaseController<CompactionDataBaseController>
  {
    /// <summary>
    /// The request factory
    /// </summary>
    protected readonly IProductionDataRequestFactory RequestFactory;

    /// <summary>
    /// The TRex Gateway proxy for use by executor.
    /// </summary>
    protected readonly ITRexCompactionDataProxy TRexCompactionDataProxy;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CompactionDataBaseController(IConfigurationStore configStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager, IProductionDataRequestFactory requestFactory, ITRexCompactionDataProxy trexCompactionDataProxy)
      : base(configStore, fileListProxy, settingsManager)
    {
      RequestFactory = requestFactory;
      TRexCompactionDataProxy = trexCompactionDataProxy;
    }

    /// <summary>
    /// Short-circuit cache time for Archived projects.
    /// </summary>
    protected Task SetCacheControlPolicy(Guid projectUid)
    {
      Task<ProjectData> project = ((RaptorPrincipal)User).GetProject(projectUid);

      if (project.Result.IsArchived)
      {
        const string days365 = "31536000";
        Response.Headers["Cache-Control"] = $"public,max-age={days365}";
      }

      return Task.CompletedTask;
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
      return new CMVRequest(projectId, projectUid, null, cmvSettings, liftSettings, filter, -1, null, null, null, isCustomCMVTargets);
    }

    /// <summary>
    /// Creates an instance of the PassCounts class and populate it with data.
    /// </summary>
    protected async Task<PassCounts> GetPassCountRequest(Guid projectUid, Guid? filterUid, bool isSummary)
    {
      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var passCountSettings = isSummary ? null : SettingsManager.CompactionPassCountSettings(projectSettings);
      var liftSettings = SettingsManager.CompactionLiftBuildSettings(projectSettings);

      Task<FilterResult> filter = GetCompactionFilter(projectUid, filterUid);
      Task<long> projectId = GetLegacyProjectId(projectUid);

      return new PassCounts(projectId.Result, projectUid, null, passCountSettings, liftSettings, filter.Result, -1, null, null, null);
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

      var excludedIds = GetExcludedSurveyedSurfaceIds(projectUid);
      var projectId = GetLegacyProjectId(projectUid);

      var request = ProjectStatisticsRequest.CreateStatisticsParameters(projectId.Result, excludedIds.Result?.ToArray());

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
