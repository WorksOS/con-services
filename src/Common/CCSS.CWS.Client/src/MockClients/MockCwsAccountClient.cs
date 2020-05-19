using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client.MockClients
{
  /// <summary>
  /// Mocks to use until we can get the real endpoints
  /// </summary>
  public class MockCwsAccountClient : CwsProfileManagerClient, ICwsAccountClient
  {
    private const string Daves3dDemoCustomerTrn = "trn::profilex:us-west-2:account:8abcf851-44c5-e311-aa77-00505688274d";

    public MockCwsAccountClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    public Task<AccountListResponseModel> GetMyAccounts(Guid userUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetMyAccounts)} Mock: userUid {userUid}");

      var accountListResponseModel = new AccountListResponseModel()
      {
        Accounts = new List<AccountResponseModel>()
        {
          new AccountResponseModel
          {
            TRN = Daves3dDemoCustomerTrn,
            Name = "3D Demo customer"
          },
          new AccountResponseModel
          {
            TRN = "trn::profilex:us-west-2:account:158ef953-4967-4af7-81cc-952d47cb6c6f",
            Name = "WM test Trimble CEC march 26"
          }
        }
      };

      log.LogDebug($"{nameof(GetMyAccounts)} Mock: accountListResponseModel {JsonConvert.SerializeObject(accountListResponseModel)}");
      return Task.FromResult(accountListResponseModel);
    }

    /// </summary>
    public Task<AccountResponseModel> GetMyAccount(Guid userUid, Guid customerUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetMyAccount)} Mock: userUid {userUid} customerUid {customerUid}");

      if (Daves3dDemoCustomerTrn.Contains(customerUid.ToString()))
        return Task.FromResult(new AccountResponseModel()
        {
          TRN = Daves3dDemoCustomerTrn,
          Name = "3D Demo customer"
        });

      var accountResponseModel = new AccountResponseModel
      {
        TRN = TRNHelper.MakeTRN(customerUid.ToString(), TRNHelper.TRN_ACCOUNT),
        Name = "Got this other customer"
      };

      log.LogDebug($"{nameof(GetMyAccount)} Mock: accountResponseModel {JsonConvert.SerializeObject(accountResponseModel)}");
      return Task.FromResult(accountResponseModel);
    }

    public Task<DeviceLicenseResponseModel> GetDeviceLicenses(Guid customerUid, IHeaderDictionary customHeaders = null)
    {
      log.LogDebug($"{nameof(GetDeviceLicenses)} Mock: customerUid {customerUid}");

      var deviceLicenseResponseModel = new DeviceLicenseResponseModel
      {
        Total = 10
      };
      return Task.FromResult(deviceLicenseResponseModel);
    }
  }
}
