using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  public class LoadDumpProxy : BaseProxy, ILoadDumpProxy
  {
    public LoadDumpProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(configurationStore, logger, cache)
    {
    }

    public async Task<List<LoadDumpLocation>> GetLoadDumpLocations(string projectUid, IDictionary<string, string> customHeaders)
    {
      //single asset
      //https://unifiedproductivity.myvisionlink.com/t/trimble.com/vss-unifiedproductivity/1.0/productivity/assets/4553dc81-da9d-11e7-80fe-06dbf546f101/cycles?endDate=2018-09-23&isCompleteCycle=true&startDate=2018-09-17

      //Note: May not be able to use caching and GetContainedMasterDataList. Can always do directly like raptorProxy production data tile
      const string urlKey = "LOADDUMP_API_URL";
      string url = configurationStore.GetValueString(urlKey);
      log.LogDebug($"LoadDumpProxy.GetLoadDumpLocations: urlKey: {urlKey}  url: {url} customHeaders: {JsonConvert.SerializeObject(customHeaders)}");

      var queryParams = $"?projectUid={projectUid}";
      var response = await GetContainedMasterDataList<LoadDumpResult>(projectUid, null, "LOADDUMP_CACHE_LIFE", urlKey, customHeaders, queryParams);
      return response.cycles;
    }
  }
}
