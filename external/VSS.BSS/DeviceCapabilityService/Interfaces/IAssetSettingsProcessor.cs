using VSS.Nighthawk.DeviceCapabilityService.Interfaces.DTOs;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces
{
  public interface IAssetSettingsProcessor
  {
    IFactoryOutboundEventTypeDescriptor GetAssetIdConfigurationChangedEvent(IDeviceQuery device);
  }
}
