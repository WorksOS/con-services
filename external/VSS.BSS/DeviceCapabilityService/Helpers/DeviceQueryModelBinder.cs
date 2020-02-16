using System;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using log4net;
using Magnum.Reflection;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;
using ED = VSS.Nighthawk.ExternalDataTypes.Enumerations;
namespace VSS.Nighthawk.DeviceCapabilityService.Helpers
{
  public class DeviceQueryModelBinder : IModelBinder
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
    {
      var deviceQuery = FastActivator.Create(InterfaceImplementationBuilder.GetProxyFor(typeof(IDeviceQuery))) as IDeviceQuery;

      var assetid = bindingContext.ValueProvider.GetValue("assetid");
      var id = bindingContext.ValueProvider.GetValue("id");
      var gpsdeviceID = bindingContext.ValueProvider.GetValue("gpsdeviceid");
      var deviceType = bindingContext.ValueProvider.GetValue("devicetype");

      long assetIdValue;
      if (assetid != null && !string.IsNullOrEmpty(assetid.AttemptedValue) && long.TryParse(assetid.AttemptedValue, out assetIdValue))
      {
        Log.IfDebugFormat("Query contains asset id {0} for Asset", assetIdValue);
        deviceQuery.AssetID = assetIdValue;
      }

      long idValue;
      if (id != null && !string.IsNullOrEmpty(id.AttemptedValue) && long.TryParse(id.AttemptedValue, out idValue))
      {
        Log.IfDebugFormat("Query contains id {0} for Device", idValue);
        deviceQuery.ID = idValue;
      }

      if (gpsdeviceID != null && !string.IsNullOrEmpty(gpsdeviceID.AttemptedValue))
      {
        Log.IfDebugFormat("Query contains GpsDeviceID {0} for Device", gpsdeviceID.AttemptedValue);
        deviceQuery.GPSDeviceID = gpsdeviceID.AttemptedValue;
      }

      ED.DeviceTypeEnum deviceTypeValue;
      if (deviceType != null && !string.IsNullOrEmpty(deviceType.AttemptedValue) && Enum.TryParse(deviceType.AttemptedValue, out deviceTypeValue))
      {
        Log.IfDebugFormat("Query contains DeviceType {0} for Device", deviceTypeValue);
        deviceQuery.DeviceType = deviceTypeValue;
      }

      bindingContext.Model = deviceQuery;

      if (!deviceQuery.AssetID.HasValue && !deviceQuery.ID.HasValue &&
        ((!deviceQuery.DeviceType.HasValue && !string.IsNullOrEmpty(deviceQuery.GPSDeviceID)) || (deviceQuery.DeviceType.HasValue && string.IsNullOrEmpty(deviceQuery.GPSDeviceID))))
      {
        Log.IfWarn("Invalid request: query string does not have valid gpsdeviceID and deviceType");
        actionContext.ModelState.AddModelError(ModelBinderConstants.DeviceQueryModelBinderError, "Invalid request: query string does not have valid gpsdeviceID and deviceType");
        return false;
      }

      if (!deviceQuery.AssetID.HasValue && !deviceQuery.ID.HasValue && !deviceQuery.DeviceType.HasValue )
      {
        Log.IfWarn("Invalid request: query string does not have valid deviceID, deviceType, or assetID");
        actionContext.ModelState.AddModelError(ModelBinderConstants.DeviceQueryModelBinderError, "Invalid request: query string does not have valid deviceID, deviceType, or assetID");
        return false;
      }
      
      return true;
    }
  }
}
