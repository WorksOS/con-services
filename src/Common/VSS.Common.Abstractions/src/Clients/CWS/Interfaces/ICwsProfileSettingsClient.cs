using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Common.Abstractions.Clients.CWS.Interfaces
{
  public interface ICwsProfileSettingsClient
  {
    Task<ProjectConfigurationFileResponseModel> GetProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, IHeaderDictionary customHeaders = null);

    Task<ProjectConfigurationFileListResponseModel> GetProjectConfigurations(Guid projectUid, IHeaderDictionary customHeaders = null);

    Task<ProjectConfigurationFileResponseModel> SaveProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType,
      ProjectConfigurationFileRequestModel projectConfigurationFileRequest, IHeaderDictionary customHeaders = null);

    Task<ProjectConfigurationFileResponseModel> UpdateProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType,
      ProjectConfigurationFileRequestModel projectConfigurationFileRequest, IHeaderDictionary customHeaders = null);

    Task DeleteProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, IHeaderDictionary customHeaders = null);
  }
}
