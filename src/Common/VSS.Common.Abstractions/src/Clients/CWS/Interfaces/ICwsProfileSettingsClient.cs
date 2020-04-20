using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Common.Abstractions.Clients.CWS.Interfaces
{
  public interface ICwsProfileSettingsClient
  {
    Task<ProjectConfigurationFileResponseModel> GetProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, IDictionary<string, string> customHeaders = null);

    Task<ProjectConfigurationFileResponseModel> SaveProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, 
      ProjectConfigurationFileRequestModel projectConfigurationFileRequest, IDictionary<string, string> customHeaders = null);

    Task<ProjectConfigurationFileResponseModel> UpdateProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType,
      ProjectConfigurationFileRequestModel projectConfigurationFileRequest, IDictionary<string, string> customHeaders = null);
    
    Task DeleteProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, IDictionary<string, string> customHeaders = null);  
  }
}
