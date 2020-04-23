namespace VSS.Common.Abstractions.ServiceDiscovery.Constants
{
  public static class ServiceNameConstants
  {
    public const string FILTER_SERVICE = "filter-service";
    public const string PRODUCTIVITY3D_SERVICE = "productivity3d-service";
    public const string PROJECT_SERVICE = "project-service";
    public const string SCHEDULER_SERVICE = "scheduler-service";
    public const string ASSETMGMT3D_SERVICE = "assetmgmt3d-service";
    public const string PUSH_SERVICE = "push-service";
    public const string TILE_SERVICE = "tile-service";
    public const string TAGFIELAUTH_SERVICE = "tagfileauth-service";
    public const string DEVICE_SERVICE = "project-service"; // currently this is in the project service, once it gets its own service it will need to change
    public const string TREX_SERVICE_IMMUTABLE = "trex-service-immutable";
    public const string TREX_SERVICE_MUTABLE = "trex-service-mutable";
    public const string TREX_SERVICE_CONNECTEDSITE = "trex-service-connectedsite";

    public const string REDIS_CACHE = "redis-cache-service";
    public const string INFLUX_DB = "influx-db-service";
  }
}
