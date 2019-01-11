using System;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
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
    /// Raptor client for use by executor
    /// </summary>
    protected readonly IASNodeClient RaptorClient;

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
    public CompactionDataBaseController(IASNodeClient raptorClient, IConfigurationStore configStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager, IProductionDataRequestFactory requestFactory, ITRexCompactionDataProxy trexCompactionDataProxy)
      : base(configStore, fileListProxy, settingsManager)
    {
      RaptorClient = raptorClient;
      RequestFactory = requestFactory;
      TRexCompactionDataProxy = trexCompactionDataProxy;
    }

    /// <summary>
    /// Short-circuit cache time for Archived projects.
    /// </summary>
    protected Task SetCacheControlPolicy(Guid projectUid)
    {
      var project = ((RaptorPrincipal)User).GetProject(projectUid);

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

      var cmvSettings = !isCustomCMVTargets ?
        SettingsManager.CompactionCmvSettings(projectSettings) :
        SettingsManager.CompactionCmvSettingsEx(projectSettings);

      var liftSettings = SettingsManager.CompactionLiftBuildSettings(projectSettings);
      var filterTask = GetCompactionFilter(projectUid, filterUid);
      var projectIdTask = GetLegacyProjectId(projectUid);

      await Task.WhenAll(filterTask, projectIdTask);

      return new CMVRequest(projectIdTask.Result, projectUid, null, cmvSettings, liftSettings, filterTask.Result, -1, null, null, null, isCustomCMVTargets);
    }

    /// <summary>
    /// Creates an instance of the PassCounts class and populate it with data.
    /// </summary>
    protected async Task<PassCounts> GetPassCountRequest(Guid projectUid, Guid? filterUid, FilterResult filterResult = null, bool isSummary = false)
    {
      Task<FilterResult> filterTask = null;

      if (filterResult == null)
      {
        filterTask = GetCompactionFilter(projectUid, filterUid);
      }

      var projectIdTask = GetLegacyProjectId(projectUid);

      var projectSettings = await GetProjectSettingsTargets(projectUid);
      var passCountSettings = isSummary ? null : SettingsManager.CompactionPassCountSettings(projectSettings);
      var liftSettings = SettingsManager.CompactionLiftBuildSettings(projectSettings);

      await Task.WhenAll(filterTask ?? Task.CompletedTask, projectIdTask);

      if (filterResult == null)
      {
        filterResult = await filterTask;
      }

      return new PassCounts(projectIdTask.Result, projectUid, passCountSettings, liftSettings, filterResult, -1, null, null, null);
    }

    /// <summary>
    /// Tests if there is overlapping data in Raptor 
    /// </summary>
    protected async Task<(bool isValidFilterForProjextExtents, FilterResult filterResult)> ValidateFilterAgainstProjectExtents(Guid projectUid, Guid? filterUid)
    {
      if (!filterUid.HasValue) return (true, null);

      var excludedIdsTask = GetExcludedSurveyedSurfaceIds(projectUid);
      var projectIdTask = GetLegacyProjectId(projectUid);
      var filterTask = GetCompactionFilter(projectUid, filterUid, filterMustExist: true);

      await Task.WhenAll(filterTask, excludedIdsTask, projectIdTask);

      var filter = await filterTask;
      var projectId = await projectIdTask;
      var excludedIds = await excludedIdsTask;

      var request = ProjectStatisticsRequest.CreateStatisticsParameters(projectId, excludedIds?.ToArray());
      request.Validate();

      try
      {
        var projectExtents = RequestExecutorContainerFactory
                             .Build<ProjectStatisticsExecutor>(LoggerFactory, RaptorClient)
                             .Process(request) as ProjectStatisticsResult;

        //No data in Raptor - stop
        if (projectExtents == null) return (false, null);

        //No filter dates defined - project extents requested. Proceed further
        if (filter.StartUtc == null && filter.EndUtc == null) return (true, filter);

        //Do we have intersecting dates? True if yes
        if (filter.StartUtc != null && filter.EndUtc != null) return (true, filter);

        //Handle 'as-at' dates where StartUTC is null but EndUTC is not null
        if (filter.StartUtc == null && filter.EndUtc != null) return (true, filter);

        //All other cases - proceed further
        return (true, filter);
      }
      catch
      {
        //Some exception - do not proceed further
        return (false, null);
      }
    }
  }
}
