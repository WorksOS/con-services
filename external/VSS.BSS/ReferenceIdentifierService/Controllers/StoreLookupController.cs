using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;

namespace VSS.Nighthawk.ReferenceIdentifierService.Controllers
{
  public class StoreLookupController : ApiController
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IStoreLookupManager _worker;

    public StoreLookupController(IStoreLookupManager worker)
    {
      _worker = worker;
    }

    [HttpGet]
    public HttpResponseMessage FindStoreByCustomerId(HttpRequestMessage request, long customerId)
    {
      Log.IfInfoFormat("{0}.{1}: Received request for {2}", GetType().Name, "FindStoreByCustomerId", customerId);
      var response = new LookupResponse<long>();

      try
      {
        response.Data = _worker.FindStoreByCustomerId(customerId);
        return request.CreateResponse(HttpStatusCode.OK, response);
      }
      catch (Exception ex)
      {
        Log.IfWarn("Error finding Store by CustomerId", ex);
        response.Exception = ex;
        return request.CreateResponse(HttpStatusCode.InternalServerError, response);
      }
    }
  }
}
