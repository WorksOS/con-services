using VSS.Nighthawk.DeviceCapabilityService.Interfaces.DTOs;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces
{
  public interface ISiteAdministrationProcessor
  {
    IFactoryOutboundEventTypeDescriptor GetSiteDispatchedEvent(IDeviceQuery device);
    IFactoryOutboundEventTypeDescriptor GetSiteRemovedEvent(IDeviceQuery device);
  }
}
