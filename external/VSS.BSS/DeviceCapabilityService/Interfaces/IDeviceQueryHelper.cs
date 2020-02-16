//using VSS.Hosted.VLCommon;
using  VSS.Nighthawk.ExternalDataTypes.Enumerations;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces
{
  public interface IDeviceQueryHelper
  {
    DeviceTypeEnum? GetDeviceType(IDeviceQuery device, IStorage storage);
    string GetPrintableValues(IDeviceQuery device);
  }
}
