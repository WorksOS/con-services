using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
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
    [Route("api/v1/mock/getcustomersforme")]
    [HttpGet]
    public CustomerDataResult DummyGetCustomersForMe(HttpContext context)
    {
      var theCount = context.Request.Headers.Count;
      if (context.Request.Headers == null)
        return new CustomerDataResult(ContractExecutionStatesEnum.InternalProcessingError, "CustomerProxy missing Authenication headers");
      var customerUid = context.Request.Headers["X-VisionLink-CustomerUID"];
      if (string.IsNullOrEmpty(customerUid))
        return new CustomerDataResult(ContractExecutionStatesEnum.InternalProcessingError, "CustomerProxy missing customerUid");

      var cs = new CustomerDataResult(0, ContractExecutionResult.DefaultMessage)
      { 
        CustomerDescriptors = new List<CustomerData>()
      };
      cs.CustomerDescriptors.Add(new CustomerData() {Uid = customerUid, Name = "customerName", Type = "Customer"});

      Console.WriteLine($"DummyGetCustomersForMe: customerUid {customerUid}. CustomerDataResult {JsonConvert.SerializeObject(cs)}");
      return cs;
    }
    
  }
}
