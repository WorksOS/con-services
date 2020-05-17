using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client
{
  /// <summary>
  /// CWS identifies entities using a rather long TRN, 
  ///      however internally we would prefer to use a Guid: to minimize code changes and distance from potential cws swap-out
  /// Since ProfileX (where the TRNs come from) are currently only in 1 region, and there is no anticipated date to do multi-regions,
  ///      we can hide the TRN-complexity inside these cwsClients, and extract the unique Guid to pass around WorksOS/3dp
  /// </summary>
  public class CwsAccountClient : CwsProfileManagerClient, ICwsAccountClient
  {
    public CwsAccountClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    { }

    /// <summary>
    ///   user token
    ///   Observation: If you use an application token you get 10 accounts, but for user token you get 1. We will only use a user token
    /// </summary>
    public async Task<AccountListResponseModel> GetMyAccounts(Guid userUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetMyAccounts)}: userUid {userUid}");

      var accountListResponseModel = await GetData<AccountListResponseModel>("/users/me/accounts", null, userUid, null, customHeaders);
      foreach (var account in accountListResponseModel.Accounts)
      {
        account.Id = TRNHelper.ExtractGuidAsString(account.Id);
      }

      log.LogDebug($"{nameof(GetMyAccounts)}: accountListResponseModel {JsonConvert.SerializeObject(accountListResponseModel)}");
      return accountListResponseModel;
    }

    /// <summary>
    ///   user token
    /// </summary>
    public async Task<AccountResponseModel> GetMyAccount(Guid userUid, Guid customerUid, IHeaderDictionary customHeaders = null)
    {
      var accountListResponseModel = await GetMyAccounts(userUid, customHeaders);

      if (accountListResponseModel == null || accountListResponseModel.Accounts.Count == 0)
      {
        return null;
      }

      var accountResponseModel = accountListResponseModel.Accounts
        .Find(a => string.Equals(a.Id, customerUid.ToString(), StringComparison.InvariantCultureIgnoreCase));

      log.LogDebug($"{nameof(GetMyAccount)}: accountResponseModel {JsonConvert.SerializeObject(accountResponseModel)}");
      return accountResponseModel;
    }

    /// <summary>
    ///   application token and user token
    ///   used by UI to determine functionality allowed by user token
    ///   used by TFA using an application token
    /// </summary>
    public async Task<DeviceLicenseResponseModel> GetDeviceLicenses(Guid customerUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDeviceLicenses)}: customerUid {customerUid}");

      var accountTrn = TRNHelper.MakeTRN(customerUid, TRNHelper.TRN_ACCOUNT);
      var deviceLicenseResponseModel = await GetData<DeviceLicenseResponseModel>($"/accounts/{accountTrn}/devicelicense", customerUid, null, null, customHeaders);

      log.LogDebug($"{nameof(GetDeviceLicenses)}: deviceLicenseResponseModel {JsonConvert.SerializeObject(deviceLicenseResponseModel)}");
      return deviceLicenseResponseModel;
    }
  }
}
