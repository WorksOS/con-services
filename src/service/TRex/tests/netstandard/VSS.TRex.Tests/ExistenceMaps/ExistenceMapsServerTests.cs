using System;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.ExistenceMaps.Servers;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.ExistenceMaps
{
  public class ExistenceMapsServerTests_MissingIgnite
  {
    [Fact]
    public void Creation()
    {
      Action act = () =>
      {
        var _ = new ExistenceMapServer();
      };

      act.Should().Throw<TRexException>($"Failed to get or create Ignite cache {TRexCaches.DesignTopologyExistenceMapsCacheName()}, ignite reference is null");
    }
  }

  public class ExistenceMapsServerTests_WithMockedIgnite : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  { 
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
    public void SetExistenceMap()
    {
      var server = new ExistenceMapServer();
      Guid projectUid = Guid.NewGuid();
      server.SetExistenceMap(new NonSpatialAffinityKey(projectUid, "UnitTestExistenceMap_Set"), new SerialisedByteArrayWrapper(new byte[1000]));
    }

    [Fact]
    public void GetExistenceMap()
    {
      var server = new ExistenceMapServer();
      Guid projectUid = Guid.NewGuid();
      var setMap = Enumerable.Range(0, 1000).Select(x => (byte) x).ToArray();

      server.SetExistenceMap(new NonSpatialAffinityKey(projectUid, "UnitTestExistenceMap_Get"), new SerialisedByteArrayWrapper(setMap));

      var getMap = server.GetExistenceMap(new NonSpatialAffinityKey(projectUid, "UnitTestExistenceMap_Get"));

      setMap.SequenceEqual(getMap.Bytes).Should().BeTrue();
    }
  }
}
