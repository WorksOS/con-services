using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  /// <summary>
  /// These use the cws cws-profilemanager controller
  /// </summary>
  public class CwsProjectClient : BaseClient, ICwsProjectClient
  {
    public CwsProjectClient(IConfigurationStore configuration, ILoggerFactory logger, IWebRequest gracefulClient) : base(configuration, logger, gracefulClient)
    {
    }

    public Task<CreateProjectResponseModel> CreateProject(CreateProjectRequestModel createProjectRequest, IDictionary<string, string> customHeaders = null)
    {
      return PostData<CreateProjectRequestModel, CreateProjectResponseModel>($"/projects", createProjectRequest, null, customHeaders);
    }
    
    public async Task<DeviceListResponseModel> GetDevicesForProject(string projectUid, IDictionary<string, string> customHeaders = null)
    {
      //  http://api-stg.trimble.com/cws-profilemanager-stg/1.0/projects/{projectId}/devices
      return await GetData<DeviceListResponseModel>($"/projects/{projectUid}/devices");
    }
  }
}
