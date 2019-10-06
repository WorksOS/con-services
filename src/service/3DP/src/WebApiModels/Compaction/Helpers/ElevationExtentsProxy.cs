using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Cache.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.Productivity3D.Models.Compaction;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.Productivity3D.WebApi.Models.Report.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// Proxy for getting elevation statistics from Raptor. Used by elevation range, elevation palette and elevation tiles requests.
  /// </summary>
  public class ElevationExtentsProxy : IElevationExtentsProxy
  {
    private static readonly AsyncDuplicateLock memCacheLock = new AsyncDuplicateLock();

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Cache for elevation extents
    /// </summary>
    private readonly IDataCache elevationExtentsCache;
#if RAPTOR
    /// <summary>
    /// Raptor client for use by executor
    /// 
    /// </summary>
    private readonly IASNodeClient raptorClient;
#endif
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
    /// For requesting data from TRex database.
    /// </summary>
    private readonly ITRexCompactionDataProxy trexCompactionDataProxy;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="cache">Elevation extents cache</param>
    /// <param name="settingsManager">Compaction settings manager</param>
    /// <param name="configStore">Configuration store</param>
    /// <param name="trexCompactionDataProxy">Trex Gateway production data proxy</param>
    public ElevationExtentsProxy(
#if RAPTOR
      IASNodeClient raptorClient, 
#endif
      ILoggerFactory logger, IDataCache cache, ICompactionSettingsManager settingsManager, IConfigurationStore configStore, ITRexCompactionDataProxy trexCompactionDataProxy)
    {
#if RAPTOR
      this.raptorClient = raptorClient;
#endif
      this.logger = logger;
      this.log = logger.CreateLogger<ElevationExtentsProxy>();
      elevationExtentsCache = cache;
      this.settingsManager = settingsManager;
      this.configStore = configStore;
      this.trexCompactionDataProxy = trexCompactionDataProxy;
    }


    /// <summary>
    /// Gets the elevation statistics for the given filter from Raptor
    /// </summary>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="filter">Compaction filter</param>
    /// <param name="projectSettings">Project settings</param>
    /// <returns>Elevation statistics</returns>
    public async Task<ElevationStatisticsResult> GetElevationRange(long projectId, Guid projectUid, FilterResult filter,
      CompactionProjectSettings projectSettings, IDictionary<string, string> customHeaders)
    {
      const double NO_ELEVATION = 10000000000.0;

      var cacheKey = ElevationCacheKey(projectUid, filter);
      var strFilter = filter != null ? JsonConvert.SerializeObject(filter) : "";
      var opts = new MemoryCacheEntryOptions().GetCacheOptions(elevationExtentsCacheLifeKey, configStore, log);

      // User Story: 88271 - when the UI makes calls requiring elevation, we can bombard Raptor with duplicate calls to retrieve elevation
      // This can cause raptor to take longer than expected to query the same data over and over again.
      // This only ever happens when there is no cached item here, as once the item is cached for a given filter the cached item is returned.
      // To fix this, we will lock per cache key here, so only one call can be made to raptor, blocking all other calls requesting the same info until the cache item is ready
      // Overall the call time should not change for any request, however the number of calls will be reduced to 1 for each unique projectid / filter.
      ElevationStatisticsResult resultElevationStatisticsResult = null;

      using (await memCacheLock.LockAsync(cacheKey))
      {
        resultElevationStatisticsResult = await elevationExtentsCache.GetOrCreate(cacheKey, async entry =>
        {
          ElevationStatisticsResult result;
          entry.SetOptions(opts);
          if (filter == null || (filter.isFilterContainsSSOnly) || (filter.IsFilterEmpty))
          {
            log.LogDebug($"Calling elevation statistics from Project Extents for project {projectId} and filter {strFilter}");

            var projectExtentsRequest = new ExtentRequest(projectId, projectUid,filter != null ? filter.SurveyedSurfaceExclusionList.ToArray() : null);
            var extents = await RequestExecutorContainerFactory.Build<ProjectExtentsSubmitter>(logger,
#if RAPTOR
                raptorClient,
#endif
                configStore: configStore, trexCompactionDataProxy: trexCompactionDataProxy, customHeaders: customHeaders)
              .ProcessAsync(projectExtentsRequest) as ProjectExtentsResult;

            if (extents != null)
            {
              result = new ElevationStatisticsResult(
                new BoundingBox3DGrid(extents.ProjectExtents.MinX, extents.ProjectExtents.MinY,
                  extents.ProjectExtents.MinZ, extents.ProjectExtents.MaxX, extents.ProjectExtents.MaxY,
                  extents.ProjectExtents.MaxZ), extents.ProjectExtents.MinZ, extents.ProjectExtents.MaxZ, 0.0);
            }
            else
              result = new ElevationStatisticsResult(null, 0.0, 0.0, 0.0);
          }
          else
          {
            log.LogDebug(
              $"Calling elevation statistics from Elevation Statistics for project {projectId} and filter {strFilter}");

            var liftSettings = settingsManager.CompactionLiftBuildSettings(projectSettings);

            var statsRequest =
              new ElevationStatisticsRequest(projectId, projectUid, null, filter, 0,
                liftSettings);
            statsRequest.Validate();

            result =
              await RequestExecutorContainerFactory.Build<ElevationStatisticsExecutor>(logger,
#if RAPTOR
                  raptorClient,
#endif
                  configStore: configStore, trexCompactionDataProxy: trexCompactionDataProxy, customHeaders: customHeaders)
                .ProcessAsync(statsRequest) as ElevationStatisticsResult;
          }

          //Check for 'No elevation range' result
          if (Math.Abs(result.MinElevation - NO_ELEVATION) < 0.001 &&
              Math.Abs(result.MaxElevation + NO_ELEVATION) < 0.001)
          {
            result = null;
          }

          // We need to tag the result as this filter and project for cache invalidation
          var identifiers = new List<string> {projectUid.ToString()};
          if (filter?.Uid != null)
            identifiers.Add(filter.Uid.Value.ToString());

          log.LogDebug($"Done elevation request");
          return new CacheItem<ElevationStatisticsResult>(result, identifiers);
        });
      }
      
      return resultElevationStatisticsResult;
    }

    /// <summary>
    /// Gets the key for the elevation extents cache
    /// </summary>
    /// <param name="projectId">project ID</param>
    /// <param name="filter">Compaction filter</param>
    /// <returns>Cache key</returns>
    private string ElevationCacheKey(Guid projectUid, FilterResult filter)
    {
      var filterHash = filter?.Uid == null ? string.Empty : filter.Uid.Value.ToString();
      return $"Elevation-{projectUid}--{filterHash}";
    }
  }
}
