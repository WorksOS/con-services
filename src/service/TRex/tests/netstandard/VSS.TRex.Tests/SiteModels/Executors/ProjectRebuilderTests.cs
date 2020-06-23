using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Nito.AsyncEx.Synchronous;
using VSS.AWS.TransferProxy;
using VSS.TRex.Common.Extensions;
using VSS.TRex.Common.Interfaces.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels.Executors;
using VSS.TRex.SiteModels.GridFabric.ComputeFuncs;
using VSS.TRex.SiteModels.GridFabric.Requests;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Executors;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.TAGFiles.GridFabric.Arguments;
using VSS.TRex.TAGFiles.GridFabric.ComputeFuncs;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SiteModels.GridFabric.Executors
{
  public class SiteModelRebuilderTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>, IDisposable
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
      IgniteMock.Mutable.AddApplicationGridRouting<SubmitTAGFileComputeFunc, SubmitTAGFileRequestArgument, SubmitTAGFileResponse>();
      IgniteMock.Mutable.AddApplicationGridRouting<ProcessTAGFileComputeFunc, ProcessTAGFileRequestArgument, ProcessTAGFileResponse>();
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
    public void SucceedWithPreexistingRebuildMetadataInCompleteState()
    {
      AddApplicationGridRouting();

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      siteModel.SaveMetadataToPersistentStore(siteModel.PrimaryStorageProxy, true);

      var metadataCache = DIContext.Obtain<Func<RebuildSiteModelCacheType, IStorageProxyCacheCommit>>()(RebuildSiteModelCacheType.Metadata)
        as IStorageProxyCache<INonSpatialAffinityKey, IRebuildSiteModelMetaData>;

      metadataCache.Put(new NonSpatialAffinityKey(siteModel.ID, SiteModelRebuilder.MetadataKeyName),
                        new RebuildSiteModelMetaData{Phase = RebuildSiteModelPhase.Complete});

      // Install an active builder into the manager to cause the failure.
      var rebuilder = new SiteModelRebuilder(siteModel.ID, false, TransferProxyType.TAGFiles);
      rebuilder.ValidateNoActiveRebuilderForProject(siteModel.ID).Should().BeTrue();
    }

    [Fact]
    public async void ExecuteAsync()
    {
      AddApplicationGridRouting();

      // Create sitemodel, add project metadata for it, check validator hates it...
      var sitemodel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      sitemodel.SaveMetadataToPersistentStore(sitemodel.PrimaryStorageProxy, true);

      var rebuilder = CreateBuilder(sitemodel.ID, false, TransferProxyType.TAGFiles);
      var result = rebuilder.ExecuteAsync().WaitAndUnwrapException();

      result.Should().NotBeNull();
      result.DeletionResult.Should().Be(DeleteSiteModelResult.OK);
    }

    [Fact]
    public async void ExecuteAsync_SingleTAGFile()
    {
      AddApplicationGridRouting();

      // Construct a site model from a single TAG file
      var tagFiles = new[] { Path.Combine(TestHelper.CommonTestDataPath, "TestTAGFile.tag") };
      var siteModel = DITAGFileAndSubGridRequestsFixture.BuildModel(tagFiles, out _);

      // Push the tag file into the S3 bucket 

      var s3Proxy = DIContext.Obtain<Func<TransferProxyType, IS3FileTransfer>>()(TransferProxyType.TAGFiles);
      s3Proxy.WriteFile(tagFiles[0], $"{siteModel.ID}/{siteModel.Machines[0].ID}/{Path.GetFileName(tagFiles[0])}");

      var rebuilder = CreateBuilder(siteModel.ID, false, TransferProxyType.TAGFiles);

      // Add the rebuilder to the manager in a 'hands-off' mode to allow notification routing to it.
      var manager = DIContext.Obtain<ISiteModelRebuilderManager>();
      manager.AddRebuilder(rebuilder).Should().BeTrue();

      // Start the rebuild executing
      var rebuilderTask = rebuilder.ExecuteAsync();

      // Wait until the rebuilder is in the monitoring state and then inject the contents of the tag file buffer queue cache into the handler
      while (rebuilder.Metadata.Phase != RebuildSiteModelPhase.Monitoring)
      {
        await Task.Delay(1000);
      }

      // While the rebuilder is waiting in the monitoring state, inject the contents of the TAGFileBuffer queue into the TAG file processor
      var mutableIgniteMock = DIContext.Obtain<Func<StorageMutability, IgniteMock>>()(StorageMutability.Mutable);
      var mockQueueCacheDictionary = mutableIgniteMock.MockedCacheDictionaries[TRexCaches.TAGFileBufferQueueCacheName()] as Dictionary<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>;

      var handler = new TAGFileBufferQueueItemHandler();
      handler.Should().NotBeNull();

      // Inject the keys for the handler to use to extract the TAG file content to be processed.
      mockQueueCacheDictionary.ForEach(kv => handler.Add(kv.Key));

      // Now wait for the rebuilder task to complete
      var result = await rebuilderTask;

      result.Should().NotBeNull();
      result.DeletionResult.Should().Be(DeleteSiteModelResult.OK);
      result.RebuildResult.Should().Be(RebuildSiteModelResult.OK);

      result.NumberOfTAGFileKeyCollections.Should().Be(1);
      result.NumberOfTAGFilesProcessed.Should().Be(1);
      result.NumberOfTAGFilesFromS3.Should().Be(1);

      result.LastProcessedTagFile.Should().Be(Path.GetFileName(tagFiles[0]));
    }

    public void Dispose()
    {
      DIContext.Obtain<ISiteModelRebuilderManager>().AbortAll();
    }
  }
}
