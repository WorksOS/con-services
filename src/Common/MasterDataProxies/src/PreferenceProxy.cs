using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  /// <summary>
  /// Proxy to access the preference master data web api
  /// </summary>
  public class PreferenceProxy : BaseServiceDiscoveryProxy, IPreferenceProxy
  {
    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.Preferences;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V1;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "PREFERENCE_CACHE_LIFE";

    public PreferenceProxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    /// <summary>
    /// Gets user preferences
    /// </summary>
    public async Task<UserPreferenceData> GetUserPreferences(string userId, IHeaderDictionary customHeaders = null)
    {
      var queryParams = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("keyName", "global") };
      var response = await GetMasterDataItemServiceDiscovery<UserPreferenceResult>("/user",
        null, userId, customHeaders, queryParams);
      log.LogDebug($"{nameof(GetUserPreferences)} response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(LogMaxChar))}");

      return response == null ? null : JsonConvert.DeserializeObject<UserPreferenceData>(response.PreferenceJson);
    }

    /// <summary>
    /// Gets user preferences
    /// </summary>
    public async Task<UserPreferenceData> GetShortCachedUserPreferences(string userId, TimeSpan invalidation, IHeaderDictionary customHeaders = null)
    {
      var queryParams = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("keyName", "global") };
      var response = await GetMasterDataItemServiceDiscovery<UserPreferenceResult>("/user",
        "GlobalSettings", userId, customHeaders, queryParams, cacheLife: invalidation);
      log.LogDebug($"{nameof(GetShortCachedUserPreferences)} response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(LogMaxChar))}");

      return response == null ? null : JsonConvert.DeserializeObject<UserPreferenceData>(response.PreferenceJson);
    }
  }
}
