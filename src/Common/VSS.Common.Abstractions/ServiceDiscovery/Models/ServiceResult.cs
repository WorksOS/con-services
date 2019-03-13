namespace VSS.Common.Abstractions.ServiceDiscovery.Models
{
  public class ServiceResult
  {
    /// <summary>
    /// End point returned for the service, null if unknown
    /// </summary>
    public string Endpoint { get; set; }

    /// <summary>
    /// Service Resolver Type for the endpoint returned
    /// Or Unknown if no endpoint found
    /// </summary>
    public ServiceResultType Type { get; set; }
  }
}