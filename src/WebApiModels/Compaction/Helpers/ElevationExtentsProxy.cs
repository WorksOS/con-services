using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.Productivity3D.WebApiModels.Report.Executors;
using VSS.Productivity3D.WebApiModels.Report.Models;
using Filter = VSS.Productivity3D.Common.Models.Filter;

namespace VSS.Productivity3D.WebApiModels.Compaction.Helpers
{
  /// <summary>
  /// Proxy for getting elevation statistics from Raptor. Used by elevation range, elevation palette and elevation tiles requests.
  /// </summary>
  public class ElevationExtentsProxy : IElevationExtentsProxy
  {


    private static readonly object listLockObject = new object();
    private static readonly Dictionary<string, object> statisticsRequestsDictionary = new Dictionary<string, object>();



    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Cache for elevation extents
    /// </summary>
    private readonly IMemoryCache elevationExtentsCache;

    /// <summary>
    /// Raptor client for use by executor
    /// 
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// For getting compaction settings for a project
    /// </summary>
    private readonly ICompactionSettingsManager settingsManager;

    private readonly string elevationExtentsCacheLifeKey = "ELEVATION_EXTENTS_CACHE_LIFE";

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    private readonly IConfigurationStore configStore;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="cache">Elevation extents cache</param>
    /// <param name="settingsManager">Compaction settings manager</param>
    /// <param name="configStore">Configuration store</param>
    public ElevationExtentsProxy(IASNodeClient raptorClient, ILoggerFactory logger, IMemoryCache cache, ICompactionSettingsManager settingsManager, IConfigurationStore configStore)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<ElevationExtentsProxy>();
      elevationExtentsCache = cache;
      this.settingsManager = settingsManager;
      this.configStore = configStore;
    }

    /// <summary>
    /// Gets the elevation statistics for the given filter from Raptor
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="filter">Compaction filter</param>
    /// <param name="projectSettings">Project settings</param>
    /// <returns>Elevation statistics</returns>
    public ElevationStatisticsResult GetElevationRange(long projectId, Filter filter, CompactionProjectSettings projectSettings)
    {
      string cacheKey;
      cacheKey = ElevationCacheKey(projectId, filter);

      if (filter == null || (filter.isFilterContainsSSOnly))
      {
        var projectExtentsRequest = ExtentRequest.CreateExtentRequest(projectId,null);
      }

      lock (listLockObject)
      {
     /*   cacheKey = ElevationCacheKey(projectId, filter);
        if (statisticsRequestsDictionary.ContainsKey(cacheKey))
          Monitor.Wait(statisticsRequestsDictionary[cacheKey]);*/
      }

      if (!elevationExtentsCache.TryGetValue(cacheKey, out ElevationStatisticsResult result))
      {
       /* lock (listLockObject)
        {
          statisticsRequestsDictionary.Add(cacheKey, new object());
          Monitor.Enter(statisticsRequestsDictionary[cacheKey]);
        }*/
        LiftBuildSettings liftSettings = settingsManager.CompactionLiftBuildSettings(projectSettings);

        ElevationStatisticsRequest statsRequest =
          ElevationStatisticsRequest.CreateElevationStatisticsRequest(projectId, null, filter, 0,
            liftSettings);
        statsRequest.Validate();

        result =
          RequestExecutorContainerFactory.Build<ElevationStatisticsExecutor>(logger, raptorClient)
            .Process(statsRequest) as ElevationStatisticsResult;

        //Check for 'No elevation range' result
        const double NO_ELEVATION = 10000000000.0;
        if (Math.Abs(result.MinElevation - NO_ELEVATION) < 0.001 && Math.Abs(result.MaxElevation + NO_ELEVATION) < 0.001)
        {
          result = null;
        }

        var opts = MemoryCacheExtensions.GetCacheOptions(elevationExtentsCacheLifeKey, configStore, log);
        elevationExtentsCache.Set(cacheKey, result, opts);
    /*    lock (listLockObject)
        {
          if (statisticsRequestsDictionary.ContainsKey(cacheKey))
            statisticsRequestsDictionary.Remove(cacheKey);
          Monitor.Exit(statisticsRequestsDictionary[cacheKey]);
        }*/
      }
      return result;
    }

    /// <summary>
    /// Gets the key for the elevation extents cache
    /// </summary>
    /// <param name="projectId">project ID</param>
    /// <param name="filter">Compaction filter</param>
    /// <returns>Cache key</returns>
    private string ElevationCacheKey(long projectId, Filter filter)
    {
      var filterHash = filter == null ? 0 : filter.GetHashCode();
      return $"{projectId},{filterHash}";
    }
  }
}