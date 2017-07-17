using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterDataProxies.Interfaces;

namespace VSS.MasterDataProxies
{
  /// <summary>
  /// Proxy to access the preference master data web api
  /// </summary>
  public class PreferenceProxy : BaseProxy, IPreferenceProxy
  {
    public PreferenceProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(configurationStore, logger, cache)
    {
    }
    /// <summary>
    /// Gets user preferences
    /// </summary>
    /// <returns></returns>
    public async Task<UserPreferenceData> GetUserPreferences(IDictionary<string, string> customHeaders=null)
    {
      var response = await GetItem<UserPreferenceResult>("PREFERENCE_API_URL", customHeaders, "/user?keyName=global");
      var message = string.Format("PreferenceProxy.GetUserPreferences: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));
      log.LogDebug(message);

      return response == null ? null : JsonConvert.DeserializeObject<UserPreferenceData>(response.PreferenceJson);
    }

  }
}
