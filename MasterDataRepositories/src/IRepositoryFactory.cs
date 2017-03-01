namespace Repositories
{
    public interface IRepositoryFactory
    {
        IRepository<T> GetRepository<T>();
    }
}