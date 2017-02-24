using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using VSS.Masterdata;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace MasterDataConsumer
{
  public class RepositoryFactory : IRepositoryFactory
  {
    private readonly ILogger log;
    private static readonly Dictionary<Type, object> container = new Dictionary<Type, object>();

    public RepositoryFactory(IRepository<ICustomerEvent> custRepository, IRepository<IProjectEvent> projRepository,
        IRepository<ISubscriptionEvent> subsRepository, IRepository<IGeofenceEvent> geoRepository, ILoggerFactory logger)
    {
      log = logger.CreateLogger<RepositoryFactory>();
      if (container.Any()) return;

      log.LogTrace("Registering repositories");

      container.Add(typeof(IAssetEvent), custRepository);
      container.Add(typeof(ICustomerEvent), custRepository);
      container.Add(typeof(IDeviceEvent), custRepository);
      container.Add(typeof(IGeofenceEvent), geoRepository);
      container.Add(typeof(IProjectEvent), projRepository);
      container.Add(typeof(ISubscriptionEvent), subsRepository);


      log.LogTrace("Registered {0} repos", container.Count);
    }

    public IRepository<T> GetRepository<T>()
    {
      object result;
      log.LogTrace("Resolving repo of type {0} out of {1} available", typeof(T).ToString(), container.Count);
      if (container.TryGetValue(typeof(T), out result))
      {
        log.LogTrace("Resolved to {0}", result.ToString());
        return result as IRepository<T>;
      }
      log.LogWarning("Can not resolve repo of type {0}", typeof(T).ToString());
      return null;
    }
  }
}