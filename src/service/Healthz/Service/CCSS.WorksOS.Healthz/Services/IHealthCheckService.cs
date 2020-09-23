using System.Threading.Tasks;
using CCSS.WorksOS.Healthz.Responses;

namespace CCSS.WorksOS.Healthz.Services
{
  public interface IHealthCheckService
  {
    Task<ServicePingResponse> QueryService(string serviceIdentifier, string url, Microsoft.AspNetCore.Http.IHeaderDictionary customHeaders);
  }
}
