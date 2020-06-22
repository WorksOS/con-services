using System;
using System.IO;
using FluentAssertions;
using VSS.AWS.TransferProxy;
using VSS.TRex.Common;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels.Executors;
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
    private ISiteModelRebuilder CreateBuilder(Guid projectUid, bool archiveTAGFiles, TransferProxyType transferProxyType)
    {
      return new SiteModelRebuilder(projectUid, archiveTAGFiles, transferProxyType)
      {
        MetadataCache = DIContext.Obtain<Func<RebuildSiteModelCacheType, IStorageProxyCacheCommit>>()(RebuildSiteModelCacheType.Metadata)
                  as IStorageProxyCache<INonSpatialAffinityKey, IRebuildSiteModelMetaData>,
        FilesCache = DIContext.Obtain<Func<RebuildSiteModelCacheType, IStorageProxyCacheCommit>>()(RebuildSiteModelCacheType.KeyCollections)
               as IStorageProxyCache<INonSpatialAffinityKey, ISerialisedByteArrayWrapper>
      };
    }

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
      var rebuilder = CreateBuilder(Guid.NewGuid(), false, TransferProxyType.TAGFiles);

      Assert.True(rebuilder.ValidateNoActiveRebuilderForProject(Guid.NewGuid()));
    }

    [Fact]
    public async void ExecuteAsync()
    {
      AddApplicationGridRouting();

      // Create sitemodel, add project metadata for it, check validator hates it...
      var sitemodel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var rebuilder = CreateBuilder(sitemodel.ID, false, TransferProxyType.TAGFiles);
      var result = await rebuilder.ExecuteAsync();

      result.Should().NotBeNull();
      result.DeletionResult.Should().Be(DeleteSiteModelResult.OK);
    }

    [Fact] async void ExecuteAsync_SingleTAGFile()
    {
      AddApplicationGridRouting();

      // Construct a site model from a single TAG file
      var tagFiles = new[] { Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag") };
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      // Push the tag file into the S3 bucket 
      var s3Proxy = new S3FileTransfer(TransferProxyType.TAGFiles);
      s3Proxy.WriteFile(tagFiles[0], $"{siteModel.ID}/{siteModel.Machines[0].ID}/{tagFiles[0]}");

      var rebuilder = CreateBuilder(siteModel.ID, false, TransferProxyType.TAGFiles);

      var result = await rebuilder.ExecuteAsync();

      result.Should().NotBeNull();
      result.DeletionResult.Should().Be(DeleteSiteModelResult.OK);
      result.RebuildResult.Should().Be(RebuildSiteModelResult.OK);

      result.NumberOfTAGFileKeyCollections.Should().Be(1);
      result.NumberOfTAGFilesProcessed.Should().Be(1);
      result.NumberOfTAGFilesFromS3.Should().Be(1);

      result.LastProcessedTagFile.Should().Be(tagFiles[0]);
    }
  }
}
