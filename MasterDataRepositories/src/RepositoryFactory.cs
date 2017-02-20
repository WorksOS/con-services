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

          container.Add(typeof(IRepository<ICustomerEvent>), custRepository);
          container.Add(typeof(IRepository<IProjectEvent>), projRepository);
          container.Add(typeof(IRepository<ISubscriptionEvent>), subsRepository);
          container.Add(typeof(IRepository<IGeofenceEvent>), geoRepository);

          log.LogTrace("Registered {0} repos", container.Count);
        }

      public IRepository<T> GetRepository<T>()
      {
          object result;
          log.LogTrace("Resolving repo of type {0}", typeof(T).ToString());
          if (container.TryGetValue(typeof(T), out result))
          {
                log.LogDebug("Resolved to {0}", result.ToString());
                return result as IRepository<T>;
          }
            log.LogWarning("Can not resolve repo of type {0}", typeof(T).ToString());
            return null;
      }
  }
}