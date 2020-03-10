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
      TRexCaches.SpatialCacheName(StorageMutability.Mutable, FileSystemStreamType.SubGridDirectory).Should().Be(TRexCaches.SpatialSubGridDirectoryCacheName(StorageMutability.Mutable));
      TRexCaches.SpatialCacheName(StorageMutability.Mutable, FileSystemStreamType.SubGridSegment).Should().Be(TRexCaches.SpatialSubGridSegmentCacheName(StorageMutability.Mutable));
      TRexCaches.SpatialCacheName(StorageMutability.Immutable, FileSystemStreamType.SubGridDirectory).Should().Be(TRexCaches.SpatialSubGridDirectoryCacheName(StorageMutability.Immutable));
      TRexCaches.SpatialCacheName(StorageMutability.Immutable, FileSystemStreamType.SubGridSegment).Should().Be(TRexCaches.SpatialSubGridSegmentCacheName(StorageMutability.Immutable));
      TRexCaches.SpatialCacheName(StorageMutability.Mutable, FileSystemStreamType.SubGridExistenceMap).Should().Be(TRexCaches.ProductionDataExistenceMapCacheName(StorageMutability.Mutable));
      TRexCaches.SpatialCacheName(StorageMutability.Immutable, FileSystemStreamType.SubGridExistenceMap).Should().Be(TRexCaches.ProductionDataExistenceMapCacheName(StorageMutability.Immutable));
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
      TRexCaches.SpatialSubGridDirectoryCacheName(StorageMutability.Immutable).Should().NotBeNullOrWhiteSpace();
      TRexCaches.SpatialSubGridDirectoryCacheName(StorageMutability.Mutable).Should().NotBeNullOrWhiteSpace();
      TRexCaches.SpatialSubGridSegmentCacheName(StorageMutability.Immutable).Should().NotBeNullOrWhiteSpace();
      TRexCaches.SpatialSubGridSegmentCacheName(StorageMutability.Mutable).Should().NotBeNullOrWhiteSpace();
      TRexCaches.MutableNonSpatialCacheName().Should().NotBeNullOrWhiteSpace();
      TRexCaches.SegmentRetirementQueueCacheName().Should().NotBeNullOrWhiteSpace();
      TRexCaches.SiteModelMetadataCacheName().Should().NotBeNullOrWhiteSpace();
      TRexCaches.SiteModelsCacheName(StorageMutability.Immutable).Should().NotBeNullOrWhiteSpace();
      TRexCaches.SiteModelsCacheName(StorageMutability.Mutable).Should().NotBeNullOrWhiteSpace();
      TRexCaches.TAGFileBufferQueueCacheName().Should().NotBeNullOrWhiteSpace();
      TRexCaches.SiteModelChangeMapsCacheName().Should().NotBeNullOrWhiteSpace();
      TRexCaches.ProductionDataExistenceMapCacheName(StorageMutability.Immutable).Should().NotBeNullOrWhiteSpace();
      TRexCaches.ProductionDataExistenceMapCacheName(StorageMutability.Mutable).Should().NotBeNullOrWhiteSpace();
    }
  }
}
