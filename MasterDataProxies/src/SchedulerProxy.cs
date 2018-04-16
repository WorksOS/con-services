using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
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
    /// <param name="exportDataUrl">The URL for getting the export data.</param>
    /// <param name="customHeaders">The custom headers.</param>
    /// <returns></returns>
    public async Task<ScheduleJobResult> ScheduleExportJob(string exportDataUrl, IDictionary<string, string> customHeaders)
    {
      var result = await SendRequest<ScheduleJobResult>("SCHEDULER_EXPORT_URL", exportDataUrl, customHeaders, null, "POST", string.Empty);
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
    public async Task<JobStatusResult> GetExportJobStatus(string jobId, IDictionary<string, string> customHeaders)
    {
      var result = await GetMasterDataItem<JobStatusResult>("SCHEDULER_EXPORT_URL",
        customHeaders, null, $"/{jobId}");
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

    /*

    /// <summary>
    /// Schedules the veta export job with a Scheduler Service.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="machineNames">The machine names.</param>
    /// <param name="filterUid">The filter uid.</param>
    /// <param name="customHeaders">The custom headers.</param>
    /// <returns></returns>
    public async Task<ScheduleJobResult> ScheduleVetaExportJob(Guid projectUid,
      string fileName, string machineNames, Guid? filterUid, IDictionary<string, string> customHeaders)
    {
      var result = await GetMasterDataItem<ScheduleJobResult>("SCHEDULER_EXPORT_URL",
        customHeaders,
        $"?projectUid={projectUid}&fileName={fileName}&machineNames={machineNames}&filterUid={filterUid}");
      if (result!=null)
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
    /// <param name="projectUid">The project for which the veta export is being done.</param>
    /// <param name="jobId">The job identifier.</param>
    /// <param name="customHeaders">Custom request headers</param>
    /// <returns></returns>
    public async Task<JobStatusResult> GetVetaExportJobStatus(Guid projectUid, string jobId, IDictionary<string, string> customHeaders)
    {
      var result = await GetMasterDataItem<JobStatusResult>("SCHEDULER_EXPORT_URL",
        customHeaders, null, $"/{projectUid}/{jobId}");
      if (result!=null)
      {
        return result;
      }
      else
      {
        log.LogDebug("Failed to get job data");
        return null;
      }
    }
    */
  }
}

