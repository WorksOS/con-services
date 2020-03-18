using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Project.WebAPI.Common.Utilities;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;

namespace VSS.Productivity3D.Project.Proxy
{
  /// <summary>
  /// This proxy is for the UI to get customer list etc as we have no CustomerSvc yet
  /// </summary>
  public class CustomerV5Proxy : BaseServiceDiscoveryProxy, ICustomerProxy
  {
    private IAccountClient _accountClient;

    public CustomerV5Proxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution,
      IAccountClient accountClient) 
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
      _accountClient = accountClient;
    }

    public  override bool IsInsideAuthBoundary => true;

    public  override ApiService InternalServiceType => ApiService.Account;

    public override string ExternalServiceName => null;

    public  override ApiVersion Version => ApiVersion.V5;

    public  override ApiType Type => ApiType.Public;

    public  override string CacheLifeKey => "ACCOUNT_CACHE_LIFE";

    /// <summary>
    /// list will include any customers associated with the User
    /// </summary>
    public async Task<CustomerDataResult> GetCustomersForMe(string userUid, IDictionary<string, string> customHeaders)
    {
      var accounts = await _accountClient.GetAccountsForUser(userUid, customHeaders);
      return AutoMapperUtility.Automapper.Map<CustomerDataResult>(accounts);
    }

    public async Task<List<CustomerData>> GetCustomersForUser(string userUid, IDictionary<string, string> customHeaders)
    {
      var result = await GetCustomersForMe(userUid, customHeaders);
      return result.customer;
    }

    public async Task<CustomerData> GetCustomerForUser(string userUid, string customerUid, IDictionary<string, string> customHeaders = null)
    {
      var result = await GetCustomersForMe(userUid, customHeaders);
      return result.customer.Where(c => c.uid == customerUid).FirstOrDefault();
    }

      public async Task<int> GetDeviceLicenses(string customerUid)
    {
      var deviceLicenseResponseModel = await _accountClient.GetDeviceLicenses(customerUid);
      return deviceLicenseResponseModel.Total;
    }

    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    /// <param name="uid">The uid of the item (deviceUid) to remove from the cache</param>
    /// <param name="userId">The user ID</param>
    public void ClearCacheItem(string uid, string userId = null)
    {
      ClearCacheByTag(uid);

      if (string.IsNullOrEmpty(userId))
        ClearCacheByTag(userId);
    }
  }
}
