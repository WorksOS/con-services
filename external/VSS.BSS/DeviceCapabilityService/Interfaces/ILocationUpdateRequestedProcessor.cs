using VSS.Nighthawk.DeviceCapabilityService.Interfaces.DTOs;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces
{
  public interface ILocationUpdateRequestedProcessor
  {
    IFactoryOutboundEventTypeDescriptor GetLocationUpdateRequestedEvent(IDeviceQuery device);
  }
}
