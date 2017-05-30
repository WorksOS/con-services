using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Models;
using MockProjectWebApi.Utils;
using Newtonsoft.Json;

namespace MockProjectWebApi.Controllers
{
  public class MockCustomerController : Controller
  {

    /// <summary>
    /// Dummies the post.
    /// </summary>
    [Route("api/v1/mock/GetCustomersForMe")]
    [HttpPost]
    public ContractExecutionResult DummyGetCustomersForMe(IDictionary<string, string> customHeaders)
    {
      if (customHeaders == null)
        return new ContractExecutionResult((int)HttpStatusCode.InternalServerError, "CustomerProxy missing Authenication headers");
      var customerUid = customHeaders["X-VisionLink-CustomerUID"];
      if (string.IsNullOrEmpty(customerUid))
        return new ContractExecutionResult((int)HttpStatusCode.InternalServerError, "CustomerProxy missing customerUid");

      var cs = new CustomerDataResult()
      { 
        CustomerDescriptors = new List<CustomerData>()
      };
      cs.CustomerDescriptors.Add(new CustomerData() {Uid = customerUid, Name = "customerName", Type = "Customer"});

      Console.WriteLine($"DummyGetCustomersForMe: customerUid {customerUid}. CustomerDataResult {JsonConvert.SerializeObject(cs)}");
      return cs;
    }
    
  }
}
