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

namespace CCSS.CWS.Client.MockClients
{
  /// <summary>
  /// Mocks to use until we can get the real endpoints
  /// </summary>
  public class MockCwsProfileSettingsClient : CwsProfileSettingsManagerClient, ICwsProfileSettingsClient
  {
    public MockCwsProfileSettingsClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    public Task<ProjectConfigurationFileResponseModel> GetProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetProjectConfiguration)} Mock: projectUid {projectUid} projectConfigurationFileType {JsonConvert.SerializeObject(projectConfigurationFileType)}");

      var projectConfigurationFileResponseModel = new ProjectConfigurationFileResponseModel
      {
        FileName = "MyTestFilename.dc",
        FileDownloadLink = "http//whatever",
        FileType = ProjectConfigurationFileType.CALIBRATION.ToString(),
        CreatedAt = DateTime.UtcNow.ToString(),
        UpdatedAt = DateTime.UtcNow.ToString(),
        Size = "66"
      };

      log.LogDebug($"{nameof(GetProjectConfiguration)} Mock: projectConfigurationFileResponseModel {JsonConvert.SerializeObject(projectConfigurationFileResponseModel)}");
      return Task.FromResult(projectConfigurationFileResponseModel);
    }

    public Task<ProjectConfigurationFileResponseModel> SaveProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType,
      ProjectConfigurationFileRequestModel projectConfigurationFileRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(SaveProjectConfiguration)} Mock: projectUid {projectUid} projectConfigurationFileType {JsonConvert.SerializeObject(projectConfigurationFileType)} projectConfigurationFileRequest {JsonConvert.SerializeObject(projectConfigurationFileRequest)}");

      var projectConfigurationFileResponseModel = new ProjectConfigurationFileResponseModel
      {
        FileName = "MyTestFilename.dc",
        FileDownloadLink = "http//whatever",
        FileType = ProjectConfigurationFileType.CALIBRATION.ToString(),
        CreatedAt = DateTime.UtcNow.ToString(),
        UpdatedAt = DateTime.UtcNow.ToString(),
        Size = "66"
      };

      log.LogDebug($"{nameof(SaveProjectConfiguration)} Mock: projectConfigurationFileResponseModel {JsonConvert.SerializeObject(projectConfigurationFileResponseModel)}");
      return Task.FromResult(projectConfigurationFileResponseModel);
    }

    public Task<ProjectConfigurationFileResponseModel> UpdateProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType,
      ProjectConfigurationFileRequestModel projectConfigurationFileRequest, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(UpdateProjectConfiguration)} Mock: projectUid {projectUid} projectConfigurationFileType {JsonConvert.SerializeObject(projectConfigurationFileType)} projectConfigurationFileRequest {JsonConvert.SerializeObject(projectConfigurationFileRequest)}");

      var projectConfigurationFileResponseModel = new ProjectConfigurationFileResponseModel
      {
        FileName = "MyTestFilename.dc",
        FileDownloadLink = "http//whatever",
        FileType = ProjectConfigurationFileType.CALIBRATION.ToString(),
        CreatedAt = DateTime.UtcNow.ToString(),
        UpdatedAt = DateTime.UtcNow.ToString(),
        Size = "66"
      };

      log.LogDebug($"{nameof(UpdateProjectConfiguration)} Mock: projectConfigurationFileResponseModel {JsonConvert.SerializeObject(projectConfigurationFileResponseModel)}");
      return Task.FromResult(projectConfigurationFileResponseModel);
    }

    public Task DeleteProjectConfiguration(Guid projectUid, ProjectConfigurationFileType projectConfigurationFileType, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(DeleteProjectConfiguration)} Mock: projectUid {projectUid} projectConfigurationFileType {JsonConvert.SerializeObject(projectConfigurationFileType)}");
      return Task.CompletedTask;
    }
  }
}
