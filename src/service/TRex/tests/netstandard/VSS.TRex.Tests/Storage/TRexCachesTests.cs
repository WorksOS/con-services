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
      TRexCaches.SpatialCacheName(StorageMutability.Mutable, FileSystemStreamType.SubGridDirectory).Should().Be(TRexCaches.kSpatialSubGridDirectoryMutable);
      TRexCaches.SpatialCacheName(StorageMutability.Mutable, FileSystemStreamType.SubGridSegment).Should().Be(TRexCaches.kSpatialSubGridSegmentMutable);
      TRexCaches.SpatialCacheName(StorageMutability.Immutable, FileSystemStreamType.SubGridDirectory).Should().Be(TRexCaches.kSpatialSubGridDirectoryImmutable);
      TRexCaches.SpatialCacheName(StorageMutability.Immutable, FileSystemStreamType.SubGridSegment).Should().Be(TRexCaches.kSpatialSubGridSegmentImmutable);
      TRexCaches.SpatialCacheName(StorageMutability.Mutable, FileSystemStreamType.SubGridExistenceMap).Should().Be(TRexCaches.kProductionDataExistenceMapCacheMutable);
      TRexCaches.SpatialCacheName(StorageMutability.Immutable, FileSystemStreamType.SubGridExistenceMap).Should().Be(TRexCaches.kProductionDataExistenceMapCacheImmutable);
    }

    [Fact]
    public void NonSpatialCacheName()
    {
      TRexCaches.NonSpatialCacheName(StorageMutability.Mutable, FileSystemStreamType.Events).Should().Be(TRexCaches.kNonSpatialMutable);
      TRexCaches.NonSpatialCacheName(StorageMutability.Immutable, FileSystemStreamType.Events).Should().Be(TRexCaches.kNonSpatialImmutable);

      TRexCaches.NonSpatialCacheName(StorageMutability.Immutable, FileSystemStreamType.SiteModelMachineElevationChangeMap).Should().Be(TRexCaches.kSiteModelChangeMapsCacheName);
      TRexCaches.NonSpatialCacheName(StorageMutability.Immutable, FileSystemStreamType.SiteModelMachineElevationChangeMap).Should().Be(TRexCaches.kSiteModelChangeMapsCacheName);
    }

    [Fact]
    public void SiteModelsCacheName()
    {
      TRexCaches.NonSpatialCacheName(StorageMutability.Mutable, FileSystemStreamType.ProductionDataXML).Should().Be(TRexCaches.kSiteModelsCacheMutable);
      TRexCaches.NonSpatialCacheName(StorageMutability.Immutable, FileSystemStreamType.ProductionDataXML).Should().Be(TRexCaches.kSiteModelsCacheImmutable);
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
