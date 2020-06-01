using System.Collections.Generic;
using CCSS.CWS.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS;
using VSS.Common.Abstractions.Clients.CWS.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockCwsAccountController : BaseController
  {
    public MockCwsAccountController(ILoggerFactory loggerFactory) : base(loggerFactory)
    { }

    [Route("api/v1/users/me/accounts")]
    [HttpGet]
    public AccountListResponseModel GetMyAccounts()
    {
      var accountListResponseModel = new AccountListResponseModel()
      {
        Accounts = new List<AccountResponseModel>()
        {
          new AccountResponseModel
          {
            TRN = TRNHelper.MakeTRN(HttpContext.Request.Headers["X-VisionLink-CustomerUID"], TRNHelper.TRN_ACCOUNT),
            Name = "Customer from header in TIDAuthentication"
          }
        }
      };

      Logger.LogInformation($"{nameof(GetMyAccounts)}: accountListResponseModel {JsonConvert.SerializeObject(accountListResponseModel)}");

      return accountListResponseModel;
    }

    [Route("api/v1/accounts/{accountTrn}/devicelicense")]
    [HttpGet]
    public DeviceLicenseResponseModel GetDeviceLicenses(string accountTrn)
    {
      var deviceLicenseResponseModel = new DeviceLicenseResponseModel
      {
        Total = 10
      };

      Logger.LogInformation($"{nameof(GetDeviceLicenses)}: accountTrn {accountTrn}. deviceLicenseResponseModel {JsonConvert.SerializeObject(deviceLicenseResponseModel)}");

      return deviceLicenseResponseModel;
    }
  }
}
