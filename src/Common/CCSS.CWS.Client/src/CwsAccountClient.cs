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

    public Task<AccountListResponseModel> GetMyAccounts(string userId, IDictionary<string, string> customHeaders = null)
    {
      // todoMaverick odd, but this only seems to require an application token
      return GetData<AccountListResponseModel>("/users/me/accounts", null, userId, null, customHeaders);
    }

    public Task<AccountListResponseModel> GetAccountsForUser(string userId, IDictionary<string, string> customHeaders = null)
    {
      // todoMaverick need to query by userUid to allow override as per TIDAuthentication
      // CCSSCON-122
      throw new NotImplementedException();
    }

    public async Task<AccountResponseModel> GetAccountForUser(string userId, string accountId, IDictionary<string, string> customHeaders = null)
    {
      // todoMaverick call GetAccountsForUser() when ready
      var myAccounts = await GetData<AccountListResponseModel>("/users/me/accounts", null, userId, null, customHeaders);
      return myAccounts.Accounts.Where(a => a.Id == accountId).FirstOrDefault();
    }

    public async Task<DeviceLicenseResponseModel> GetDeviceLicenses(string accountId, IDictionary<string, string> customHeaders = null)
    {
      // todoMaverick require an application token
      return await GetData<DeviceLicenseResponseModel>($"/accounts/{accountId}/devicelicense", accountId, null, null, customHeaders);
    }
  }
}
