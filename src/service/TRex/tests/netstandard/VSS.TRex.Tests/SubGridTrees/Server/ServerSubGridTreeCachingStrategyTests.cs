using System;
using System.IO;
using FluentAssertions;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Utilities;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Server
{
  public class ServerSubGridTreeCachingStrategyTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void DefaultMutableStrategy()
    {
      var tree = new ServerSubGridTree(Guid.NewGuid(), StorageMutability.Mutable);

      tree.CachingStrategy.Should().Be(ServerSubGridTreeCachingStrategy.CacheSubGridsInTree);
    }

    [Fact]
    public void DefaultImmutableStrategy()
    {
      var tree = new ServerSubGridTree(Guid.NewGuid(), StorageMutability.Immutable);

      tree.CachingStrategy.Should().Be(ServerSubGridTreeCachingStrategy.CacheSubGridsInIgniteGridCache);
    }

    [Fact]
    public void NoLocalCachingAfterRequest()
    {
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel
        (new [] { Path.Combine(TestHelper.CommonTestDataPath, "TestTagFile.tag")}, out _);
      siteModel.SetStorageRepresentationToSupply(StorageMutability.Immutable);
      siteModel.SetStorageRepresentationToSupply(StorageMutability.Mutable);

      siteModel.Grid.CountLeafSubGridsInMemory().Should().Be(0);
      siteModel.Grid.CachingStrategy.Should().Be(ServerSubGridTreeCachingStrategy.CacheSubGridsInTree);

      // Request all sub grids and ensure they are present in cache
      siteModel.ExistenceMap.ScanAllSetBitsAsSubGridAddresses(address =>
      {
        var subGrid = SubGridUtilities.LocateSubGridContaining
          (siteModel.PrimaryStorageProxy, siteModel.Grid, address.X, address.Y, siteModel.Grid.NumLevels, false, false);

        subGrid.Should().NotBeNull();
      });

      siteModel.Grid.CountLeafSubGridsInMemory().Should().BeGreaterThan(0);

      DIContext.Obtain<ISiteModels>().DropSiteModel(siteModel.ID);

      siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModel.ID);
      siteModel.SetStorageRepresentationToSupply(StorageMutability.Immutable);

      siteModel.Grid.CountLeafSubGridsInMemory().Should().Be(0);

      // Request all sub grids and ensure they are NOT present in cache
      siteModel.ExistenceMap.ScanAllSetBitsAsSubGridAddresses(address =>
      {
        var subGrid = SubGridUtilities.LocateSubGridContaining
          (siteModel.PrimaryStorageProxy, siteModel.Grid, address.X, address.Y, siteModel.Grid.NumLevels, false, false);

        subGrid.Should().NotBeNull();
        subGrid.Parent.Should().NotBeNull();
        subGrid.Owner.Should().Be(siteModel.Grid);
      });

      siteModel.Grid.CountLeafSubGridsInMemory().Should().Be(0);
    }
  }
}
