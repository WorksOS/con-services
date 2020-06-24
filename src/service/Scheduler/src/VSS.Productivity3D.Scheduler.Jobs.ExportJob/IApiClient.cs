using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  /// <summary>
  /// Interface for API calls to requested URLs
  /// </summary>
  public interface IApiClient
  {
    Task<HttpContent> SendRequestStream(ScheduleJobRequest jobRequest, IHeaderDictionary customHeaders);
    Task<T> SendRequest<T>(ScheduleJobRequest jobRequest, IHeaderDictionary customHeaders);
  }
}
