using VSS.Common.Abstractions.Exceptions;

namespace VSS.Common.Abstractions.ServiceDiscovery.Exceptions
{
  /// <summary>
  /// No service found in Service Discovery
  /// </summary>
  public class ServiceNotFoundException : ProductivityException
  {
    public ServiceNotFoundException(string serviceName) : base($"Could not find a service for '{serviceName}'")
    {
      
    }
  }
}
