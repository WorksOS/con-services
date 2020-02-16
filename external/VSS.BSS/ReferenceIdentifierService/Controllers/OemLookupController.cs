using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;
using VSS.Nighthawk.ReferenceIdentifierService.Workers;

namespace VSS.Nighthawk.ReferenceIdentifierService.Controllers
{
  public class OemLookupController : ApiController
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IOemLookupManager _worker;

    public OemLookupController(IOemLookupManager worker)
    {
      _worker = worker;
    }

    [HttpGet]
    public HttpResponseMessage FindOemIdentifierByCustomerId(HttpRequestMessage request, long customerId)
    {
      Log.IfInfoFormat("{0}.{1}: Received request for {2}", GetType().Name, "FindOemIdentifierByCustomerId", customerId);
      var response = new LookupResponse<int>();

      try
      {
        response.Data = _worker.FindOemIdentifierByCustomerId(customerId);
        return request.CreateResponse(HttpStatusCode.OK, response);
      }
      catch (Exception ex)
      {
        Log.IfWarn("Error finding OemIdentifier by CustomerId", ex);
        response.Exception = ex;
        return request.CreateResponse(HttpStatusCode.InternalServerError, response);
      }
    }
  }
}
