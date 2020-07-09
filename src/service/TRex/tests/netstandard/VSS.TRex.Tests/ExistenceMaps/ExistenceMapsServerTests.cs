using System;
using System.Linq;
using FluentAssertions;
using VSS.TRex.ExistenceMaps.GridFabric.Requests;
using VSS.TRex.ExistenceMaps.Servers;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.SubGridTrees;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.ExistenceMaps
{
  public class ExistenceMapsServerTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    [Fact]
    public void Creation()
    {
      var server = new ExistenceMapServer();
      server.Should().NotBeNull();
    }

    [Fact]
    public void GetExistenceMap_NullKey()
    {
      var server = new ExistenceMapServer();
      Action act = () => server.GetExistenceMap(null);

      act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetExistenceMap_NonExistingKey()
    {
      var server = new ExistenceMapServer();
      server.GetExistenceMap(new NonSpatialAffinityKey(Guid.Empty, "")).Should().BeNull();
    }

    [Fact]
    public void GetExistenceMap()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var DesignUid = Guid.NewGuid();
      var filename = BaseExistenceMapRequest.CacheKeyString(TRex.ExistenceMaps.Interfaces.Consts.EXISTENCE_MAP_DESIGN_DESCRIPTOR, DesignUid);
      var setMap = new SubGridTreeSubGridExistenceBitMask();

      siteModel.PrimaryStorageProxy.WriteStreamToPersistentStore
        (siteModel.ID,
        filename,
        FileSystemStreamType.DesignTopologyExistenceMap,
        setMap.ToStream(), null)
      .Should().Be(FileSystemErrorStatus.OK);

      var server = new ExistenceMapServer();
      var getMap = server.GetExistenceMap(new NonSpatialAffinityKey(siteModel.ID, filename));

      setMap.ToBytes().SequenceEqual(getMap.ToBytes()).Should().BeTrue();
    }
  }
}
