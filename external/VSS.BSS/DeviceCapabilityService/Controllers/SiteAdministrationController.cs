using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.DeviceCapabilityService.Helpers;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.DTOs;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Controllers
{
  public class SiteAdministrationController : ApiController
  {
    private readonly ISiteAdministrationProcessor _processor;
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public SiteAdministrationController(ISiteAdministrationProcessor processor)
    {
      _processor = processor;
    }

    [ActionName(ActionConstants.GetSiteDispatchedEvent)]
    public HttpResponseMessage GetSiteDispatchedEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "GetSiteDispatchedEvent", ActionConstants.GetSiteDispatchedEvent);
      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _processor.GetSiteDispatchedEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfDebugFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "GetSiteDispatchedEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "GetSiteDispatchedEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "GetSiteDispatchedEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }

    [ActionName(ActionConstants.GetSiteRemovedEvent)]
    public HttpResponseMessage GetSiteRemovedEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "GetSiteRemovedEvent", ActionConstants.GetSiteRemovedEvent);

      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _processor.GetSiteRemovedEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfDebugFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "GetSiteRemovedEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "GetSiteRemovedEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "GetSiteRemovedEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }
  }
}