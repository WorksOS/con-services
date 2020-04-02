using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Clients.CWS.Utilities;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  /// <summary>
  /// CWS identifies entities using a rather long TRN, 
  ///      however internally we would prefer to use a Guid: to minimise code changes adndistance from cws swap-out
  /// Since ProfileX (where the TRNs come from) are currently only in 1 region, and there is no antiicipated date to do multi-regions,
  ///      we can hide the TRN-complexity inside these CWSclients, and extract the unique Guid to pass around WorksOS/3dp
  /// </summary>
  public class CwsAccountClient : CwsProfileManagerClient, ICwsAccountClient
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
    public async Task<AccountListResponseModel> GetMyAccounts(Guid userUid, IDictionary<string, string> customHeaders = null)
    {
      var userTrn = TRNHelper.MakeTRN(userUid, TRNHelper.TRN_USER);
      var accountListResponseModel = await GetData<AccountListResponseModel>("/users/me/accounts", null, userUid.ToString(), null, customHeaders);
      // todoMaveric what if error?
      foreach (var account in accountListResponseModel.Accounts)
      {
        account.Id = TRNHelper.ExtractGuidAsString(account.Id);
      }
      return accountListResponseModel;
    }

    /// <summary>
    /// We need to query by userUid to allow override as per TIDAuthentication
    /// https://api.trimble.com/t/trimble.com/cws-profilemanager/1.0/users/{userId}/accounts?
    ///   application token
    ///   todoMaaverick where is this used ?
    ///                 what response fields are required?
    ///   CCSSCON-122
    /// </summary>
    public Task<AccountListResponseModel> GetAccountsForUser(Guid userUid, IDictionary<string, string> customHeaders = null)
    {
      var userTrn = TRNHelper.MakeTRN(userUid, TRNHelper.TRN_USER);
      var queryParameters = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("userId", userTrn) };
      return GetData<AccountListResponseModel>("/users/me/accounts", null, userUid.ToString(), queryParameters, customHeaders);
    }

    public async Task<AccountResponseModel> GetAccountForUser(Guid userUid, Guid customerUid, IDictionary<string, string> customHeaders = null)
    {
      var userTrn = TRNHelper.MakeTRN(userUid, TRNHelper.TRN_USER);
      var accountTrn = TRNHelper.MakeTRN(customerUid, TRNHelper.TRN_ACCOUNT);
      var queryParameters = new List<KeyValuePair<string, string>>{new KeyValuePair<string, string>( "userId", userTrn),
         new KeyValuePair<string, string>( "accountId", accountTrn )
        };

      var accountResponseModel = await GetData<AccountResponseModel>("/users/me/account", null, userUid.ToString(), queryParameters, customHeaders);
      
      // todoMaveric what if error?
      accountResponseModel.Id = TRNHelper.ExtractGuidAsString(accountResponseModel.Id);
      return accountResponseModel;
    }

    /// <summary>
    /// https://api.trimble.com/t/trimble.com/cws-profilemanager/1.0/accounts/{accountId}/devicelicense
    ///   application token
    ///   todoMaaverick where is this used ?
    ///                 what response fields are required?
    ///   CCSSCON-available                
    /// </summary>
    public Task<DeviceLicenseResponseModel> GetDeviceLicenses(Guid customerUid, IDictionary<string, string> customHeaders = null)
    {
      var accountTrn = TRNHelper.MakeTRN(customerUid, TRNHelper.TRN_ACCOUNT);
      return GetData<DeviceLicenseResponseModel>($"/accounts/{accountTrn}/devicelicense", customerUid.ToString(), null, null, customHeaders);
    }
  }
}
