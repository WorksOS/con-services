using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.MasterData.Proxies
{
  /// <summary>
  /// Proxy for getting load/dump locations for asset cycles from unified productivity
  /// </summary>
  public class LoadDumpProxy : BaseProxy, ILoadDumpProxy
  {
    public LoadDumpProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache cache) : base(configurationStore, logger, cache)
    { }

    /// <summary>
    /// Gets a list of load/dump locations for all cycles for all assets for a project
    /// </summary>
    public async Task<List<LoadDumpLocation>> GetLoadDumpLocations(string projectUid, IHeaderDictionary customHeaders)
    {
      //Note: May not be able to use caching and GetContainedMasterDataList. Can always do directly like productivity3dProxy production data tile
      const string urlKey = "LOADDUMP_API_URL";
      string url = configurationStore.GetValueString(urlKey);
      log.LogDebug($"{nameof(GetLoadDumpLocations)}: urlKey: {urlKey}  url: {url}");

      var queryParams = $"?projectUid={projectUid}";
      var response = await GetContainedMasterDataList<LoadDumpResult>(projectUid, null, "LOADDUMP_CACHE_LIFE", urlKey, customHeaders, queryParams);
      return response.cycles;
    }
  }
}
