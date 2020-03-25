﻿using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Common.Abstractions.Clients.CWS.Interfaces
{
  public interface ICwsProjectClient
  {
    Task<CreateProjectResponseModel> CreateProject(CreateProjectRequestModel createProjectRequest, IDictionary<string, string> customHeaders = null);

    Task UpdateProjectDetails(string projectUid, UpdateProjectDetailsRequestModel updateProjectDetailsRequestModel, IDictionary<string, string> customHeaders = null);

    Task UpdateProjectBoundary(string projectUid, ProjectBoundary projectBoundary, IDictionary<string, string> customHeaders = null);
  }
}