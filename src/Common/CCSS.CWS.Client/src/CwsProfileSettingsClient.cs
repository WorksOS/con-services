using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS;
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
  ///   
  /// How to create project configuration files:
  ///   1) Call POST CwsDeesignClient.CreateFile() with the filename you want to use. This returns the assigned fileSpaceID and Url.
  ///   2) Use the Url from #1 to PUT the binary file to DataOcean.
  ///   3) Call POST CwsProfileSettingsClient.SaveCalibrationFile() using the fileSpaceId from #1 (or PUT Update)
  ///   
  /// Files per ProjectConfigurationFileType.
  ///    Normally there is only 1 file per type
  ///    However for  control point and avoidance zones (at least), the user can select 2 files.
  ///      One for MachineControl, and the other for SiteCollectors. 
  ///      This is because each machine type supports different formats and content etc. 
  ///      Indicate in ProjectConfigurationFileRequestModel machineControlFilespaceId and siteCollectorFilespaceId
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
    public async Task<ProjectConfigurationFileResponseModel> GetProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, IHeaderDictionary customHeaders = null)
    { 
      log.LogDebug($"{nameof(GetProjectConfiguration)}: projectUid {projectUid} projectConfigurationFileType {projectConfigurationFileType}");

      var projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_PROJECT);
      ProjectConfigurationFileResponseModel projectConfigurationFileResponse = null;
      try
      {
        projectConfigurationFileResponse = await GetData<ProjectConfigurationFileResponseModel>($"/projects/{projectTrn}/configuration/{projectConfigurationFileType.ToString().ToUpper()}", null, null, null, customHeaders);
      }
      catch (HttpRequestException e)
      {
        if (e.IsNotFoundException())
        {
          return null;
        }

        throw;
      }
      
      log.LogDebug($"{nameof(GetProjectConfiguration)}: projectConfigurationFileResponse {JsonConvert.SerializeObject(projectConfigurationFileResponse)}");
      return projectConfigurationFileResponse;
    }

    ///// <summary>
    ///// GET https://trimble.com/connectedsiteprofilesettings/1.0/projects/{projectId}/configuration
    ////// Only 1 of each project calibrationtype is allowed
    /////   user token
    /////   used by ProjectSvc v6 and v5TBC
    ///// </summary>
    public async Task<ProjectConfigurationFileListResponseModel> GetProjectConfigurations(Guid projectUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetProjectConfigurations)}: projectUid {projectUid}");

      var projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_PROJECT);
      ProjectConfigurationFileListResponseModel projectConfigurationFileListResponse = null;
      try 
      {
        projectConfigurationFileListResponse = await GetData<ProjectConfigurationFileListResponseModel>($"/projects/{projectTrn}/configuration", null, null, null, customHeaders);
      }
      catch (HttpRequestException e)
      {
        if (e.IsNotFoundException())
        {
          return null;
        }

        throw;
      }
      log.LogDebug($"{nameof(GetProjectConfigurations)}: projectConfigurationFileListResponse {JsonConvert.SerializeObject(projectConfigurationFileListResponse)}");
      return projectConfigurationFileListResponse;
    }

    ///// <summary>
    ///// POST https://trimble.com/connectedsiteprofilesettings/1.0/projects/{projectId}/configuration/{fileType}
    /////   user token
    /////   used by ProjectSvc v6 and v5TBC
    ///// </summary>
    public async Task<ProjectConfigurationFileResponseModel> SaveProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, ProjectConfigurationFileRequestModel projectConfigurationFileRequest, IHeaderDictionary customHeaders = null)
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
    public async Task<ProjectConfigurationFileResponseModel> UpdateProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, ProjectConfigurationFileRequestModel projectConfigurationFileRequest, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(UpdateProjectConfiguration)}: projectUid {projectUid} projectConfigurationFileType {projectConfigurationFileType} projectConfigurationFileRequest {JsonConvert.SerializeObject(projectConfigurationFileRequest)}");

      var projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_PROJECT);
      ProjectConfigurationFileResponseModel projectConfigurationResponse = null;
      try
      {
        projectConfigurationResponse = await UpdateData<ProjectConfigurationFileRequestModel, ProjectConfigurationFileResponseModel>($"/projects/{projectTrn}/configuration/{projectConfigurationFileType.ToString().ToUpper()}", projectConfigurationFileRequest, null, customHeaders);
      }
      catch (HttpRequestException e)
      {
        if (e.IsNotFoundException())
        {
          return null;
        }

        throw;
      }

      log.LogDebug($"{nameof(UpdateProjectConfiguration)}: projectConfigurationResponse {JsonConvert.SerializeObject(projectConfigurationResponse)}");
      return projectConfigurationResponse;
    }


    /// <summary>
    /// Only 1 of each project calibrationtype is allowed, so this will delete that type
    /// </summary>
    public Task DeleteProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(DeleteProjectConfiguration)}: projectUid {projectUid} projectConfigurationFileType {projectConfigurationFileType}");

      var projectTrn = TRNHelper.MakeTRN(projectUid, TRNHelper.TRN_PROJECT);
      try
      {
        return DeleteData($"/projects/{projectTrn}/configuration/{projectConfigurationFileType.ToString().ToUpper()}", null, customHeaders);
      }
      catch (HttpRequestException e)
      {
        if (e.IsNotFoundException())
        {
          return Task.CompletedTask;
        }

        throw;
      }
    }
  }
}
