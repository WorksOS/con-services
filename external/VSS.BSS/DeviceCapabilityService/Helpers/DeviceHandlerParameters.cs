using System.Collections.Generic;
//using VSS.Hosted.VLCommon;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;

namespace VSS.Nighthawk.DeviceCapabilityService.Helpers
{
  public class DeviceHandlerParameters : IDeviceHandlerParameters
  {
    public DeviceHandlerParameters(IDictionary<DeviceTypeEnum, IDeviceHandlerStrategy> deviceHandlers, IDeviceHandlerStrategy unknownDeviceHandler)
    {
      DeviceHandlers = deviceHandlers;
      UnknownDeviceHandler = unknownDeviceHandler;
    }

    public IDictionary<DeviceTypeEnum, IDeviceHandlerStrategy> DeviceHandlers { get; private set; }

    public IDeviceHandlerStrategy UnknownDeviceHandler { get; private set; }
  }
}
