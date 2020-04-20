using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  /// <summary>
  /// These use the cws cws-profilesettingsmanager controller
  ///   See comments in CwsAccountClient re TRN/Guid conversions
  /// </summary>
  public class CwsProfileSettingsClient : CwsProfileSettingsManagerClient, ICwsProfileSettingsClient
  {
    public CwsProfileSettingsClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    ///// <summary>
    ///// GET https://trimble.com/connectedsiteprofilesettings/1.0/projects/{projectId}/configuration/{fileType}
    ////// Only 1 of each project calibrationtype is allowed
    /////   user token
    /////   used by ProjectSvc v6 and v5TBC
    ///// </summary>
    public async Task<ProjectConfigurationFileResponseModel> GetProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, IDictionary<string, string> customHeaders = null)
    { 
      log.LogDebug($"{nameof(GetProjectConfiguration)}: projectUid {projectUid} projectConfigurationFileType {projectConfigurationFileType}");

      var projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_PROJECT);
      var projectConfigurationResponse = await GetData<ProjectConfigurationFileResponseModel>($"/projects/{projectTrn}/configuration/{projectConfigurationFileType.ToString().ToUpper()}", null, null, null, customHeaders);

      log.LogDebug($"{nameof(GetProjectConfiguration)}: projectConfigurationResponse {JsonConvert.SerializeObject(projectConfigurationResponse)}");
      return projectConfigurationResponse;
    }

    ///// <summary>
    ///// POST https://trimble.com/connectedsiteprofilesettings/1.0/projects/{projectId}/configuration/{fileType}
    /////   user token
    /////   used by ProjectSvc v6 and v5TBC
    ///// </summary>
    public async Task<ProjectConfigurationFileResponseModel> SaveProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, ProjectConfigurationFileRequestModel projectConfigurationFileRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(SaveProjectConfiguration)}: projectUid {projectUid} projectConfigurationFileType {projectConfigurationFileType} projectConfigurationFileRequest {JsonConvert.SerializeObject(projectConfigurationFileRequest)}");

      var projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_PROJECT);
      var projectConfigurationResponse = await PostData<ProjectConfigurationFileRequestModel, ProjectConfigurationFileResponseModel>($"/projects/{projectTrn}/configuration/{projectConfigurationFileType.ToString().ToUpper()}", projectConfigurationFileRequest, null, customHeaders);

      log.LogDebug($"{nameof(SaveProjectConfiguration)}: projectConfigurationResponse {JsonConvert.SerializeObject(projectConfigurationResponse)}");
      return projectConfigurationResponse;
    }

    ///// <summary>
    ///// PUT https://trimble.com/connectedsiteprofilesettings/1.0/projects/{projectId}/configuration/{fileType}
    /////   user token
    /////   used by ProjectSvc v6 and v5TBC
    ///// </summary>
    public async Task<ProjectConfigurationFileResponseModel> UpdateProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, ProjectConfigurationFileRequestModel projectConfigurationFileRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(UpdateProjectConfiguration)}: projectUid {projectUid} projectConfigurationFileType {projectConfigurationFileType} projectConfigurationFileRequest {JsonConvert.SerializeObject(projectConfigurationFileRequest)}");

      var projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_PROJECT);
      var projectConfigurationResponse = await UpdateData<ProjectConfigurationFileRequestModel, ProjectConfigurationFileResponseModel>($"/projects/{projectTrn}/configuration/{projectConfigurationFileType.ToString().ToUpper()}", projectConfigurationFileRequest, null, customHeaders);

      log.LogDebug($"{nameof(UpdateProjectConfiguration)}: projectConfigurationResponse {JsonConvert.SerializeObject(projectConfigurationResponse)}");
      return projectConfigurationResponse;
    }


    /// <summary>
    /// Only 1 of each project calibrationtype is allowed, so this will delete that type
    /// </summary>
    public Task DeleteProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(DeleteProjectConfiguration)}: projectUid {projectUid} projectConfigurationFileType {projectConfigurationFileType}");

      var projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_PROJECT);
      return Task.FromResult(DeleteData($"/projects/{projectTrn}/configuration/{projectConfigurationFileType.ToString().ToUpper()}", null, customHeaders));
    }
  }
}
