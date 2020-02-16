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
  public class LocationUpdateRequestedController : ApiController
  {
    private readonly ILocationUpdateRequestedProcessor _locationProcessor;
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public LocationUpdateRequestedController(ILocationUpdateRequestedProcessor locationProcessor)
    {
      _locationProcessor = locationProcessor;
    }

    [ActionName(ActionConstants.GetLocationUpdateRequestEvent)]
    public HttpResponseMessage GetLocationUpdateRequestedEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "GetLocationUpdateRequestedEvent", ActionConstants.GetLocationUpdateRequestEvent);
      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _locationProcessor.GetLocationUpdateRequestedEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfWarnFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "GetLocationUpdateRequestedEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "GetLocationUpdateRequestedEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "GetLocationUpdateRequestedEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }
  }
}
