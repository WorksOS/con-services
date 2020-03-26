using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  /// <summary>
  /// These use the cws cws-profilemanager controller
  /// </summary>
  public class CwsProjectClient : CwsProfileManagerClient, ICwsProjectClient
  {
    public CwsProjectClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }
    
    /// <summary>
    /// POST https://api.trimble.com/t/trimble.com/cws-profilemanager/1.0/projects
    ///   user token
    ///   todoMaaverick where is this used ?
    ///                 what response fields are required?
    ///   CCSSCON- available              
    /// </summary>
    public Task<CreateProjectResponseModel> CreateProject(CreateProjectRequestModel createProjectRequest, IDictionary<string, string> customHeaders = null)
    {
      return PostData<CreateProjectRequestModel, CreateProjectResponseModel>($"/projects", createProjectRequest, null, customHeaders);
    }

    /// <summary>
    /// PUT https://api.trimble.com/t/trimble.com/cws-profilemanager/1.0/projects/{projectUid}
    ///   user token
    ///   todoMaaverick where is this used ?
    ///                 what response fields are required?
    ///   CCSSCON-???                
    /// </summary>
    public async Task UpdateProjectDetails(string projectUid, UpdateProjectDetailsRequestModel updateProjectDetailsRequest, IDictionary<string, string> customHeaders = null)
    {
      //  http://api-stg.trimble.com/cws-profilemanager-stg/1.0/projects/{projectId}/devices
      // return await UpdateData<UpdateProjectDetailsRequestModel>($"/projects/{projectUid}", updateProjectDetailsRequest, null, customHeaders);
      // todoMaverick return CallEndpoint<UpdateProjectDetailsRequestModel>($"/projects/{projectUid}", updateProjectDetailsRequest, HttpMethod.Put, customHeaders);
    }

    /// <summary>
    /// PUT https://api.trimble.com/t/trimble.com/cws-profilemanager/1.0/projects/{projectUid}/boundary
    ///   user token
    ///   todoMaaverick where is this used ?
    ///                 what response fields are required?
    ///   CCSSCON-???                
    /// </summary>
    public async Task UpdateProjectBoundary(string projectUid, ProjectBoundary projectBoundary, IDictionary<string, string> customHeaders = null)
    {
      // todoMaverick return CallEndpoint<ProjectBoundary>($"/projects/{projectUid}/boundary", projectBoundary, HttpMethod.Put, customHeaders);
    }
  }
}
