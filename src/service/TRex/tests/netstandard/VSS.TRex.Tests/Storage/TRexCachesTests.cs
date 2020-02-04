using FluentAssertions;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Models;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Storage
{
  public class TRexCachesTests: IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void SpatialCacheName()
    {
      TRexCaches.SpatialCacheName(StorageMutability.Mutable, FileSystemStreamType.SubGridDirectory).Should().Be(TRexCaches.MutableSpatialCacheName());
      TRexCaches.SpatialCacheName(StorageMutability.Immutable, FileSystemStreamType.SubGridDirectory).Should().Be(TRexCaches.ImmutableSpatialCacheName());
      TRexCaches.SpatialCacheName(StorageMutability.Immutable, FileSystemStreamType.ProductionDataXML).Should().Be(TRexCaches.SiteModelsCacheName(StorageMutability.Immutable));
      TRexCaches.SpatialCacheName(StorageMutability.Mutable, FileSystemStreamType.ProductionDataXML).Should().Be(TRexCaches.SiteModelsCacheName(StorageMutability.Mutable));
    }

    [Fact]
    public void NonSpatialCacheName()
    {
      TRexCaches.NonSpatialCacheName(StorageMutability.Mutable, FileSystemStreamType.Events).Should().Be(TRexCaches.MutableNonSpatialCacheName());
      TRexCaches.NonSpatialCacheName(StorageMutability.Immutable, FileSystemStreamType.Events).Should().Be(TRexCaches.ImmutableNonSpatialCacheName());
      TRexCaches.NonSpatialCacheName(StorageMutability.Immutable, FileSystemStreamType.SiteModelMachineElevationChangeMap).Should().Be(TRexCaches.SiteModelChangeMapsCacheName());
    }

    [Fact]
    public void SiteModelsCacheName()
    {
      TRexCaches.NonSpatialCacheName(StorageMutability.Mutable, FileSystemStreamType.ProductionDataXML).Should().Be(TRexCaches.SiteModelsCacheName(StorageMutability.Mutable));
      TRexCaches.NonSpatialCacheName(StorageMutability.Immutable, FileSystemStreamType.ProductionDataXML).Should().Be(TRexCaches.SiteModelsCacheName(StorageMutability.Immutable));
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
      TRexCaches.SiteModelsCacheName(StorageMutability.Immutable).Should().NotBeNullOrWhiteSpace();
      TRexCaches.SiteModelsCacheName(StorageMutability.Mutable).Should().NotBeNullOrWhiteSpace();
      TRexCaches.TAGFileBufferQueueCacheName().Should().NotBeNullOrWhiteSpace();
      TRexCaches.SiteModelChangeBufferQueueCacheName().Should().NotBeNullOrWhiteSpace();
    }
  }
}
