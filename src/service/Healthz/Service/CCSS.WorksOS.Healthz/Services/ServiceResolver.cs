using System.Collections.Generic;
using VSS.Common.Abstractions.ServiceDiscovery.Constants;

namespace CCSS.WorksOS.Healthz.Services
{
  public static class ServiceResolver
  {
    /// <summary>
    /// Return a list of service identifiers from the <see cref="ServiceNameConstants"/> pool.
    /// </summary>
    public static List<string> GetKnownServiceIdentifiers() =>
      // Could use reflection to iterate service constants and remove those from a provided whitelist.
      new List<string>
      {
        ServiceNameConstants.FILTER_SERVICE,
        ServiceNameConstants.PRODUCTIVITY3D_SERVICE,
        ServiceNameConstants.PROJECT_SERVICE,
        ServiceNameConstants.SCHEDULER_SERVICE,
        //ServiceNameConstants.ASSETMGMT3D_SERVICE,
        ServiceNameConstants.PUSH_SERVICE,
        ServiceNameConstants.TILE_SERVICE,
        ServiceNameConstants.TAGFIELAUTH_SERVICE,
        ServiceNameConstants.DEVICE_SERVICE,
        ServiceNameConstants.ENTITLEMENTS_SERVICE,
        ServiceNameConstants.TREX_SERVICE_IMMUTABLE,
        ServiceNameConstants.TREX_SERVICE_MUTABLE,
        ServiceNameConstants.PREFERENCES_SERVICE
      };
  }
}
