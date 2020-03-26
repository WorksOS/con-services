using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Common.Abstractions.Clients.CWS.Interfaces
{
  public interface ICwsAccountClient
  {
    Task<AccountListResponseModel> GetMyAccounts(string userId, IDictionary<string, string> customHeaders = null);
    Task<AccountListResponseModel> GetAccountsForUser(string userId, IDictionary<string, string> customHeaders = null);
    Task<AccountResponseModel> GetAccountForUser(string userId, string accountId, IDictionary<string, string> customHeaders = null);
    Task<DeviceLicenseResponseModel> GetDeviceLicenses(string accountId, IDictionary<string, string> customHeaders = null);
  }
}
