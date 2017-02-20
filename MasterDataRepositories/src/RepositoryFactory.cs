using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Masterdata;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace MasterDataConsumer
{
  public class RepositoryFactory : IRepositoryFactory
  {
    private static readonly Dictionary<Type, object> container = new Dictionary<Type, object>();

      public RepositoryFactory(IRepository<ICustomerEvent> custRepository, IRepository<IProjectEvent> projRepository,
          IRepository<ISubscriptionEvent> subsRepository, IRepository<IGeofenceEvent> geoRepository)
      {
          if (container.Any()) return;
          container.Add(typeof(IRepository<ICustomerEvent>), custRepository);
          container.Add(typeof(IRepository<IProjectEvent>), projRepository);
          container.Add(typeof(IRepository<ISubscriptionEvent>), subsRepository);
          container.Add(typeof(IRepository<IGeofenceEvent>), geoRepository);
      }

      public IRepository<T> GetRepository<T>()
      {
          object result;
          if (container.TryGetValue(typeof(T), out result))
              return result as IRepository<T>;
          else
              return null;
      }
  }
}