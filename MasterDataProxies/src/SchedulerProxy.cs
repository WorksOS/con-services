using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
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
    /// Schedules the veta export job with a Scheduler Service.
    /// </summary>
    /// <param name="projectUid">The project uid.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="machineNames">The machine names.</param>
    /// <param name="filterUid">The filter uid.</param>
    /// <param name="customHeaders">The custom headers.</param>
    /// <returns></returns>
    public async Task<string> ScheduleVetaExportJob(Guid projectUid,
      string fileName, string machineNames, Guid? filterUid, IDictionary<string, string> customHeaders = null)
    {
      var result = await GetMasterDataItem<string>("SCHEDULER_EXPORT_URL",
        customHeaders,
        $"?projectUid={projectUid}&fileName={fileName}&machineNames={machineNames}&filterUid={filterUid}");
      if (!string.IsNullOrEmpty(result))
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
    /// Retrieves the status of the requested job and a filename in S3 bucket used.
    /// </summary>
    /// <param name="projectUid">The project for which the veta export is being done.</param>
    /// <param name="jobId">The job identifier.</param>
    /// <returns></returns>
    public async Task<Tuple<string, string>> GetVetaExportJobStatus(Guid projectUid, string jobId)
    {
      var result = await GetMasterDataItem<Tuple<string, string>>("SCHEDULER_EXPORT_URL",
        null, null, $"/{projectUid}/{jobId}");
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

  }
}

