namespace VSS.Masterdata
{
    public interface IRepositoryFactory
    {
        IRepository<T> GetRepository<T>();
    }
}