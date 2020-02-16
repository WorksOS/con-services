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
  public class AssetSettingsController : ApiController
  {
    private readonly IAssetSettingsProcessor _processor;
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public AssetSettingsController(IAssetSettingsProcessor processor)
    {
      _processor = processor;
    }

    [ActionName(ActionConstants.GetAssetIdConfigurationChangedEvent)]
    public HttpResponseMessage GetAssetIdConfigurationChangedEvent(HttpRequestMessage request, [ModelBinder(typeof(DeviceQueryModelBinder))]IDeviceQuery device)
    {
      if (ModelState.ContainsKey(ModelBinderConstants.DeviceQueryModelBinderError))
      {
        return request.CreateResponse(HttpStatusCode.NotFound, ModelState[ModelBinderConstants.DeviceQueryModelBinderError].Errors[0].ErrorMessage);
      }

      Log.IfDebugFormat("{0} {1} processing {2} request", GetType().Name, "GetAssetIdConfigurationChangedEvent", ActionConstants.GetAssetIdConfigurationChangedEvent);
      IFactoryOutboundEventTypeDescriptor typeDescriptor = null;

      try
      {
        typeDescriptor = _processor.GetAssetIdConfigurationChangedEvent(device);
      }
      catch (UnknownDeviceException e)
      {
        Log.IfDebugFormat("{0} {1} UnknownDeviceException - {2}", GetType().Name, "GetAssetIdConfigurationChangedEvent", e.Message);        
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (NotImplementedException e)
      {
        Log.IfDebugFormat("{0} {1} NotImplementedException - {2}", GetType().Name, "GetAssetIdConfigurationChangedEvent", e.Message);
        return request.CreateResponse(HttpStatusCode.NotFound, e.Message);
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "{0} {1} Exception", GetType().Name, "GetAssetIdConfigurationChangedEvent");
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
      }

      return request.CreateResponse(HttpStatusCode.OK, typeDescriptor);
    }
  }
}
