using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Common.Abstractions.Clients.CWS.Interfaces
{
  public interface ICwsProjectClient
  {
    Task<CreateProjectResponseModel> CreateProject(CreateProjectRequestModel createProjectRequest, IDictionary<string, string> customHeaders = null);
    
    Task<DeviceListResponseModel> GetDevicesForProject(string projectUid, IDictionary<string, string> customHeaders = null);
  }
}
