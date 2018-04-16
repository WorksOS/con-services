using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.Productivity3D.Scheduler.WebAPI.ExportJobs
{
  public interface IApiClient
  {
    Task<T> SendRequest<T>(string url, IDictionary<string, string> customHeaders,
      string method = "POST", string payload = null, Stream streamPayload = null);
  }
}
