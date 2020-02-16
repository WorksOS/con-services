using System.Collections.Generic;
//using VSS.Hosted.VLCommon;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces
{
  public interface IDeviceHandlerParameters
  {
    IDictionary<DeviceTypeEnum, IDeviceHandlerStrategy> DeviceHandlers { get; }
    IDeviceHandlerStrategy UnknownDeviceHandler { get; }
  }
}
