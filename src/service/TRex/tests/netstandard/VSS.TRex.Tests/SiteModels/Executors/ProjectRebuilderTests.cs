using System;
using FluentAssertions;
using VSS.AWS.TransferProxy;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.GridFabric.ComputeFuncs;
using VSS.TRex.SiteModels.GridFabric.Requests;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Executors;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModels.GridFabric.Executors
{
  public class SiteModelRebuilderTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private void AddApplicationGridRouting()
    {
      IgniteMock.Mutable.AddApplicationGridRouting<DeleteSiteModelRequestComputeFunc, DeleteSiteModelRequestArgument, DeleteSiteModelRequestResponse>();
    }

    [Fact]
    public void Creation()
    {
      var rebuilder = new SiteModelRebuilder(Guid.NewGuid(), false, TransferProxyType.TAGFiles);
      rebuilder.Should().NotBeNull();
    }

    [Fact]
    public void ValidateNoActiveRebuilderForProject()
    {
      // Create sitemodel, add project metadata for it, check validator hates it...
  //    var sitemodel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

 //     var metaData = new RebuildSiteModelMetaData()
 //     {
 //       ProjectUID = sitemodel.ID
 //     };

      var rebuilder = new SiteModelRebuilder(Guid.NewGuid(), false, TransferProxyType.TAGFiles);

      rebuilder.MetadataCache = DIContext.Obtain<Func<RebuildSiteModelCacheType, IStorageProxyCacheCommit>>()(RebuildSiteModelCacheType.Metadata)
        as IStorageProxyCache<INonSpatialAffinityKey, IRebuildSiteModelMetaData>;

      Assert.True(rebuilder.ValidateNoAciveRebuilderForProject(Guid.NewGuid()));
    }

    [Fact]
    public async void ExecuteAsync()
    {
      AddApplicationGridRouting();

      // Create sitemodel, add project metadata for it, check validator hates it...
      var sitemodel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      var rebuilder = new SiteModelRebuilder(sitemodel.ID, false, TransferProxyType.TAGFiles)
      {
        MetadataCache = DIContext.Obtain<Func<RebuildSiteModelCacheType, IStorageProxyCacheCommit>>()(RebuildSiteModelCacheType.Metadata)
                        as IStorageProxyCache<INonSpatialAffinityKey, IRebuildSiteModelMetaData>,
        FilesCache = DIContext.Obtain<Func<RebuildSiteModelCacheType, IStorageProxyCacheCommit>>()(RebuildSiteModelCacheType.KeyCollections)
                     as IStorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>
      };

      var result = await rebuilder.ExecuteAsync();

      result.Should().NotBeNull();
    }
  }
}
