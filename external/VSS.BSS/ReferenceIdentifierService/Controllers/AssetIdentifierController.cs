using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ReferenceIdentifierService.Helpers;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Constants;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.DTOs;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Requests;

namespace VSS.Nighthawk.ReferenceIdentifierService.Controllers
{
  public class AssetIdentifierController : ApiController
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IAssetIdentifierManager _worker;

    public AssetIdentifierController(IAssetIdentifierManager worker)
    {
      _worker = worker;
    }

    [HttpPost]
    public HttpResponseMessage Create(HttpRequestMessage request, [ModelBinder(typeof(IdentifierDefinitionModelBinder))]IdentifierDefinition identifierDefinition)
    {
      Log.IfInfoFormat("{0}.{1}: Received request for {2}", GetType().Name, "Create", JsonConvert.SerializeObject(identifierDefinition));

      var response = new LookupResponse<Guid?>();
      response.Data = identifierDefinition.UID; 
      try
      {
        if (ModelState.ContainsKey(ModelBinderConstants.IdentifierDefinitionModelBinderError))
          throw new ArgumentException(
            ModelState[ModelBinderConstants.IdentifierDefinitionModelBinderError].Errors[0].ErrorMessage,
            "identifierDefinition");

        if (identifierDefinition.UID == Guid.Empty)
          throw new ArgumentException("UID cannot be empty", "identifierDefinition");

        _worker.Create(identifierDefinition);

        return request.CreateResponse(HttpStatusCode.OK, response);
      }
      catch (Exception ex)
      {
        Log.IfWarn("Error creating asset reference", ex);
        response.Exception = ex;
        return request.CreateResponse(HttpStatusCode.InternalServerError, response);
      }
    }

    [HttpGet]
    public HttpResponseMessage Retrieve(HttpRequestMessage request, [ModelBinder(typeof(IdentifierDefinitionModelBinder))]IdentifierDefinition identifierDefinition)
    {
      Log.IfInfoFormat("{0}.{1}: Received request for {2}", GetType().Name, "Retrieve", JsonConvert.SerializeObject(identifierDefinition));

      var response = new LookupResponse<Guid?>();
      try
      {
        if (ModelState.ContainsKey(ModelBinderConstants.IdentifierDefinitionModelBinderError))
          throw new ArgumentException(
            ModelState[ModelBinderConstants.IdentifierDefinitionModelBinderError].Errors[0].ErrorMessage,
            "identifierDefinition");

        response.Data = _worker.Retrieve(identifierDefinition);

        return request.CreateResponse(HttpStatusCode.OK, response);
      }
      catch (Exception ex)
      {
        Log.IfWarn("Error retrieving asset reference", ex);
        response.Exception = ex;
        return request.CreateResponse(HttpStatusCode.InternalServerError, response);
      }
    }

    [HttpGet]
    public HttpResponseMessage GetAssociatedDevices(HttpRequestMessage request, Guid assetUid)
    {
      Log.IfInfoFormat("{0}.{1}: Received request for {2}", GetType().Name, "GetAssociatedDevices", assetUid);

      var response = new LookupResponse<IList<Guid>>();

      try
      {
        response.Data = _worker.GetAssociatedDevices(assetUid);
        return request.CreateResponse(HttpStatusCode.OK, response);
      }
      catch(Exception ex)
      {
        Log.IfWarn("Error Getting Associated Devices", ex);

        response.Exception = ex;
        return request.CreateResponse(HttpStatusCode.InternalServerError, response);
      }
    }

    [HttpGet]
    public HttpResponseMessage FindOwner(HttpRequestMessage request, Guid assetUid)
    {
      Log.IfInfoFormat("{0}.{1}: Received request for {2}", GetType().Name, "FindOwner", assetUid);

      var response = new LookupResponse<Guid?>();

      try
      {
        response.Data = _worker.FindOwner(assetUid);
        return request.CreateResponse(HttpStatusCode.OK, response);
      }
      catch (Exception ex)
      {
        Log.IfWarn("Error Getting Associated Devices", ex);

        response.Exception = ex;
        return request.CreateResponse(HttpStatusCode.InternalServerError, response);
      }
    }
  }
}