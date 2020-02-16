using System.Collections.Generic;
using VSS.Nighthawk.ExternalDataTypes.Enumerations;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.DTOs;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces
{
  public interface IStorage
  {
    DeviceTypeEnum? GetDeviceTypeForDevice(long deviceId);
    DeviceTypeEnum? GetDeviceTypeForAsset(long assetId);
    IEnumerable<EndpointDescriptor> GetEndpointDescriptorsForNames(IEnumerable<string> endpointNames);
  }
}
