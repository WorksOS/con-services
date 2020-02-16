namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces.DTOs
{
  public interface IEndpointDescriptor
  {
    string Name { get; set; }
    long Id { get; set; }
    string Url { get; set; }
    string ContentType { get; set; }
    string Username { get; set; }
    string EncryptedPwd { get; set; }
  }
}
