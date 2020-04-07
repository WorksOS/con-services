using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockCwsAccountController : BaseController
  {
    public MockCwsAccountController(ILoggerFactory loggerFactory) : base(loggerFactory)
    { }

    [Route("api/v1/users/me/accounts")]
    [HttpGet]
    public AccountListResponseModel GetMyAccounts([FromQuery] string userId)
    {
      var accountListResponseModel = new AccountListResponseModel()
      {
        Accounts = new List<AccountResponseModel>()
        {
          new AccountResponseModel
          {
            Id = Guid.NewGuid().ToString(),
            Name = "customerName"
          }
        }
      };

      Logger.LogInformation($"{nameof(GetMyAccounts)}: userId {userId}. accountListResponseModel {JsonConvert.SerializeObject(accountListResponseModel)}");

      return accountListResponseModel;
    }

    //[Route("api/v1/users/accounts")]
    //[HttpGet]
    //public AccountListResponseModel GetAccountsForUser([FromQuery] string userId, [FromQuery] string accountId)
    //{

    //  var accountListResponseModel = new AccountListResponseModel()
    //  {
    //    Accounts = new List<AccountResponseModel>()
    //    {
    //      new AccountResponseModel
    //      {
    //        Id = accountId,
    //        Name = "customerName"
    //      }
    //    }
    //  };

    //  Logger.LogInformation($"{nameof(GetAccountsForUser)}: userId {userId} accountId {accountId}. accountListResponseModel {JsonConvert.SerializeObject(accountListResponseModel)}");

    //  return accountListResponseModel;
    //}

    //[Route("api/v1/users/account")]
    //[HttpGet]
    //public AccountResponseModel GetAccountForUser([FromQuery] string userId, [FromQuery] string accountId)
    //{
    //  var accountResponseModel = new AccountResponseModel
    //  {
    //    Id = accountId,
    //    Name = "customerName"
    //  };

    //  Logger.LogInformation($"{nameof(GetAccountForUser)}: userId {userId} accountId {accountId}. accountResponseModel {JsonConvert.SerializeObject(accountResponseModel)}");

    //  return accountResponseModel;
    //}

    [Route("api/v1/accounts/{accountId}/devicelicense")]
    [HttpGet]
    public DeviceLicenseResponseModel GetDeviceLicenses(string accountId)
    {
      var deviceLicenseResponseModel = new DeviceLicenseResponseModel
      {
        Total = 10
      };

      Logger.LogInformation($"{nameof(GetDeviceLicenses)}: customerUid {accountId}. deviceLicenseResponseModel {JsonConvert.SerializeObject(deviceLicenseResponseModel)}");

      return deviceLicenseResponseModel;
    }
  }
}
