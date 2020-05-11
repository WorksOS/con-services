using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  /// <summary>
  /// Proxy to access the preference master data web api
  /// </summary>
  public class PreferenceProxy : BaseProxy, IPreferenceProxy
  {
    public PreferenceProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache cache) : base(configurationStore, logger, cache)
    {
    }
    /// <summary>
    /// Gets user preferences
    /// </summary>
    /// <returns></returns>
    public async Task<UserPreferenceData> GetUserPreferences(IDictionary<string, string> customHeaders=null)
    {
      var response = await GetMasterDataItem<UserPreferenceResult>("PREFERENCE_API_URL", customHeaders,  "?keyName=global", "/user");
      log.LogDebug($"{nameof(GetUserPreferences)} response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");

      return response == null ? null : JsonConvert.DeserializeObject<UserPreferenceData>(response.PreferenceJson);
    }

    /// <summary>
    /// Gets user preferences
    /// </summary>
    /// <returns></returns>
    public async Task<UserPreferenceData> GetShortCachedUserPreferences(string userId, TimeSpan invalidation, IDictionary<string, string> customHeaders = null)
    {
      var response = await GetMasterDataItem<UserPreferenceResult>("GlobalSettings", userId, invalidation, "PREFERENCE_API_URL", customHeaders, "/user?keyName=global");
      log.LogDebug($"{nameof(GetShortCachedUserPreferences)} response: {(response == null ? null : JsonConvert.SerializeObject(response).Truncate(_logMaxChar))}");

      return response == null ? null : JsonConvert.DeserializeObject<UserPreferenceData>(response.PreferenceJson);
    }
  }
}
