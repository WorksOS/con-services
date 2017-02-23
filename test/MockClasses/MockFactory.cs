using System;
using Microsoft.Extensions.Logging;
using VSS.Asset.Data;
using VSS.GenericConfiguration;
using VSS.Masterdata;
using VSS.TagFileAuth.Service.MockClasses;

namespace MockClasses
{
  public class MockFactory : IRepositoryFactory
  {
    //NOTE: Currently repos are both read and write. We can add readonly ones later with a readonly connection string if required.

    //private static readonly ILogger log = DependencyInjectionProvider.ServiceProvider.GetService<ILoggerFactory>().CreateLogger<MockFactory>();

    private readonly MockAssetRepository assetRepo;

    public MockFactory(IConfigurationStore _connectionString, ILoggerFactory logger) 
    {
      //log.LogDebug("Repo: Building repository factory");
      assetRepo = new MockAssetRepository(_connectionString, logger);      
    }

    //public AssetRepository GetAssetRepository()
    //{
    //  return assetRepo;
    //}

    public IRepository<T> GetRepository<T>()
    {
      throw new NotImplementedException();
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
