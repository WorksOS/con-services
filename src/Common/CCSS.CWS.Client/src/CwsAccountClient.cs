using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    /// </summary>
    public async Task<AccountListResponseModel> GetMyAccounts(Guid userUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetMyAccounts)}: userUid {userUid}");

      var accountListResponseModel = await GetData<AccountListResponseModel>("/users/me/accounts", null, userUid, null, customHeaders);
      // todoMaveric what if error?
      foreach (var account in accountListResponseModel.Accounts)
      {
        account.Id = TRNHelper.ExtractGuidAsString(account.Id);
      }

      log.LogDebug($"{nameof(GetMyAccounts)}: accountListResponseModel {JsonConvert.SerializeObject(accountListResponseModel)}");
      return accountListResponseModel;
    }

    /// <summary>
    /// https://api.trimble.com/t/trimble.com/cws-profilemanager/1.0/users/me/accounts
    ///   user token
    /// </summary>
    public async Task<AccountResponseModel> GetMyAccount(Guid userUid, Guid customerUid, IDictionary<string, string> customHeaders = null)
    {
      var accountListResponseModel = await GetMyAccounts(userUid, customHeaders);
  
      if (accountListResponseModel == null || !accountListResponseModel.Accounts.Any())
        return null;

      var accountResponseModel = accountListResponseModel.Accounts
        .Where(a => (string.Compare(a.Id, customerUid.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0))
        .FirstOrDefault();

      log.LogDebug($"{nameof(GetMyAccount)}: accountResponseModel {JsonConvert.SerializeObject(accountResponseModel)}");
      return accountResponseModel;
    }

    /// <summary>
    /// https://api.trimble.com/t/trimble.com/cws-profilemanager/1.0/accounts/{accountId}/devicelicense
    ///   application token and user token
    ///   used by UI to determine functionality allowed by user user token
    ///   used by TFA CCSSSCON-207? application token            
    /// </summary>
    public Task<DeviceLicenseResponseModel> GetDeviceLicenses(Guid customerUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDeviceLicenses)}: customerUid {customerUid}");

      var accountTrn = TRNHelper.MakeTRN(customerUid, TRNHelper.TRN_ACCOUNT);
      var deviceLicenseResponseModel = GetData<DeviceLicenseResponseModel>($"/accounts/{accountTrn}/devicelicense", customerUid, null, null, customHeaders);
      
      log.LogDebug($"{nameof(GetDeviceLicenses)}: deviceLicenseResponseModel {JsonConvert.SerializeObject(deviceLicenseResponseModel)}");
      return deviceLicenseResponseModel;
    }
  }
}
