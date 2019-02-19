using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.ProfileX.Enums;
using VSS.Common.Abstractions.Clients.ProfileX.Interfaces;
using VSS.Common.Abstractions.Clients.ProfileX.Models;
using VSS.Common.Abstractions.ExtensionMethods;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.ProfileX.Client
{
  public class ProjectClient : BaseClient, IProjectClient 
  {
    
    public ProjectClient(IConfigurationStore configuration, ILoggerFactory logger, IWebRequest gracefulClient) : base(configuration, logger, gracefulClient)
    {
    }

    public Task<ProjectCreateResponseModel> CreateProject(ProjectCreateRequestModel request, IDictionary<string, string> customHeaders = null)
    {
      return PostData<ProjectCreateRequestModel, ProjectCreateResponseModel>("/profiles/projects",
        request,
        customHeaders: customHeaders);
    }

    public Task<ProjectUpdateResponseModel> UpdateProject(string trnId, ProjectUpdateRequestModel request, IDictionary<string, string> customHeaders = null)
    {
      return UpdateData<ProjectUpdateRequestModel, ProjectUpdateResponseModel>($"/profiles/projects/{trnId}", 
        request,
        null, customHeaders);
    }

    public Task<ProjectListResponseModel> RetrieveMyProjects(int? fromIndex = null, int? limit = null, string sortBy = null,
      SortOrder sortOrder = SortOrder.Ascending, IDictionary<string, string> customHeaders = null)
    {
      var parameters = new Dictionary<string,string>();
      if (fromIndex.HasValue) 
        parameters["from"] = fromIndex.Value.ToString();

      if (limit.HasValue) 
        parameters["limit"] = limit.Value.ToString();

      if (!string.IsNullOrEmpty(sortBy)) 
        parameters["sortby"] = sortBy;

      parameters["sortorder"] = sortOrder.GetDescription();

      return GetData<ProjectListResponseModel>("/profiles/projects/me", parameters, customHeaders);
    }

    public Task<ProjectResponseModel> RetrieveProjectById(string trnId, IDictionary<string, string> customHeaders = null)
    {
      return GetData<ProjectResponseModel>($"/profiles/projects/{trnId}", null, customHeaders);
    }

    public Task DeleteProject(string trnId, IDictionary<string, string> customHeaders = null)
    {
      return DeleteData($"/profiles/projects/{trnId}", null, customHeaders);
    }

    public Task CreateExternalReferences(string projectTrnId, UpsertExternalReferenceRequestModel references,
      IDictionary<string, string> customHeaders = null)
    {
      return CallEndpoint($"/profiles/projects/{projectTrnId}/externalrefs", references, HttpMethod.Post,
        customHeaders);
    }

    public Task UpdateExternalReferences(string projectTrnId, UpsertExternalReferenceRequestModel references,
      IDictionary<string, string> customHeaders = null)
    {
      return CallEndpoint($"/profiles/projects/{projectTrnId}/externalrefs", 
        references, 
        HttpMethod.Put,
        customHeaders);
    }

    public Task<ProjectExternalReferencesResponse> GetExternalReferencesForProject(string projectTrnId, IDictionary<string, string> customHeaders = null)
    {
      return GetData<ProjectExternalReferencesResponse>($"/profiles/projects/{projectTrnId}/externalrefs", 
        null,
        customHeaders);
    }

    public Task DeleteProjectExternalReferences(string projectTrnId, IDictionary<string, string> customHeaders = null)
    {
      return DeleteData($"/profiles/projects/{projectTrnId}/externalrefs",
        null,
        customHeaders);
    }
  }
}
