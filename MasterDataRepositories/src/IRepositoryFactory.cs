namespace VSS.Productivity3D.Repo
{
    public interface IRepositoryFactory
    {
        IRepository<T> GetRepository<T>();
    }
}