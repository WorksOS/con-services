using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Common.Abstractions.Clients.CWS.Interfaces
{
  public interface ICwsAccountClient
  {
    Task<AccountListResponseModel> GetMyAccounts(Guid userUid, IHeaderDictionary customHeaders = null);
    Task<AccountResponseModel> GetMyAccount(Guid userUid, Guid customerUid, IHeaderDictionary customHeaders = null);
    Task<DeviceLicenseResponseModel> GetDeviceLicenses(Guid customerUid, IHeaderDictionary customHeaders = null);
  }
}
