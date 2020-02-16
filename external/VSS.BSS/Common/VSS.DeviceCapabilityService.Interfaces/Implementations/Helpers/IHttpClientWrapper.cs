namespace VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Helpers
{
  public interface IHttpClientWrapper
  {
    IHttpResponseWrapper Get(string uri, object query = null);
  }
}
