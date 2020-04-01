namespace VSS.Common.Abstractions.ServiceDiscovery.Enums
{
  /// <summary>
  /// Do not rename these values, without updating any service definitions
  /// </summary>
  public enum ApiType
  {
    /// <summary>
    /// Internal URL Endpoints, such as scheduler background job
    /// In the format http://localhost/internal/v1/route
    /// </summary>
    Private,
    
    /// <summary>
    /// Urls that are exposed to the public (via TPaaS Or other)
    /// In the format http://localhost/api/v1/route
    /// </summary>
    Public
  }
}