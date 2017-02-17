using VSS.TagFileAuth.Service.Repositories.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.TagFileAuth.Service.Repositories
{
  public class RepositoryFactory : IRepositoryFactory
  {
    private readonly AssetRepository assetRepository;    

    public RepositoryFactory(AssetRepository assetRepository)
    {
      this.assetRepository = assetRepository;
    }

    public IAssetRepository GetAssetRepository()
    {
      return assetRepository;
    }

    //public IRepository<T> GetRepository<T>()
    //{
    //  if (typeof(T) == typeof(IAssetEvent))
    //    return assetRepository as IRepository<T>;
    //  return null;
    //}
  }
}