using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;

namespace VSS.Productivity3D.Project.Proxy
{
  public class AccountV5Proxy : BaseServiceDiscoveryProxy, IAccountProxy
  {
    public AccountV5Proxy(IWebRequest webRequest, IConfigurationStore configurationStore, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution) 
      : base(webRequest, configurationStore, logger, dataCache, serviceResolution)
    {
    }

    public  override bool IsInsideAuthBoundary => true;

    public  override ApiService InternalServiceType => ApiService.Account;

    public override string ExternalServiceName => null;

    public  override ApiVersion Version => ApiVersion.V5;

    public  override ApiType Type => ApiType.Public;

    public  override string CacheLifeKey => "ACCOUNT_CACHE_LIFE";

    public Task<DeviceLicenseResponseModel> GetDeviceLicenses(string accountUid)
    {
      // todoMaverick
      // hookup to existing link in ProjectSvc.AccountController
      throw new System.NotImplementedException();
    }

    /// <summary>
    /// Clears an item from the cache
    /// </summary>
    /// <param name="uid">The uid of the item (deviceUid) to remove from the cache</param>
    /// <param name="userId">The user ID</param>
    public void ClearCacheItem(string uid, string userId=null)
    {
      ClearCacheByTag(uid);

      if(string.IsNullOrEmpty(userId))
        ClearCacheByTag(userId);
    }
    
  }
}
