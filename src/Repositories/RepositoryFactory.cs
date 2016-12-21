using KafkaConsumer;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace MasterDataConsumer
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly IRepository<ISubscriptionEvent> subscriptionReporitory;

        public RepositoryFactory(IRepository<ISubscriptionEvent> subsRepository)
        {
            subscriptionReporitory = subsRepository;
        }

        public IRepository<T> GetRepository<T>()
        {
            if (typeof(T) == typeof(ISubscriptionEvent))
                return subscriptionReporitory as IRepository<T>;
            return null;
        }
    }
}