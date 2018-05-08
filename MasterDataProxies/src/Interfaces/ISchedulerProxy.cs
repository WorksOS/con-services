using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace VSS.MasterData.Proxies
{
  public interface ISchedulerProxy
  {
    Task<JobStatusResult> GetExportJobStatus(string jobId, string filename, IDictionary<string, string> customHeaders);
    Task<ScheduleJobResult> ScheduleExportJob(ScheduleJobRequest request, IDictionary<string, string> customHeaders);
  }
}