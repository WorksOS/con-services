using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace VSS.MasterData.Proxies
{
  public class SchedulerProxy : BaseProxy, ISchedulerProxy
  {
    public SchedulerProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(
      configurationStore, logger, cache)
    {
    }

    /// <summary>
    /// Schedules the export job with a Scheduler Service.
    /// </summary>
    /// <param name="request">Http request details of how to get the export data</param>
    /// <param name="customHeaders">The custom headers.</param>
    /// <returns></returns>
    [Obsolete("Use ScheduleBackgroundJob instead - generic solution")]
    public async Task<ScheduleJobResult> ScheduleExportJob(ScheduleJobRequest request, IDictionary<string, string> customHeaders)
    {
      var payload = JsonConvert.SerializeObject(request);
      var result = await SendRequest<ScheduleJobResult>("SCHEDULER_INTERNAL_EXPORT_URL", payload, customHeaders, null, HttpMethod.Post, string.Empty);
      if (result != null)
      {
        return result;
      }
      else
      {
        log.LogDebug("Failed to schedule a job");
        return null;
      }
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
      var result = await GetMasterDataItem<JobStatusResult>("SCHEDULER_EXTERNAL_EXPORT_URL",
        customHeaders, string.Empty, $"/{jobId}");
      if (result != null)
      {
        return result;
      }
      else
      {
        log.LogDebug("Failed to get job data");
        return null;
      }
    }

    /// <inheritdoc />
    public async Task<ScheduleJobResult> ScheduleBackgroundJob(ScheduleJobRequest request, IDictionary<string, string> customHeaders)
    {
      var payload = JsonConvert.SerializeObject(request);
      var result = await SendRequest<ScheduleJobResult>("SCHEDULER_INTERNAL_BACKGROUND_JOB_URL", payload, customHeaders, null, HttpMethod.Post, string.Empty);
      if (result != null)
        return result;

      log.LogDebug("Failed to schedule a job");
      return null;
    }

    /// <inheritdoc />
    public async Task<JobStatusResult> GetBackgroundJobStatus(string jobId, IDictionary<string, string> customHeaders)
    {
      var result = await GetMasterDataItem<JobStatusResult>("SCHEDULER_EXTERNAL_BACKGROUND_JOB_URL", customHeaders, string.Empty, $"{jobId}/result");

      if (result != null)
        return result;

      log.LogDebug("Failed to get job data");
      return null;
    }

    /// <inheritdoc />
    public async Task<Stream> GetBackgroundJobResults(string jobId, IDictionary<string, string> customHeaders)
    {
      var result = await GetMasterDataStreamContent("SCHEDULER_EXTERNAL_BACKGROUND_JOB_URL", customHeaders, string.Empty, $"{jobId}/result");

      if (result != null)
        return result;

      log.LogDebug("Failed to get job data");
      return null;
    }

  }
}

