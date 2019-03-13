using FluentAssertions;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Storage
{
  public class TRexCachesTests: IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void SpatialCacheName()
    {
      TRexCaches.SpatialCacheName(StorageMutability.Mutable).Should().Be(TRexCaches.MutableSpatialCacheName());
      TRexCaches.SpatialCacheName(StorageMutability.Immutable).Should().Be(TRexCaches.ImmutableSpatialCacheName());
    }

    [Fact]
    public void NonSpatialCacheName()
    {
      TRexCaches.NonSpatialCacheName(StorageMutability.Mutable).Should().Be(TRexCaches.MutableNonSpatialCacheName());
      TRexCaches.NonSpatialCacheName(StorageMutability.Immutable).Should().Be(TRexCaches.ImmutableNonSpatialCacheName());
    }

    [Fact]
    public void NonNullNames()
    {
      TRexCaches.DesignTopologyExistenceMapsCacheName().Should().NotBeNullOrWhiteSpace();
      TRexCaches.ImmutableNonSpatialCacheName().Should().NotBeNullOrWhiteSpace();
      TRexCaches.ImmutableSpatialCacheName().Should().NotBeNullOrWhiteSpace();
      TRexCaches.MutableNonSpatialCacheName().Should().NotBeNullOrWhiteSpace();
      TRexCaches.MutableSpatialCacheName().Should().NotBeNullOrWhiteSpace();
      TRexCaches.SegmentRetirementQueueCacheName().Should().NotBeNullOrWhiteSpace();
      TRexCaches.SiteModelMetadataCacheName().Should().NotBeNullOrWhiteSpace();
      TRexCaches.TAGFileBufferQueueCacheName().Should().NotBeNullOrWhiteSpace();
    }
  }
}
