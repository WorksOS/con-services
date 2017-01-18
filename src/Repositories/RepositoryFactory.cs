using KafkaConsumer;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace MasterDataConsumer
{
  public class RepositoryFactory : IRepositoryFactory
  {
    private readonly IRepository<ICustomerEvent> customerRepository;
    private readonly IRepository<IProjectEvent> projectRepository;
    private readonly IRepository<ISubscriptionEvent> subscriptionRepository;
    private readonly IRepository<IGeofenceEvent> geofenceRepository;

    public RepositoryFactory(IRepository<ICustomerEvent> custRepository, IRepository<IProjectEvent> projRepository, IRepository<ISubscriptionEvent> subsRepository, IRepository<IGeofenceEvent> geoRepository)
    {
      customerRepository = custRepository;
      projectRepository = projRepository;
      subscriptionRepository = subsRepository;    
      geofenceRepository = geoRepository;
    }

    public IRepository<T> GetRepository<T>()
    {
      if (typeof(T) == typeof(ICustomerEvent))
        return customerRepository as IRepository<T>;
      if (typeof(T) == typeof(IProjectEvent))
        return projectRepository as IRepository<T>;
      if (typeof(T) == typeof(ISubscriptionEvent))
        return subscriptionRepository as IRepository<T>;
      if (typeof(T) == typeof(IGeofenceEvent))
        return geofenceRepository as IRepository<T>;
      return null;
    }
  }
}