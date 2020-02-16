//using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;
namespace VSS.Nighthawk.DeviceCapabilityService.Helpers
{
  public class DeviceHandlerExceptionHelper
  {
    public static string NotCurrentlyImplemented(DeviceTypeEnum deviceType, string eventType)
    {
      return string.Format("{0} {1} requests are not currently implemented", deviceType, eventType);
    }

    public static string NotSupported(DeviceTypeEnum deviceType, string eventType)
    {
      return string.Format("{0} {1} requests are not supported", deviceType, eventType);
    }

    public static string Unknown(string eventType)
    {
      return string.Format("{0} requested for unknown device type", eventType);
    }
  }
}
