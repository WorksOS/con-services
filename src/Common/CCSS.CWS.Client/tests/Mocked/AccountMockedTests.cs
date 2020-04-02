﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Clients.CWS.Utilities;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;

namespace CCSS.CWS.Client.UnitTests.Mocked
{
  [TestClass]
  public class TRNHelperTests : BaseTestClass
  {
    protected override IServiceCollection SetupTestServices(IServiceCollection services)
    {
      services.AddSingleton(mockWebRequest.Object);
      services.AddSingleton(mockServiceResolution.Object);
      services.AddTransient<ICwsAccountClient, CwsAccountClient>();

      return services;
    }

    [TestMethod]
    public void GetMyAccountsTest()
    {
      const string userId = "trn::profilex:us-west-2:user:d79a392d-6513-46c1-baa1-75c537cf0c32";
      const string expectedId = "trn::profilex:us-west-2:account:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";
      const string expectedName = "Berthoud";
      const string route = "/users/me/accounts";

      var accountListModel = new AccountListResponseModel
      {
        HasMore = false,
        Accounts = new List<AccountResponseModel>()
        {
          new AccountResponseModel() {Id = expectedId, Name = expectedName}
        }
      };

      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get My Accounts", mockWebRequest, null, expectedUrl, HttpMethod.Get, accountListModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsAccountClient>();
        var result = await client.GetMyAccounts(TRNHelper.ExtractGuid(userId).Value);

        Assert.IsNotNull(result, "No result from getting my accounts");
        Assert.IsFalse(result.HasMore);
        Assert.IsNotNull(result.Accounts);
        Assert.AreEqual(1, result.Accounts.Count);
        Assert.AreEqual(TRNHelper.ExtractGuidAsString(expectedId), result.Accounts[0].Id);
        Assert.AreEqual(expectedName, result.Accounts[0].Name);
        return true;
      });
    }

    [TestMethod]
    [Ignore]
    public void GetAccountsForUserTest()
    {
      throw new NotImplementedException();
    }

    [TestMethod]
    [Ignore]
    public void GetAccountForUserTest()
    {
      throw new NotImplementedException();
    }

    [TestMethod]
    public void GetDeviceLicensesTest()
    {
      const string accountId = "trn::profilex:us-west-2:account:560c2a6c-6b7e-48d8-b1a5-e4009e2d4c97";

      var deviceLicensesModel = new DeviceLicenseResponseModel
      {
        Total = 57
      };
      var route = $"/accounts/{accountId}/devicelicense";
      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get Device Licenses", mockWebRequest, null, expectedUrl, HttpMethod.Get, deviceLicensesModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsAccountClient>();
        var result = await client.GetDeviceLicenses(TRNHelper.ExtractGuid(accountId).Value);

        Assert.IsNotNull(result, "No result from getting device licenses");
        Assert.AreEqual(57, result.Total);
        return true;
      });
    }
  }
}

