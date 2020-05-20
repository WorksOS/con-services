using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
  public class SchedulerV1Proxy : BaseServiceDiscoveryProxy, ISchedulerProxy
  {
    public SchedulerV1Proxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    { }

    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.Scheduler;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V1;

    public override ApiType Type => ApiType.Private;

    public override string CacheLifeKey => "SCHEDULER_CACHE_LIFE";

    /// <summary>
    /// Schedules the export job with a Scheduler Service.
    /// </summary>
    [Obsolete("Use ScheduleBackgroundJob instead - generic solution")]
    public async Task<ScheduleJobResult> ScheduleExportJob(ScheduleJobRequest request, IHeaderDictionary customHeaders)
    {
      var jsonData = JsonConvert.SerializeObject(request);
      using (var payload = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
      {
        // "/internal/v1/export"
        var result = await SendMasterDataItemServiceDiscoveryNoCache<ScheduleJobResult>
            ("/export", customHeaders, HttpMethod.Post, payload: payload);
        if (result != null)
          return result;
      }

      log.LogDebug($"{nameof(ScheduleExportJob)} Failed to schedule an export job");
      return null;
    }

    /// <inheritdoc />
    public async Task<ScheduleJobResult> ScheduleBackgroundJob(ScheduleJobRequest request, IHeaderDictionary customHeaders)
    {
      var jsonData = JsonConvert.SerializeObject(request);
      using (var payload = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
      {
        // "/internal/v1/background"
        var result = await SendMasterDataItemServiceDiscoveryNoCache<ScheduleJobResult>
            ("/background", customHeaders, HttpMethod.Post, payload: payload);
        if (result != null)
          return result;
      }

      log.LogDebug($"{nameof(ScheduleBackgroundJob)} Failed to schedule a background job");
      return null;
    }

    /// <inheritdoc />
    public async Task<JobStatusResult> GetBackgroundJobStatus(string jobId, IHeaderDictionary customHeaders)
    {
      // "internal/v1/background/{jobId}"
      var result = await GetMasterDataItemServiceDiscoveryNoCache<JobStatusResult>
      ($"/background/{jobId}", customHeaders);
      if (result != null)
        return result;

      log.LogDebug($"{nameof(GetBackgroundJobStatus)} Failed to get job status");
      return null;
    }

    /// <inheritdoc />
    public async Task<Stream> GetBackgroundJobResults(string jobId, IHeaderDictionary customHeaders)
    {
      // "internal/v1/background/{jobId}/result"
      var result = await GetMasterDataStreamItemServiceDiscoveryNoCache
        ($"/background/{jobId}/result", customHeaders, HttpMethod.Get);
      if (result != null)
        return result;

      log.LogDebug($"{nameof(GetBackgroundJobResults)} Failed to get background jobid {jobId} results");
      return null;
    }

    /// <inheritdoc />
    public async Task<ScheduleJobResult> ScheduleVSSJob(JobRequest request, IHeaderDictionary customHeaders)
    {
      var jsonData = JsonConvert.SerializeObject(request);
      using (var payload = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
      {
        // "/internal/v1/runjob" there is a proxy for this but no endpoint in scheduler API
        var result = await SendMasterDataItemServiceDiscoveryNoCache<ScheduleJobResult>
          ("/runjob", customHeaders, HttpMethod.Post, payload: payload);
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

