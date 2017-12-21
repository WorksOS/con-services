using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling;

namespace VSS.MasterData.Proxies
{
  public interface ISchedulerProxy
  {
    Task<JobStatusResult> GetVetaExportJobStatus(Guid projectUId, string jobId, IDictionary<string, string> customHeaders);

    Task<ScheduleJobResult> ScheduleVetaExportJob(Guid projectUid,
      string fileName, string machineNames, Guid? filterUid, IDictionary<string, string> customHeaders);
  }
}