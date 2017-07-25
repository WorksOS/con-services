using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies;
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
    private IConfigurationStore configStore;

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
      ElevationStatisticsResult result = null;
      string cacheKey;
      lock (lockObject)
      {
        cacheKey = ElevationCacheKey(projectId, filter);
      }
      if (!elevationExtentsCache.TryGetValue(cacheKey, out result))
      {
        LiftBuildSettings liftSettings = settingsManager.CompactionLiftBuildSettings(projectSettings);

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

        var opts = MemoryCacheExtensions.GetCacheOptions(elevationExtentsCacheLifeKey, configStore, log);
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
    /// <returns>Cache key</returns>
    /// <param name="projectId">Legacy project ID</param>
    /// <param name="startUtc">Start UTC.</param>
    /// <param name="endUtc">End UTC. </param>
    /// <param name="vibeStateOn">Only filter cell passes recorded when the vibratory drum was 'on'.  
    /// If set to null, returns all cell passes. If true, returns only cell passes with the cell pass parameter and the drum was on.  
    /// If false, returns only cell passes with the cell pass parameter and the drum was off.</param>
    /// <param name="elevationType">Controls the cell pass from which to determine data based on its elevation.</param>
    /// <param name="layerNumber">The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file)
    ///  to be used as the layer type filter. Layer 3 is then the third layer from the
    /// datum elevation where each layer has a thickness defined by the layerThickness member.</param>
    /// <param name="onMachineDesignId">A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
    /// May be null/empty, which indicates no restriction.</param>
    /// <param name="assetID">A machine is identified by its asset ID, machine name and john doe flag, indicating if the machine is known in VL.
    /// All three parameters must be specified to specify a machine. 
    /// Cell passes are only considered if the machine that recorded them is this machine. May be null/empty, which indicates no restriction.</param>
    /// <param name="machineName">See assetID</param>
    /// <param name="isJohnDoe">See assetID</param>
    /// <param name="excludedSurveyedSurfaceIds">The legacy imported file IDs of surveyed surfaces to exclude</param>
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