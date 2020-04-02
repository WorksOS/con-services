using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Abstractions.Clients.CWS.Utilities;

namespace MockProjectWebApi.Controllers
{
  public class MockCwsCustomerController : BaseController
  {
    public MockCwsCustomerController(ILoggerFactory loggerFactory) : base(loggerFactory)
    { }

    [Route("api/v1/users/me/accounts")]
    [HttpGet]
    public AccountListResponseModel GetAccountsForUser([FromQuery] string userId)
    {
      var accountListResponseModel = new AccountListResponseModel()
      {
        Accounts = new List<AccountResponseModel>()
        {
          new AccountResponseModel
          {
            Id = TRNHelper.MakeTRN(Guid.NewGuid(), TRNHelper.TRN_ACCOUNT),
            Name = "customerName"
          }
        }
      };

      Logger.LogInformation($"GetAccountForUser: userId {userId}. CustomerDataResult {JsonConvert.SerializeObject(accountListResponseModel)}");

      return accountListResponseModel;
    }

    [Route("api/v1/users/me/account")]
    [HttpGet]
    public AccountResponseModel GetAccountForUser([FromQuery] string userId, [FromQuery] string accountId)
    {
      var accountResponseModel = new AccountResponseModel
      {
        Id = accountId,
        Name = "customerName"
      };

      Logger.LogInformation($"GetAccountForUser: userId {userId} accountId {accountId}. CustomerDataResult {JsonConvert.SerializeObject(accountResponseModel)}");

      return accountResponseModel;
    }

    [Route("api/v1/accounts/{accountId}/devicelicense")]
    [HttpGet]
    public DeviceLicenseResponseModel GetDeviceLicenses(string accountId)
    {
      var deviceLicenseResponseModel = new DeviceLicenseResponseModel
      {
        Total = 10
      };

      Logger.LogInformation($"GetDeviceLicenses: customerUid {accountId}. CustomerDataResult {JsonConvert.SerializeObject(deviceLicenseResponseModel)}");

      return deviceLicenseResponseModel;
    }
  }
}
