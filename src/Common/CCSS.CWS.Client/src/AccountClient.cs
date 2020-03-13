
using System.Collections.Generic;
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

    public Task<DeviceLicenseResponseModel> GetDeviceLicenses(string accountId, IDictionary<string, string> customHeaders = null)
    {
      return GetData<DeviceLicenseResponseModel>($"/accounts/{accountId}/devicelicense", null, customHeaders);
    }
  }
}
