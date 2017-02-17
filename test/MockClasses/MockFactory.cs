using VSS.TagFileAuth.Service.MockClasses;
using VSS.TagFileAuth.Service.Repositories;
using VSS.TagFileAuth.Service.Repositories.Interfaces;

namespace MockClasses
{
  public class MockFactory : IRepositoryFactory
  {
    //NOTE: Currently repos are both read and write. We can add readonly ones later with a readonly connection string if required.

    //private static readonly ILogger log = DependencyInjectionProvider.ServiceProvider.GetService<ILoggerFactory>().CreateLogger<MockFactory>();

    private readonly MockAssetRepository assetRepo;

    public MockFactory()
    {
      //log.LogDebug("Repo: Building repository factory");
      assetRepo = new MockAssetRepository();      
    }

    public IAssetRepository GetAssetRepository()
    {
      return assetRepo;
    }


    #region Master Data Repos

    private MockAssetRepository _AssetRepository
    {
      get { return assetRepo; }
    }


    #region Read Getters

    //public IReadAssetRepository GetReadAssetRepository()
    //{
    //  return _AssetRepository;
    //}

    #endregion


    #endregion
  }
}
