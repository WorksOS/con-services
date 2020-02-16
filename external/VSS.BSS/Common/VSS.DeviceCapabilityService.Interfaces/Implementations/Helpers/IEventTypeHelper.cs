using VSS.BaseEvents;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Helpers
{
  public interface IEventTypeHelper
  {
    T QueryServiceForTypeAndBuildInstance<T>(string serverAction, string deviceCapabilitySvcUri, IDeviceQuery deviceQuery)
      where T : IEndpointDestinedEvent;
  }
}
