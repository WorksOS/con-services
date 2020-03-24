using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Common.Abstractions.Clients.CWS.Interfaces
{
  public interface IAccountClient
  {
    Task<AccountListResponseModel> GetMyAccounts(string userId, IDictionary<string, string> customHeaders = null);

    Task<DeviceLicenseResponseModel> GetDeviceLicenses(string accountId, IDictionary<string, string> customHeaders = null);
  }
}
