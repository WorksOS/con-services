using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.DTOs;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces.DTOs
{
  // This interface specifies metadata about a type.
  // This type may or may not be a target type to be constructed.

  public interface IFactoryOutboundEventTypeDescriptor
  {
    string AssemblyQualifiedName { get; set; }
    EndpointDescriptor[] Destinations { get; set; }
  }
}