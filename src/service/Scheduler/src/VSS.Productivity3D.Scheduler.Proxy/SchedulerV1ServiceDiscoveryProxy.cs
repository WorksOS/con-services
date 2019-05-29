using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.Productivity3D.Scheduler.Models;

namespace VSS.Productivity3D.Scheduler.Proxy
{
  public class SchedulerV1ServiceDiscoveryProxy : BaseServiceDiscoveryProxy, ISchedulerProxy
  {
    public SchedulerV1ServiceDiscoveryProxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.Scheduler;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V1;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "SCHEDULER_CACHE_LIFE"; // n/a todoJeannie

    /// <summary>
    /// Schedules the export job with a Scheduler Service.
    /// </summary>
    /// <param name="request">Http request details of how to get the export data</param>
    /// <param name="customHeaders">The custom headers.</param>
    /// <returns></returns>
    [Obsolete("Use ScheduleBackgroundJob instead - generic solution")]
    public async Task<ScheduleJobResult> ScheduleExportJob(ScheduleJobRequest request,
      IDictionary<string, string> customHeaders)
    {
      // SCHEDULER_INTERNAL_BASE_URL internal/v1 POST /runjob (mock)

      // SCHEDULER_INTERNAL_EXPORT_URL         POST internal/v1/export (/mock)
      // SCHEDULER_INTERNAL_BACKGROUND_JOB_URL POST internal/v1/background

      // SCHEDULER_EXTERNAL_EXPORT_URL /api/v1/export/{jobId} (not configured anywhere)
      // SCHEDULER_EXTERNAL_BACKGROUND_JOB_URL GET api/v1 (not configured anywhere)
      var jsonData = JsonConvert.SerializeObject(request);
      using (var payload = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
      {
        //TODO: Use the new "SCHEDULER_INTERNAL_BASE_URL" and a route to reduce the number of env vars
        // "SCHEDULER_INTERNAL_EXPORT_URL "internal/v1/export" // todoJeannie pattern different
        var result =
          await PostMasterDataItemServiceDiscoveryNoCache<ScheduleJobResult>
            ("internal/v1/export", customHeaders, payload: payload);
        if (result != null)
          return result;
      }

      log.LogDebug($"{nameof(ScheduleExportJob)} Failed to schedule a job");
      return null;
    }

    /// <summary>
    /// Retrieves the status of the requested job and the filename in S3 bucket where file is stored.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="customHeaders">Custom request headers</param>
    /// <returns></returns>
    [Obsolete("Use ScheduleBackgroundJob instead - generic solution")]
    public async Task<JobStatusResult> GetExportJobStatus(string jobId, IDictionary<string, string> customHeaders)
    {
      // "SCHEDULER_EXTERNAL_EXPORT_URL "/api/v1/export/{jobId}" // todoJeannie pattern different
      var result = await GetMasterDataItemServiceDiscovery<JobStatusResult>($"/export/{jobId}", 
        Guid.NewGuid().ToString(), null, /* todoJeannie how to indicate to never cache? */ 
        customHeaders);
      if (result != null)
        return result;

      log.LogDebug($"{nameof(GetExportJobStatus)} Failed to get job status");
      return null;
    }

    /// <inheritdoc />
    public async Task<ScheduleJobResult> ScheduleBackgroundJob(ScheduleJobRequest request, IDictionary<string, string> customHeaders)
    {
      var jsonData = JsonConvert.SerializeObject(request);
      using (var payload = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
      {
        // "SCHEDULER_INTERNAL_BACKGROUND_JOB_URL "internal/v1/background" // todoJeannie pattern different
        var result =
          await PostMasterDataItemServiceDiscoveryNoCache<ScheduleJobResult>("internal/v1/background", customHeaders,
            payload: payload);
        if (result != null)
          return result;
      }

      log.LogDebug($"{nameof(ScheduleBackgroundJob)} Failed to schedule a background job");
      return null;
    }

    /// <inheritdoc />
    public async Task<JobStatusResult> GetBackgroundJobStatus(string jobId, IDictionary<string, string> customHeaders)
    {
      // SCHEDULER_EXTERNAL_BACKGROUND_JOB_URL api/v1/export/{jobId} & api/v1/background/{jobId} same endpoint 
      var result = await GetMasterDataItemServiceDiscovery<JobStatusResult>($"/background/{jobId}",
        Guid.NewGuid().ToString(), null, /* todoJeannie how to indicate to never cache? */ 
        customHeaders);
      if (result != null)
        return result;

      log.LogDebug($"{nameof(GetBackgroundJobStatus)} Failed to get job status");
      return null;
    }

    /// <inheritdoc />
    public async Task<Stream> GetBackgroundJobResults(string jobId, IDictionary<string, string> customHeaders)
    {
      // SCHEDULER_EXTERNAL_BACKGROUND_JOB_URL api/v1/export/{jobId}/result & api/v1/background/{jobId}/result same endpoint 
      var result = await GetMasterDataStreamItemServiceDiscoveryNoCache($"/background/{jobId}/result", customHeaders);
      if (result != null)
        return result;

      log.LogDebug($"{nameof(GetBackgroundJobResults)} Failed to get job results");
      return null;
    }

    /// <inheritdoc />
    public async Task<ScheduleJobResult> ScheduleVSSJob(JobRequest request, IDictionary<string, string> customHeaders)
    {
      var jsonData = JsonConvert.SerializeObject(request);
      using (var payload = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
      {
        // SCHEDULER_INTERNAL_BASE_URL /internal/v1/mock/runjob
        var result = await PostMasterDataItemServiceDiscoveryNoCache<ScheduleJobResult>("/runjob", customHeaders, payload: payload);
        if (result != null)
          return result;
      }

      log.LogDebug($"{nameof(ScheduleVSSJob)} Failed to schedule a VSS job");
      return null;
    }

    public void ClearCacheItem(string uid, string userId = null)
    {
      throw new NotImplementedException();
    }
  }
}

