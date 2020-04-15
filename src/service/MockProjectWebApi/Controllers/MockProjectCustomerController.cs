using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockProjectCustomerController : BaseController
  {

    public MockProjectCustomerController(ILoggerFactory loggerFactory)
    : base(loggerFactory)
    {
    }

    [HttpGet("api/v1/Customers/me")]
    public CustomerV1ListResult GetCustomersForMe()
    {
      Logger.LogInformation($"{nameof(GetCustomersForMe)}");

      return new CustomerV1ListResult()
      {
        Customers = new List<CustomerData>()
          {
           new CustomerData()
           {
            uid = "8abcf851-44c5-e311-aa77-00505688274d",
            name = "3D Demo customer",
            type = CustomerType.Customer.ToString()
           }
          }
      };
    }

    [HttpGet("api/v1/customer/license/{customerUid}")]
    public CustomerV1DeviceLicenseResult GetCustomerDeviceLicense(string customerUid)
    {
      Logger.LogInformation($"{nameof(GetCustomerDeviceLicense)} for customerUid: {customerUid}");

      return new CustomerV1DeviceLicenseResult(10);
    }
  }
}
