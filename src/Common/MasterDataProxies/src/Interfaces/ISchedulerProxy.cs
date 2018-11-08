using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace VSS.MasterData.Proxies
{
  public interface ISchedulerProxy
  {
    [Obsolete("Use ScheduleBackgroundJob instead - generic solution")]
    Task<JobStatusResult> GetExportJobStatus(string jobId, IDictionary<string, string> customHeaders);

    [Obsolete("Use ScheduleBackgroundJob instead - generic solution")]
    Task<ScheduleJobResult> ScheduleExportJob(ScheduleJobRequest request, IDictionary<string, string> customHeaders);
    
    /// <summary>
    /// Schedule a background job via Scheduler, behind TPAAS
    /// </summary>
    /// <param name="request">Request to be executed</param>
    /// <param name="customHeaders">Any custom headers to be passed to the Scheduler</param>
    /// <returns>A Scheduled Job Result, with a Job ID to check status</returns>
    Task<ScheduleJobResult> ScheduleBackgroundJob(ScheduleJobRequest request, IDictionary<string, string> customHeaders);

    /// <summary>
    /// Get the job status for a preeviously started job
    /// </summary>
    /// <param name="jobId">Job ID</param>
    /// <param name="customHeaders">Any custom headers to be passed with the request</param>
    /// <returns>A Job Status, if it is completed, a Download URL will be included</returns>
    Task<JobStatusResult> GetBackgroundJobStatus(string jobId, IDictionary<string, string> customHeaders);

    /// <summary>
    /// Get a Background Job Result if the Job is completed
    /// </summary>
    /// <param name="jobId">Job ID</param>
    /// <param name="customHeaders">Any custom headers to be passed with the request</param>
    /// <returns>Stream Content matching the Result of the original job call</returns>
    Task<StreamContent> GetBackgroundJobResults(string jobId, IDictionary<string, string> customHeaders);

  }
}