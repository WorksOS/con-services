using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Common.Abstractions.Clients.CWS.Interfaces
{
  public interface ICwsAccountClient
  {
    Task<AccountListResponseModel> GetMyAccounts(Guid userUid, IDictionary<string, string> customHeaders = null);
    Task<AccountListResponseModel> GetAccountsForUser(Guid userUid, IDictionary<string, string> customHeaders = null);
    Task<AccountResponseModel> GetAccountForUser(Guid userUid, Guid customerUid, IDictionary<string, string> customHeaders = null);
    Task<DeviceLicenseResponseModel> GetDeviceLicenses(Guid customerUid, IDictionary<string, string> customHeaders = null);
  }
}
