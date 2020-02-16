using VSS.BaseEvents;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces.Factories
{
  // This interface specifies a factory for building location status update request events
  public interface ILocationStatusUpdateRequestedEventFactory
  {
    ILocationStatusUpdateRequestedEvent BuildLocationStatusUpdateRequestedEventForDevice(IDeviceQuery deviceQuery);
  }
}
