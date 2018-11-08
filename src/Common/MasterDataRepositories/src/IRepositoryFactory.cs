namespace VSS.MasterData.Repositories
{
    public interface IRepositoryFactory
    {
        IRepository<T> GetRepository<T>();
    }
}