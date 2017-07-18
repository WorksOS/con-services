using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Interfaces;
using VSS.Productivity3D.WebApiModels.Report.Executors;
using VSS.Productivity3D.WebApiModels.Report.Models;
using VSS.Productivity3D.WebApiModels.Report.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.Compaction.Helpers
{
  /// <summary>
  /// Proxy for getting elevation statistics from Raptor. Used by elevation range, elevation palette and elevation tiles requests.
  /// </summary>
  public class ElevationExtentsProxy : IElevationExtentsProxy
  {
    private static readonly object lockObject = new object();

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
    /// How long to cache elevation extents
    /// </summary>
    private readonly TimeSpan elevationExtentsCacheLife = new TimeSpan(0, 15, 0); //TODO: how long to cache ?

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    /// <param name="cache">Elevation extents cache</param>
    public ElevationExtentsProxy(IASNodeClient raptorClient, ILoggerFactory logger, IMemoryCache cache)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      elevationExtentsCache = cache;
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
      ElevationStatisticsResult result = null;
      string cacheKey;
      lock (lockObject)
      {
        cacheKey = ElevationCacheKey(projectId, filter);
      }
      if (!elevationExtentsCache.TryGetValue(cacheKey, out result))
      {
        LiftBuildSettings liftSettings = CompactionSettings.CompactionLiftBuildSettings(projectSettings);

        ElevationStatisticsRequest statsRequest =
          ElevationStatisticsRequest.CreateElevationStatisticsRequest(projectId, null, filter, 0,
            liftSettings);
        statsRequest.Validate();

        result =
          RequestExecutorContainer.Build<ElevationStatisticsExecutor>(logger, raptorClient)
            .Process(statsRequest) as ElevationStatisticsResult;

        //Check for 'No elevation range' result
        const double NO_ELEVATION = 10000000000.0;
        if (Math.Abs(result.MinElevation - NO_ELEVATION) < 0.001 && Math.Abs(result.MaxElevation + NO_ELEVATION) < 0.001)
        { 
          result = null;
        }

        var opts = new MemoryCacheEntryOptions
        {
          SlidingExpiration = elevationExtentsCacheLife
        };
        elevationExtentsCache.Set(cacheKey, result, opts);
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
      return
        filter == null
          ? ElevationCacheKey(projectId, null, null, null, null, null, null, null, null, null, null)
          : ElevationCacheKey(projectId, filter.startUTC, filter.endUTC, filter.vibeStateOn,
            filter.elevationType, filter.layerNumber, filter.onMachineDesignID,
            filter.contributingMachines == null || filter.contributingMachines.Count == 0
              ? (long?)null
              : filter.contributingMachines[0].assetID,
            //Can only filter by one machine at present
            filter.contributingMachines == null || filter.contributingMachines.Count == 0
              ? null
              : filter.contributingMachines[0].machineName,
            filter.contributingMachines == null || filter.contributingMachines.Count == 0
              ? (bool?)null
              : filter.contributingMachines[0].isJohnDoe,
            filter.surveyedSurfaceExclusionList);
    }

    /// <summary>
    /// Gets the key for the elevation extents cache
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="startUtc"></param>
    /// <param name="endUtc"></param>
    /// <param name="vibeStateOn"></param>
    /// <param name="elevationType"></param>
    /// <param name="layerNumber"></param>
    /// <param name="onMachineDesignId"></param>
    /// <param name="assetId"></param>
    /// <param name="machineName"></param>
    /// <param name="isJohnDoe"></param>
    /// <param name="excludedSurveyedSurfaceIds"></param>
    /// <returns>Cache key</returns>
    private string ElevationCacheKey(long projectId, DateTime? startUtc, DateTime? endUtc,
      bool? vibeStateOn, ElevationType? elevationType, int? layerNumber, long? onMachineDesignId, long? assetId,
      string machineName, bool? isJohnDoe, List<long> excludedSurveyedSurfaceIds)
    {
      var key =
        $"{projectId},{startUtc},{endUtc},{vibeStateOn},{elevationType},{layerNumber},{onMachineDesignId},{assetId},{machineName},{isJohnDoe}";
      if (excludedSurveyedSurfaceIds == null || excludedSurveyedSurfaceIds.Count == 0)
      {
        key += ",null";
      }
      else
      {
        foreach (long id in excludedSurveyedSurfaceIds)
        {
          key = $"{key},{id}";
        }
      }
      return key;
    }
  }
}