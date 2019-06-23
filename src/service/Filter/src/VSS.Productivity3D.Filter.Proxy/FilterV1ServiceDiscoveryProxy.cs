using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Filter.Abstractions.Models.ResultHandling;

namespace VSS.Productivity3D.Filter.Proxy
{
  public class FilterV1ServiceDiscoveryProxy : BaseServiceDiscoveryProxy, IFilterServiceProxy
  {
    public FilterV1ServiceDiscoveryProxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public override bool IsInsideAuthBoundary => true;

    public override ApiService InternalServiceType => ApiService.Filter;

    public override string ExternalServiceName => null;

    public override ApiVersion Version => ApiVersion.V1;

    public override ApiType Type => ApiType.Public;

    public override string CacheLifeKey => "FILTER_CACHE_LIFE";

    public async Task<List<FilterDescriptor>> GetFilters(string projectUid, IDictionary<string, string> customHeaders = null)
    {
      var result = await GetMasterDataItemServiceDiscovery<FilterListData>
        ($"/filters/{projectUid}", projectUid, null, customHeaders);
      if (result.Code == 0)
      {
        return result.filterDescriptors;
      }

      log.LogWarning($"Failed to get Filter Descriptors: {result.Code}, {result.Message}");
      return null;
    }

    public async Task<FilterDescriptor> GetFilter(string projectUid, string filterUid, IDictionary<string, string> customHeaders = null)
    {
      var result = await GetMasterDataItemServiceDiscovery<FilterData>
      ($"/filter/{projectUid}", filterUid, null, customHeaders, new Dictionary<string, string> { { "filterUid", filterUid } });
      if (result.Code == 0)
      {
        return result.filterDescriptor;
      }

      log.LogWarning("Failed to get Filter Descriptor: {result.Code}, {result.Message}");
      return null;
    }


    public async Task<FilterDescriptorSingleResult> CreateFilter(string projectUid, FilterRequest request, IDictionary<string, string> customHeaders = null)
    {
      var jsonData = JsonConvert.SerializeObject(request);
      using (var payload = new MemoryStream(Encoding.UTF8.GetBytes(jsonData)))
      {
        // Need to await this, as we need the stream (if we return the task, the stream is disposed)
        return await PutMasterDataItemServiceDiscoveryNoCache<FilterDescriptorSingleResult>($"filter/{projectUid}", customHeaders, payload: payload);
      }
    }

    // can't remove these yet from IFilterServiceProxy as non serviceDiscovery is still used by 3dp
    [Obsolete("Use the Base ClearCacheByTag method")]
    public void ClearCacheItem(string uid, string userId = null)
    {
      ClearCacheByTag(uid);

      if (string.IsNullOrEmpty(userId))
        ClearCacheByTag(userId);
    }

    [Obsolete("Use the Base ClearCacheByTag method")]
    public void ClearCacheListItem(string projectUid, string userId = null)
    {
      ClearCacheItem(projectUid, userId);
    }

  }
}
