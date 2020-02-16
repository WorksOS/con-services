using VSS.Nighthawk.DeviceCapabilityService.Interfaces.DTOs;

namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.DTOs
{
  public class EndpointDescriptor : IEndpointDescriptor
  {
    public string Name { get; set; }
    public long Id { get; set; }
    public string Url { get; set; }
    public string ContentType { get; set; }
    public string Username { get; set; }
    public string EncryptedPwd { get; set; }
  }
}
