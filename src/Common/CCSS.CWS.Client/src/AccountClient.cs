using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  public class AccountClient : BaseClient, IAccountClient
  {
    public AccountClient(IConfigurationStore configuration, ILoggerFactory logger, IWebRequest gracefulClient) : base(configuration, logger, gracefulClient)
    {
    }

    public Task<AccountListResponseModel> GetMyAccounts(IDictionary<string, string> customHeaders = null)
    {
      return GetData<AccountListResponseModel>("/users/me/accounts", null, customHeaders);
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
      var myAccounts = await GetData<AccountListResponseModel>("/users/me/accounts", null, customHeaders);
      return myAccounts.Accounts.Where(a => a.Id == accountId).FirstOrDefault();
    }

    public async Task<DeviceLicenseResponseModel> GetDeviceLicenses(string accountId, IDictionary<string, string> customHeaders = null)
    {
      return await GetData<DeviceLicenseResponseModel>($"/accounts/{accountId}/devicelicense", null, customHeaders);
    }
  }
}
