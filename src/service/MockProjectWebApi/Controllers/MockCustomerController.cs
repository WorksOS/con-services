using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockCustomerController : BaseController
  {
    public MockCustomerController(ILoggerFactory loggerFactory) : base(loggerFactory)
    { }

    [Route("api/v1/mock/getcustomersforme")] // can remove this when 85920 merged.
    [Route("api/v1/Customers/me")]
    [HttpGet]
    public CustomerDataResult DummyGetCustomersForMe()
    {
      if (Request.Headers == null)
        return new CustomerDataResult { status = 500, metadata = new Metadata { msg = "CustomerProxy missing Authentication headers" } };

      var customerUid = Request.Headers["X-VisionLink-CustomerUID"];

      if (string.IsNullOrEmpty(customerUid))
        return new CustomerDataResult { status = 500, metadata = new Metadata { msg = "CustomerProxy missing customerUid" } };

      var cs = new CustomerDataResult
      {
        status = 200,
        metadata = new Metadata
        { msg = "success" },
        customer = new List<CustomerData>
                   {
                     new CustomerData { uid = customerUid, name = "customerName", type = "Customer" }
                   }
      };

      Logger.LogInformation($"DummyGetCustomersForMe: customerUid {customerUid}. CustomerDataResult {JsonConvert.SerializeObject(cs)}");

      return cs;
    }
  }
}
