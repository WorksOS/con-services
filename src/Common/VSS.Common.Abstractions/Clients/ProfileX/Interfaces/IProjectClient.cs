using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Clients.ProfileX.Enums;
using VSS.Common.Abstractions.Clients.ProfileX.Models;

namespace VSS.Common.Abstractions.Clients.ProfileX.Interfaces
{
  public interface IProjectClient
  {
    Task<ProjectCreateResponseModel> CreateProject(ProjectCreateRequestModel request, IDictionary<string, string> customHeaders = null);

    Task<ProjectUpdateResponseModel> UpdateProject(string trnId, ProjectUpdateRequestModel request, IDictionary<string, string> customHeaders = null);

    Task<ProjectListResponseModel> RetrieveMyProjects(int? fromIndex = null, int? limit = null, string sortBy = null, SortOrder sortOrder = SortOrder.Ascending, IDictionary<string, string> customHeaders = null);

    Task<ProjectModel> RetrieveProjectById(string trnId, IDictionary<string, string> customHeaders = null);

    Task DeleteProject(string trnId, IDictionary<string, string> customHeaders = null);
  }
}