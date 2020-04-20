using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Cache.Interfaces;
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
    private const string Daves3dDemoCustomerUid = "8abcf851-44c5-e311-aa77-00505688274d";
    public MockCwsAccountClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    /// </summary>
    public Task<AccountListResponseModel> GetMyAccounts(Guid userUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetMyAccounts)} Mock: userUid {userUid}");

      var accountListResponseModel = new AccountListResponseModel()
      {
        Accounts = new List<AccountResponseModel>()
        {
          new AccountResponseModel
          {
            Id = Daves3dDemoCustomerUid,
            Name = "3D Demo customer"
          }
        }
      };

      log.LogDebug($"{nameof(GetMyAccounts)} Mock: accountListResponseModel {JsonConvert.SerializeObject(accountListResponseModel)}");
      return Task.FromResult(accountListResponseModel);
    }

    /// </summary>
    public Task<AccountResponseModel> GetMyAccount(Guid userUid, Guid customerUid, IDictionary<string, string> customHeaders = null)
    {
      log.LogDebug($"{nameof(GetMyAccount)} Mock: userUid {userUid} customerUid {customerUid}");

      if (string.Compare(customerUid.ToString(), Daves3dDemoCustomerUid, StringComparison.OrdinalIgnoreCase) == 0)
        return Task.FromResult(new AccountResponseModel()
        {
          Id = Daves3dDemoCustomerUid,
          Name = "3D Demo customer"
        });

      var accountResponseModel = new AccountResponseModel
      {
        Id = customerUid.ToString(),
        Name = "Got this other customer"
      };

      log.LogDebug($"{nameof(GetMyAccount)} Mock: accountResponseModel {JsonConvert.SerializeObject(accountResponseModel)}");
      return Task.FromResult(accountResponseModel);
    }

    public Task<DeviceLicenseResponseModel> GetDeviceLicenses(Guid customerUid, IDictionary<string, string> customHeaders = null)
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
