namespace KafkaConsumer
{
    public interface IRepositoryFactory
    {
        IRepository<T> GetRepository<T>();
    }
}