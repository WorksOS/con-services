using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.CWS.Client.UnitTests.Mocked
{
  [TestClass]
  public class AccountMockedTests : BaseTestClass
  {
    private string baseUrl;

    private Mock<IWebRequest> mockWebRequest = new Mock<IWebRequest>();

    protected override IServiceCollection SetupTestServices(IServiceCollection services, IConfigurationStore configuration)
    {
      baseUrl = configuration.GetValueString(BaseClient.CWS_PROFILEMANAGER_URL_KEY);

      services.AddSingleton(mockWebRequest.Object);
      services.AddTransient<ICwsAccountClient, CwsAccountClient>();

      return services;
    }

    [TestMethod]
    public void Test_GetMyAccounts()
    {
      const string expectedId = "trn::profilex:us-west-2:account:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      const string expectedName = "Berthoud";

      var accountListModel = new AccountListResponseModel
      {
        HasMore = false,
        Accounts = new List<AccountResponseModel>()
        {
          new AccountResponseModel() {Id = expectedId, Name = expectedName}
        }
      };
      var expectedUrl = $"{baseUrl}/users/me/accounts";

      MockUtilities.TestRequestSendsCorrectJson("Get My Accounts", mockWebRequest, null, expectedUrl, HttpMethod.Get, accountListModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsAccountClient>();
        var result = await client.GetMyAccounts();

        Assert.IsNotNull(result, "No result from getting my accounts");
        Assert.IsFalse(result.HasMore);
        Assert.IsNotNull(result.Accounts);
        Assert.AreEqual(1, result.Accounts.Count);
        Assert.AreEqual(result.Accounts[0].Id, expectedId);
        Assert.AreEqual(result.Accounts[0].Name, expectedName);
        return true;
      });
    }

    [TestMethod]
    [Ignore]
    public void Test_GetAccountsForUser()
    {
      throw new NotImplementedException();
    }

    [TestMethod]
    [Ignore]
    public void Test_GetAccountForUser()
    {
      throw new NotImplementedException();
    }

    [TestMethod]
    public void Test_GetDeviceLicenses()

    {
      const string accountId = "trn::profilex:us-west-2:account:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";

      var deviceLicensesModel = new DeviceLicenseResponseModel
      {
        Total = 57
      };
      var expectedUrl = $"{baseUrl}/accounts/{accountId}/devicelicense";

      MockUtilities.TestRequestSendsCorrectJson("Get Device Licenses", mockWebRequest, null, expectedUrl, HttpMethod.Get, deviceLicensesModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsAccountClient>();
        var result = await client.GetDeviceLicenses(accountId);

        Assert.IsNotNull(result, "No result from getting device licenses");
        Assert.AreEqual(57, result.Total);
        return true;
      });
    }
  }
}
