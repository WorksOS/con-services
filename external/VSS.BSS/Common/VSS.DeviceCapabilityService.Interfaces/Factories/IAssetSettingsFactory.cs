using VSS.BaseEvents;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Query;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces.Factories
{
  public interface IAssetSettingsFactory
  {
    IAssetIdConfigurationChangedEvent BuildAssetIdConfigurationChangedEventForDevice(IDeviceQuery deviceQuery);
  }
}
