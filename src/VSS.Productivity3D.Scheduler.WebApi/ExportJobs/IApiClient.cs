using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  public interface IApiClient
  {
    Task<T> SendRequest<T>(ScheduleJobRequest jobRequest, IDictionary<string, string> customHeaders,
      Stream streamPayload = null);
  }
}
