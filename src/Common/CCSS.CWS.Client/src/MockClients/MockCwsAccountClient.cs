using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
    public MockCwsAccountClient(IWebRequest gracefulClient, IConfigurationStore configuration, ILoggerFactory logger, IDataCache dataCache, IServiceResolution serviceResolution)
      : base(gracefulClient, configuration, logger, dataCache, serviceResolution)
    {
    }

    /// </summary>
    public Task<AccountListResponseModel> GetMyAccounts(Guid userUid, IDictionary<string, string> customHeaders = null)
    {
      var accountListResponseModel = new AccountListResponseModel()
      {
        Accounts = new List<AccountResponseModel>()
        {
          new AccountResponseModel
          {
            Id = "8abcf851-44c5-e311-aa77-00505688274d",
            Name = "3D Demo customer"
          }
        }
      };
      return Task.FromResult(accountListResponseModel);
    }

    /// </summary>
    public Task<AccountResponseModel> GetMyAccount(Guid userUid, Guid customerUid, IDictionary<string, string> customHeaders = null)
    {
      if (string.Compare(customerUid.ToString(), "8abcf851-44c5-e311-aa77-00505688274d", StringComparison.OrdinalIgnoreCase) == 0)
        return Task.FromResult(new AccountResponseModel()
        {
          Id = "8abcf851-44c5-e311-aa77-00505688274d",
          Name = "3D Demo customer"
        });

      var accountListResponseModel = GetMyAccounts(userUid, customHeaders).Result;

      if (accountListResponseModel == null || !accountListResponseModel.Accounts.Any())
        return null;

      return Task.FromResult(accountListResponseModel.Accounts
        .Where(a => string.Compare(a.Id, customerUid.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0)
        .FirstOrDefault());
    }

    public Task<DeviceLicenseResponseModel> GetDeviceLicenses(Guid customerUid, IDictionary<string, string> customHeaders = null)
    {
      var deviceLicenseResponseModel = new DeviceLicenseResponseModel
      {
        Total = 10
      };
      return Task.FromResult(deviceLicenseResponseModel);
    }
  }
}
