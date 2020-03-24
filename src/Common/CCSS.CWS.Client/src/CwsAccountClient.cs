using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  public class CwsAccountClient : BaseClient, ICwsAccountClient
  {
    public CwsAccountClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    /// <summary>
    /// https://api.trimble.com/t/trimble.com/cws-profilemanager/1.0/users/me/accounts
    ///   user token
    ///   todoMaaverick where is this used ?
    ///                 what response fields are required?
    ///   CCSSCON- available
    /// </summary>
    public Task<AccountListResponseModel> GetMyAccounts(string userId, IDictionary<string, string> customHeaders = null)
    {
      return GetData<AccountListResponseModel>("/users/me/accounts", null, userId, null, customHeaders);
    }

    /// <summary>
    /// We need to query by userUid to allow override as per TIDAuthentication
    /// https://api.trimble.com/t/trimble.com/cws-profilemanager/1.0/users/{userId}/accounts?
    ///   application token
    ///   todoMaaverick where is this used ?
    ///                 what response fields are required?
    ///   CCSSCON-122
    /// </summary>
    public Task<AccountListResponseModel> GetAccountsForUser(string userId, IDictionary<string, string> customHeaders = null)
    {
      throw new NotImplementedException();
    }

    public async Task<AccountResponseModel> GetAccountForUser(string userId, string accountId, IDictionary<string, string> customHeaders = null)
    {
      // todoMaverick call GetAccountsForUser() when ready
      var myAccounts = await GetData<AccountListResponseModel>("/users/me/accounts", null, userId, null, customHeaders);
      return myAccounts.Accounts.Where(a => a.Id == accountId).FirstOrDefault();
    }

    /// <summary>
    /// https://api.trimble.com/t/trimble.com/cws-profilemanager/1.0/accounts/{accountId}/devicelicense
    ///   application token
    ///   todoMaaverick where is this used ?
    ///                 what response fields are required?
    ///   CCSSCON-available                
    /// </summary>
    public async Task<DeviceLicenseResponseModel> GetDeviceLicenses(string accountId, IDictionary<string, string> customHeaders = null)
    {
      return await GetData<DeviceLicenseResponseModel>($"/accounts/{accountId}/devicelicense", accountId, null, null, customHeaders);
    }
  }
}
