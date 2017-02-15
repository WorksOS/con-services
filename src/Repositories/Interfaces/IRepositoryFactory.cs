namespace VSS.TagFileAuth.Service.Interfaces
{
    public interface IRepositoryFactory
    {
        IRepository<T> GetRepository<T>();
    }
}