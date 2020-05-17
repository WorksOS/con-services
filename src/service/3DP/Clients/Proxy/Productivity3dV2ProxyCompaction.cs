using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Productivity3D.Models.Compaction.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Productivity3D.Proxy
{
  public class Productivity3dV2ProxyCompaction : Productivity3dV2Proxy, IProductivity3dV2ProxyCompaction
  {
    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.Productivity3D;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V2;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "PRODUCTIVITY3D_COMPACTION_CACHE_LIFE"; // not used

    public Productivity3dV2ProxyCompaction(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public async Task<Stream> GetLineworkFromAlignment(Guid projectUid, Guid alignmentUid, IHeaderDictionary customHeaders)
    {
      var queryParams = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("projectUid", projectUid.ToString()),
        new KeyValuePair<string, string>("alignmentUid", alignmentUid.ToString())
      };

      var result = await GetMasterDataStreamItemServiceDiscoveryNoCache
        ("/linework/alignment", customHeaders, method: HttpMethod.Get, queryParameters: queryParams);
      if (result != null)
        return result;

      log.LogDebug($"{nameof(GetLineworkFromAlignment)} Failed to get streamed results");
      return null;
    }

    /// <summary>
    /// Get the statistics for a project.
    /// </summary>
    public async Task<ProjectStatisticsResult> GetProjectStatistics(Guid projectUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetProjectStatistics)} projectUid: {projectUid}");
      var queryParams = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("projectUid", projectUid.ToString()) };
      var response = await GetMasterDataItemServiceDiscoveryNoCache<ProjectStatisticsResult>("/projectstatistics", customHeaders, queryParams);

      return response;
    }

    /// <summary>
    /// Validates the Settings for the project.
    /// </summary>
    public async Task<BaseMasterDataResult> ValidateProjectSettings(Guid projectUid, string projectSettings, ProjectSettingsType settingsType, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(ValidateProjectSettings)} 2) projectUid: {projectUid} settings type: {settingsType}");
      var queryParams = new List<KeyValuePair<string, string>>
      {
        new KeyValuePair<string, string>("projectUid", projectUid.ToString()),
        new KeyValuePair<string, string>("projectSettings", JsonConvert.SerializeObject(projectSettings)),
        new KeyValuePair<string, string>("settingsType", settingsType.ToString())
      };
      var response = await GetMasterDataItemServiceDiscoveryNoCache<BaseMasterDataResult>("/validatesettings", customHeaders, queryParams);

      log.LogDebug($"{nameof(ValidateProjectSettings)} 2) response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");
      return response;
    }

    /// <summary>
    /// Validates the Settings for the project.
    /// </summary>
    public async Task<BaseMasterDataResult> ValidateProjectSettings(ProjectSettingsRequest request, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(ValidateProjectSettings)} 3) projectUid: {request.projectUid}");
      using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request))))
      {
        var result = await SendMasterDataItemServiceDiscoveryNoCache<BaseMasterDataResult>("/validatesettings", customHeaders, HttpMethod.Post, payload: ms);
        if (result.Code == 0)
          return result;
      }

      log.LogDebug($"{nameof(ValidateProjectSettings)} Failed to post request");
      return null;
    }

  }
}
