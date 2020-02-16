using System.Reflection;
using log4net;
using VSS.Hosted.VLCommon;
using ED = VSS.Nighthawk.ExternalDataTypes.Enumerations;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Helpers
{
  public class DeviceQueryHelper : IDeviceQueryHelper
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public ED.DeviceTypeEnum? GetDeviceType(IDeviceQuery device, IStorage storage)
    {
      ED.DeviceTypeEnum? deviceType;
      if (device.DeviceType.HasValue)
        deviceType = device.DeviceType;
      else if (device.ID.HasValue)
        deviceType = storage.GetDeviceTypeForDevice(device.ID.Value);
      else if (device.AssetID.HasValue)
        deviceType = storage.GetDeviceTypeForAsset(device.AssetID.Value);
      else
        deviceType = ED.DeviceTypeEnum.MANUALDEVICE;

      Log.IfDebugFormat("Device type for {0} is {1}", GetPrintableValues(device), deviceType);
      return deviceType;
    }

    public string GetPrintableValues(IDeviceQuery device)
    {
      if (device.DeviceType.HasValue)
      {
        return string.Format("GPSDeviceID={0} DeviceType={1}", device.GPSDeviceID, device.DeviceType);
      }
      if (device.ID.HasValue)
      {
        return string.Format("ID={0}", device.ID.Value);
      }
      if (device.AssetID.HasValue)
      {
        return string.Format("AssetID={0}", device.AssetID.Value);
      }
      return string.Empty;
    }
  }
}
