using VSS.BaseEvents;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces.Factories
{
  // This interface specifies a factory for building site administration events

  public interface ISiteAdministrationEventFactory
  {
    ISiteDispatchedEvent BuildSiteDispatchedEventForDevice(IDeviceQuery deviceQuery);
    ISiteRemovedEvent BuildSiteRemovedEventForDevice(IDeviceQuery deviceQuery);
  }
}
