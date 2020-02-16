using VSS.Nighthawk.DeviceCapabilityService.Interfaces.DTOs;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.DTOs
{
  public class FactoryOutboundEventTypeDescriptor : IFactoryOutboundEventTypeDescriptor
  {
    public string AssemblyQualifiedName { get; set; }
    public EndpointDescriptor[] Destinations { get; set; }
  }
}