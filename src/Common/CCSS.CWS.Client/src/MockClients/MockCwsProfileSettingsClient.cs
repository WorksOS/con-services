using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client.MockClients
{
  /// <summary>
  /// Mocks to use until we can get the real endpoints
  /// </summary>
  [Obsolete("UI to use cws directly now")]
  public class MockCwsProfileSettingsClient : CwsProfileSettingsManagerClient, ICwsProfileSettingsClient
  {
    public MockCwsProfileSettingsClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    public Task<ProjectConfigurationModel> GetProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetProjectConfiguration)} Mock: projectUid {projectUid} projectConfigurationFileType {JsonConvert.SerializeObject(projectConfigurationFileType)}");

      var projectConfigurationModel = new ProjectConfigurationModel
      {
        FileName = "MyTestFilename.dc",
        FileDownloadLink = "http//whatever",
        FileType = ProjectConfigurationFileType.CALIBRATION.ToString(),
        CreatedAt = DateTime.UtcNow.ToString(),
        UpdatedAt = DateTime.UtcNow.ToString(),
        Size = 66
      };

      log.LogDebug($"{nameof(GetProjectConfiguration)} Mock: projectConfigurationModel {JsonConvert.SerializeObject(projectConfigurationModel)}");
      return Task.FromResult(projectConfigurationModel);
    }

    public Task<ProjectConfigurationFileListResponseModel> GetProjectConfigurations(Guid projectUid, IHeaderDictionary customHeaders = null)
    {
      var projectConfigurationFileListResponse = new ProjectConfigurationFileListResponseModel
      {
        new ProjectConfigurationModel()
        {
          FileName = "MyTestFilename.dc",
          FileDownloadLink = "http//whatever",
          FileType = ProjectConfigurationFileType.CALIBRATION.ToString(),
          CreatedAt = DateTime.UtcNow.ToString(),
          UpdatedAt = DateTime.UtcNow.ToString(),
          Size = 66
        },
        new ProjectConfigurationModel()
        {
          FileName = "MyTestFilename.avoid.dxf",
          FileDownloadLink = "http//whateverElse",
          FileType = ProjectConfigurationFileType.AVOIDANCE_ZONE.ToString(),
          CreatedAt = DateTime.UtcNow.ToString(),
          UpdatedAt = DateTime.UtcNow.ToString(),
          Size = 66
        }
      };
      return Task.FromResult(projectConfigurationFileListResponse);
    }

    public Task<ProjectConfigurationModel> SaveProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType,
      ProjectConfigurationFileRequestModel projectConfigurationFileRequest, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(SaveProjectConfiguration)} Mock: projectUid {projectUid} projectConfigurationFileType {JsonConvert.SerializeObject(projectConfigurationFileType)} projectConfigurationFileRequest {JsonConvert.SerializeObject(projectConfigurationFileRequest)}");

      var projectConfigurationModel = new ProjectConfigurationModel
      {
        FileName = "MyTestFilename.dc",
        FileDownloadLink = "http//whatever",
        FileType = ProjectConfigurationFileType.CALIBRATION.ToString(),
        CreatedAt = DateTime.UtcNow.ToString(),
        UpdatedAt = DateTime.UtcNow.ToString(),
        Size = 66
      };

      log.LogDebug($"{nameof(SaveProjectConfiguration)} Mock: projectConfigurationModel {JsonConvert.SerializeObject(projectConfigurationModel)}");
      return Task.FromResult(projectConfigurationModel);
    }

    public Task<ProjectConfigurationModel> UpdateProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType,
      ProjectConfigurationFileRequestModel projectConfigurationFileRequest, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(UpdateProjectConfiguration)} Mock: projectUid {projectUid} projectConfigurationFileType {JsonConvert.SerializeObject(projectConfigurationFileType)} projectConfigurationFileRequest {JsonConvert.SerializeObject(projectConfigurationFileRequest)}");

      var projectConfigurationModel = new ProjectConfigurationModel
      {
        FileName = "MyTestFilename.dc",
        FileDownloadLink = "http//whatever",
        FileType = ProjectConfigurationFileType.CALIBRATION.ToString(),
        CreatedAt = DateTime.UtcNow.ToString(),
        UpdatedAt = DateTime.UtcNow.ToString(),
        Size = 66
      };

      log.LogDebug($"{nameof(UpdateProjectConfiguration)} Mock: projectConfigurationModel {JsonConvert.SerializeObject(projectConfigurationModel)}");
      return Task.FromResult(projectConfigurationModel);
    }

    public Task DeleteProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(DeleteProjectConfiguration)} Mock: projectUid {projectUid} projectConfigurationFileType {JsonConvert.SerializeObject(projectConfigurationFileType)}");
      return Task.CompletedTask;
    }
  }
}
