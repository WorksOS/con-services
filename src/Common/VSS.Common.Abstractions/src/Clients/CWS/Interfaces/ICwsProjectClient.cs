using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Common.Abstractions.Clients.CWS.Interfaces
{
  public interface ICwsProjectClient
  {
    Task<ProjectDetailListResponseModel> GetProjectsForCustomer(Guid customerUid, Guid? userUid = null, IHeaderDictionary customHeaders = null);

    Task<ProjectSummaryListResponseModel> GetProjectsForMyCustomer(Guid customerUid, Guid? userUid = null, IHeaderDictionary customHeaders = null);

    Task<ProjectDetailResponseModel> GetMyProject(Guid projectUid, Guid? userUid = null, IHeaderDictionary customHeaders = null);

    Task<CreateProjectResponseModel> CreateProject(CreateProjectRequestModel createProjectRequest, IHeaderDictionary customHeaders = null);

    Task UpdateProjectDetails(Guid projectUid, UpdateProjectDetailsRequestModel updateProjectDetailsRequestModel, IHeaderDictionary customHeaders = null);

    Task UpdateProjectBoundary(Guid projectUid, ProjectBoundary projectBoundary, IHeaderDictionary customHeaders = null);
  }
}
