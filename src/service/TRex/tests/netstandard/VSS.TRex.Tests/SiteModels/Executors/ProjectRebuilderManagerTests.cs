using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSS.AWS.TransferProxy;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels.Executors;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Executors;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModels.Executors
{
  public class SiteModelRebuilderManagerTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>, IDisposable
  {
    public SiteModelRebuilderManagerTests(DITAGFileAndSubGridRequestsWithIgniteFixture fixture)
    {
      DIContext.Obtain<ISiteModelRebuilderManager>().AbortAll();

      fixture.ClearDynamicFixtureContent();
      fixture.ResetDynamicMockedIgniteContent();

      // Modify the SiteModels instance to be mutable, rather than immutable to mimic the mutable context 
      // project deletion operates in 
      DIBuilder
        .Continue()
        .RemoveSingle<ISiteModels>()
        .Add(x => x.AddSingleton<ISiteModels>(new TRex.SiteModels.SiteModels(StorageMutability.Mutable)))
        .Complete();
    }

    [Fact]
    public void Creation()
    {
      var rebuilder = new SiteModelRebuilderManager();
      rebuilder.Should().NotBeNull();
    }

    [Fact]
    public void RebuilderCount_None()
    {
      var rebuilder = new SiteModelRebuilderManager();
      rebuilder.RebuildCount().Should().Be(0);
    }

    [Fact]
    public void GetRebuilderState_None()
    {
      var manager = new SiteModelRebuilderManager();

      var state = manager.GetRebuildersState();
      state.Should().NotBeNull();
      state.Count.Should().Be(0);
    }

    [Fact]
    public void Abort_WithRebuilder()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var manager = new SiteModelRebuilderManager();
      var rebuilder = new SiteModelRebuilder(siteModel.ID, false, TransferProxyType.TAGFiles);

      manager.AddRebuilder(rebuilder);
      manager.GetRebuildersState().Count.Should().Be(1);

      manager.Abort(siteModel.ID);
      manager.GetRebuildersState().Count.Should().Be(0);
    }

    [Fact]
    public void Abort_WithoutRebuilder()
    {
      var manager = new SiteModelRebuilderManager();

      manager.GetRebuildersState().Count.Should().Be(0);

      manager.Abort(Guid.NewGuid());
      manager.GetRebuildersState().Count.Should().Be(0);
    }

    [Fact]
    public void BeginOperations_DontRestartCompletedProjects()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      siteModel.SaveMetadataToPersistentStore(siteModel.PrimaryStorageProxy, true);

      // Add metadata for an existing Complete project and ensure no rebuilder is constructed for it
      var metadataCache = DIContext.Obtain<Func<RebuildSiteModelCacheType, IStorageProxyCacheCommit>>()(RebuildSiteModelCacheType.Metadata)
        as IStorageProxyCache<INonSpatialAffinityKey, IRebuildSiteModelMetaData>;

      metadataCache?.Put(new NonSpatialAffinityKey(siteModel.ID, SiteModelRebuilder.MetadataKeyName), 
        new RebuildSiteModelMetaData
        {
          ProjectUID = siteModel.ID,
          Phase = RebuildSiteModelPhase.Complete
        });

      var manager = new SiteModelRebuilderManager();
      manager.BeginOperations();

      manager.GetRebuildersState().Count.Should().Be(0);
    }

    [Theory]
    [InlineData(RebuildSiteModelPhase.Completion)]
    [InlineData(RebuildSiteModelPhase.Deleting)]
    [InlineData(RebuildSiteModelPhase.Monitoring)]
    [InlineData(RebuildSiteModelPhase.Scanning)]
    [InlineData(RebuildSiteModelPhase.Submitting)]
    [InlineData(RebuildSiteModelPhase.Unknown)]
    public async void BeginOperations_RestartInProgressProjects(RebuildSiteModelPhase phase)
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      siteModel.SaveMetadataToPersistentStore(siteModel.PrimaryStorageProxy, true);

      // Add metadata for an existing Complete project and ensure no rebuilder is constructed for it
      var metadataCache = DIContext.Obtain<Func<RebuildSiteModelCacheType, IStorageProxyCacheCommit>>()(RebuildSiteModelCacheType.Metadata)
          as IStorageProxyCache<INonSpatialAffinityKey, IRebuildSiteModelMetaData>;

      metadataCache?.Put(new NonSpatialAffinityKey(siteModel.ID, SiteModelRebuilder.MetadataKeyName),
        new RebuildSiteModelMetaData
        {
          ProjectUID = siteModel.ID,
          Phase = phase
        });

      var manager = new SiteModelRebuilderManager();
      await manager.BeginOperations();

      manager.GetRebuildersState().Count.Should().Be(1);
    }

    public void Dispose()
    {
      DIContext.Obtain<ISiteModelRebuilderManager>().AbortAll();
    }
  }
}
