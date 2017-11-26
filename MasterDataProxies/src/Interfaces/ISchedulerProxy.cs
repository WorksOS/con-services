using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VSS.MasterData.Proxies
{
  public interface ISchedulerProxy
  {
    Task<Dictionary<string, string>> ScheduleVetaExportJob(string jobId);

    Task<string> ScheduleVetaExportJob(Guid projectUid,
      string fileName, string machineNames, Guid? filterUid, IDictionary<string, string> customHeaders = null);
  }
}