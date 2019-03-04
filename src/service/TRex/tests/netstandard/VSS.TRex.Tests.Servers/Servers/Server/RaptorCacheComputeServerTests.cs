using System;
using System.Collections.Generic;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache.Configuration;
using FluentAssertions;
using Moq;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Designs.Servers.Client;
using VSS.TRex.DI;
using VSS.TRex.Exports.Servers.Client;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Servers.Client;
using VSS.TRex.GridFabric.Servers.Compute;
using VSS.TRex.Profiling.Servers.Client;
using VSS.TRex.Rendering.Servers.Client;
using VSS.TRex.Reports.Servers.Client;
using VSS.TRex.Storage.Caches;
using VSS.TRex.TAGFiles.Servers.Client;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Volumes.Servers.Client;
using Xunit;

namespace VSS.TRex.Tests.Servers.Server
{
  public class DIIgniteServers : DITAGFileAndSubGridRequestsWithIgniteFixture
  {
    public DIIgniteServers()
    {
      // Create a set of mocked configurations for caches the servers was want
      var mockedConfigs = new List<CacheConfiguration>
      {
        new CacheConfiguration(TRexCaches.MutableNonSpatialCacheName()),
        new CacheConfiguration(TRexCaches.MutableSpatialCacheName()),
        new CacheConfiguration(TRexCaches.ImmutableNonSpatialCacheName()),
        new CacheConfiguration(TRexCaches.ImmutableSpatialCacheName()),
        new CacheConfiguration(TRexCaches.SiteModelMetadataCacheName()),
        new CacheConfiguration(TRexCaches.DesignTopologyExistenceMapsCacheName()),
        new CacheConfiguration(TRexCaches.TAGFileBufferQueueCacheName()),
        new CacheConfiguration(TRexCaches.SegmentRetirementQueueCacheName())
      };

      var igniteConfiguration = new IgniteConfiguration
      {
        CacheConfiguration = mockedConfigs
      };

      // Ge the mocked Ignite instance and add the configuration to it
      var ignite = DIContext.Obtain<Mock<IIgnite>>();
      ignite.Setup(x => x.GetConfiguration()).Returns(igniteConfiguration);
    }
  }

  public class TRexCacheComputeServerTests : IClassFixture<DIIgniteServers>//, IDisposable
  {
    /*
    private DIIgniteServers DIInstance = null;

    public TRexCacheComputeServerTests()
    {
      DIInstance = new DIIgniteServers();
    }
    */



    [Fact]
    public void Test_TRexCacheComputeServer_Creation()
    {
      var server = new TagProcComputeServer();

      server.Should().NotBeNull();
      server.ImmutableClientServer.Should().NotBeNull();
    }

    [Fact]
    public void Test_ImmutableClientServer_Creation()
    {
      var server = new ImmutableClientServer("UnitTests");
      server.Should().NotBeNull();
    }

    [Fact]
    public void Test_MutableClientServer_Creation()
    {
      var server = new MutableClientServer("UnitTests");
      server.Should().NotBeNull();
    }

    [Fact]
    public void Test_ImmutableCacheComputeServer_Creation()
    {
      var server = new ImmutableCacheComputeServer();

      server.Should().NotBeNull();
    }

    [Fact]
    public void Test_TRexMutableCacheComputeServer_Creation()
    {
      var server = new MutableCacheComputeServer();

      server.Should().NotBeNull();
    }

    [Fact]
    public void Test_TAGFileProcessingClientServer_Creation()
    {
      var server = new TAGFileProcessingClientServer();

      server.Should().NotBeNull();
    }

    [Fact]
    public void Test_TAGFileSubmittingClientServer_Creation()
    {
      var server = new TAGFileSubmittingClientServer();

      server.Should().NotBeNull();
    }

    [Fact]
    public void Test_ApplicationServiceServer_Creation()
    {
      var server = new ApplicationServiceServer();

      server.Should().NotBeNull();
    }

    [Fact]
    public void Test_CalculateDesignElevationsServer_Creation()
    {
      var server = new CalculateDesignElevationsServer();

      server.Should().NotBeNull();
    }

    [Fact]
    public void Test_SubGridProcessingServer_Creation()
    {
      var server = new SubGridProcessingServer();

      server.Should().NotBeNull();
    }

    [Fact]
    public void Test_ProfilingServer_Creation()
    {
      var server = new ProfilingServer();

      server.Should().NotBeNull();

      server = new ProfilingServer(new [] {"UnitTest"});
      server.Should().NotBeNull();
    }

    [Fact]
    public void Test_GenericApplicationServiceServer_Creation()
    {
      var server = new GenericApplicationServiceServer();

      server.Should().NotBeNull();
    }

    [Fact]
    public void Test_TileRenderingServer_Creation()
    {
      var server = new TileRenderingServer();

      server.Should().NotBeNull();
    }

    [Fact]
    public void Test_SimpleVolumesServer_Creation()
    {
      var server = new SimpleVolumesServer();

      server.Should().NotBeNull();
    }

    [Fact]
    public void Test_PatchRequestServer_Creation()
    {
      var server = new PatchRequestServer();

      server.Should().NotBeNull();
    }

    [Fact]
    public void Test_TINSurfaceExportRequestServer_Creation()
    {
      var server = new TINSurfaceExportRequestServer();

      server.Should().NotBeNull();
    }

    [Fact]
    public void Test_GriddedReportRequestServer_Creation()
    {
      var server = new GriddedReportRequestServer();

      server.Should().NotBeNull();
    }

    [Fact]
    public void Test_StationOffsetReportRequestServer_Creation()
    {
      var server = new StationOffsetReportRequestServer();

      server.Should().NotBeNull();
    }

    [Fact]
    public void Test_CSVExportRequestServer_Creation()
    {
      var server = new CSVExportRequestServer();

      server.Should().NotBeNull();
    }  

    [Fact]
    public void Test_ActivatePersistentGridServer_Creation()
    {
      var server = new ActivatePersistentGridServer();
      server.Should().NotBeNull();

      server.SetGridActive(TRexGrids.ImmutableGridName()).Should().BeTrue();
      server.SetGridInActive(TRexGrids.ImmutableGridName()).Should().BeTrue();

    }

    [Fact]
    public void Test_ActivatePersistentGridServer_SetGridActive()
    {
      var server = new ActivatePersistentGridServer();
      server.Should().NotBeNull();

      server.SetGridActive(TRexGrids.ImmutableGridName()).Should().BeTrue();
    }

    [Fact]
    public void Test_ActivatePersistentGridServer_SetGridInactive()
    {
      var server = new ActivatePersistentGridServer();
      server.Should().NotBeNull();

      server.SetGridInActive(TRexGrids.ImmutableGridName()).Should().BeTrue();
    }

    [Fact]
    public void Test_ActivatePersistentGridServer_WaitUntilGridActive()
    {
      var server = new ActivatePersistentGridServer();
      server.Should().NotBeNull();

      server.WaitUntilGridActive(TRexGrids.ImmutableGridName()).Should().BeTrue();
    }

    [Fact]
    public void Test_ActivatePersistentGridServer_WaitUntilGridActive_Unavailable()
    {
      const string INVALID_GRID_NAME = "UnitTestsXXX";

      var server = new ActivatePersistentGridServer();
      server.Should().NotBeNull();

      Func<bool> act = () => server.WaitUntilGridActive(INVALID_GRID_NAME);
      act.Should().Throw<TRexException>().WithMessage($"{INVALID_GRID_NAME} is an unknown grid to create a reference for.");
    }

    /*
    public void Dispose()
    {
      DIInstance = null;
      DIBuilder.Eject();

      // Ensure the TRex global config has its preference for immutable data left alone...
      TRexServerConfig.Instance().UseMutableSpatialData = false;
      TRexServerConfig.Instance().UseMutableNonSpatialData = false;
    }
    */
  }
}
