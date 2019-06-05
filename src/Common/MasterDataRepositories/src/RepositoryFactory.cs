using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.SeriLog.Extensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.MasterData.Repositories
{
  public class RepositoryFactory : IRepositoryFactory
  {
    private static readonly Dictionary<Type, object> container = new Dictionary<Type, object>();
    private readonly ILogger log;

    public RepositoryFactory(IRepository<IAssetEvent> assetRepository, IRepository<ICustomerEvent> custRepository,
      IRepository<IDeviceEvent> deviceRepository,
      IRepository<IGeofenceEvent> geoRepository, IRepository<IProjectEvent> projRepository,
      IRepository<ISubscriptionEvent> subsRepository, IRepository<IFilterEvent> filterRepository,
      ILoggerFactory logger)
    {
      log = logger.CreateLogger<RepositoryFactory>();
      if (container.Any()) return;

      if (log.IsTraceEnabled())
        log.LogTrace("Registering repositories");

      container.Add(typeof(IAssetEvent), assetRepository);
      container.Add(typeof(ICustomerEvent), custRepository);
      container.Add(typeof(IDeviceEvent), deviceRepository);
      container.Add(typeof(IGeofenceEvent), geoRepository);
      container.Add(typeof(IProjectEvent), projRepository);
      container.Add(typeof(ISubscriptionEvent), subsRepository);
      container.Add(typeof(IFilterEvent), filterRepository);


      if (log.IsTraceEnabled())
        log.LogTrace("Registered {0} repos", container.Count);
    }

    public IRepository<T> GetRepository<T>()
    {
      object result;
      if (log.IsTraceEnabled())
        log.LogTrace("Resolving repo of type {0} out of {1} available", typeof(T).ToString(), container.Count);
      if (container.TryGetValue(typeof(T), out result))
      {
        if (log.IsTraceEnabled())
          log.LogTrace("Resolved to {0}", result.ToString());
        return result as IRepository<T>;
      }
      log.LogWarning("Can not resolve repo of type {0}", typeof(T).ToString());
      return null;
    }
  }
}
