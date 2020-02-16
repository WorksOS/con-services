using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Constants;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;

namespace VSS.Nighthawk.ReferenceIdentifierService.Controllers
{
  public class CredentialController : ApiController
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly ICredentialManager _worker;

    public CredentialController(ICredentialManager worker)
    {
      _worker = worker;
    }

    [HttpGet]
    public HttpResponseMessage Retrieve(HttpRequestMessage request, string url)
    {
      Log.IfInfoFormat("{0}.{1}: Received request for url {2}", GetType().Name, "Retrieve", url);

      string decodedUrl = HttpUtility.UrlDecode(url);
      LookupResponse<Credentials> response = new LookupResponse<Credentials>();
      try
      {
        if (ModelState.ContainsKey(ModelBinderConstants.IdentifierDefinitionModelBinderError))
          throw new ArgumentException(
            ModelState[ModelBinderConstants.IdentifierDefinitionModelBinderError].Errors[0].ErrorMessage,
            "identifierDefinition");

        response.Data = _worker.RetrieveByUrl(decodedUrl);

        return request.CreateResponse(HttpStatusCode.OK, response);
      }
      catch (Exception ex)
      {
        Log.IfWarn("Error retrieving asset reference", ex);
        response.Exception = ex;
        return request.CreateResponse(HttpStatusCode.InternalServerError, response);
      }
    }
  }
}
