using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
using Xunit;

namespace CCSS.CWS.Client.UnitTests.Mocked
{
  public class AccountMockedTests : BaseTestClass
  {
    protected override IServiceCollection SetupTestServices(IServiceCollection services)
    {
      services.AddSingleton(mockWebRequest.Object);
      services.AddSingleton(mockServiceResolution.Object);
      services.AddTransient<ICwsAccountClient, CwsAccountClient>();

      return services;
    }

    [Fact]
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
          new AccountResponseModel() {Id = expectedId, Name = expectedName, DeviceCount = 10, UserCount = 5, ProjectCount = 0}
        }
      };

      var expectedUrl = $"{baseUrl}{route}";
      mockServiceResolution.Setup(m => m.ResolveRemoteServiceEndpoint(
        It.IsAny<string>(), It.IsAny<ApiType>(), It.IsAny<ApiVersion>(), route, It.IsAny<IList<KeyValuePair<string, string>>>())).Returns(Task.FromResult(expectedUrl));

      MockUtilities.TestRequestSendsCorrectJson("Get My Accounts", mockWebRequest, null, expectedUrl, HttpMethod.Get, accountListModel, async () =>
      {
        var client = ServiceProvider.GetRequiredService<ICwsAccountClient>();
        var result = await client.GetMyAccounts(TRNHelper.ExtractGuid(userId).Value);

        Assert.NotNull(result);
        Assert.False(result.HasMore);
        Assert.NotNull(result.Accounts);
        Assert.Single(result.Accounts);
        Assert.Equal(TRNHelper.ExtractGuidAsString(expectedId), result.Accounts[0].Id);
        Assert.Equal(expectedName, result.Accounts[0].Name);
        Assert.Equal(accountListModel.Accounts[0].DeviceCount, result.Accounts[0].DeviceCount);
        Assert.Equal(accountListModel.Accounts[0].UserCount, result.Accounts[0].UserCount);
        Assert.Equal(accountListModel.Accounts[0].ProjectCount, result.Accounts[0].ProjectCount);
        return true;
      });
    }

    [Fact]
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

        Assert.NotNull(result);
        Assert.Equal(57, result.Total);
        return true;
      });
    }
  }
}

