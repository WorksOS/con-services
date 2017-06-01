using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.GenericConfiguration;
using MasterDataProxies.Interfaces;
using MasterDataProxies.Models;

namespace MasterDataProxies
{
  /// <summary>
  /// Proxy to validate and post a CoordinateSystem with Raptor.
  /// </summary>
  public class CustomerProxy : BaseProxy, ICustomerProxy
  {
    private static TimeSpan customerCacheLife = new TimeSpan(0, 15, 0);
    public CustomerProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(configurationStore, logger, cache)
    {
    }

    /// <summary>
    /// list will include any customers (or dealers etc) associated with the User
    /// </summary>
    /// <param name="customHeaders"></param>
    /// <returns></returns>
    public async Task<CustomerDataResult> GetCustomersForMe(IDictionary < string, string> customHeaders)
    {
      // e.g. https://api-stg.trimble.com/t/trimble.com/vss-alpha-customerservice/1.0/customers/me
      var urlKey = "CUSTOMERSERVICE_API_URL";
      string url = configurationStore.GetValueString(urlKey);
      log.LogDebug($"CustomerProxy.GetCustomersForMe: urlKey: {urlKey}  url: {url} customHeaders {customHeaders}");

      var response = await GetContainedList<CustomerDataResult>("", customerCacheLife, urlKey, customHeaders);
      var message = string.Format("CustomerProxy.GetCustomersForMe: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));
      log.LogDebug(message);

      return response;
    }
  }
}