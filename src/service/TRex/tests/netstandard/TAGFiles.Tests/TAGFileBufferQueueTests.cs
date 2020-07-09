using System;
using System.IO;
using Apache.Ignite.Core;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.TAGFiles.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
  public class TAGFileBufferQueueTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private static IIgnite _ignite;

    private static void EnsureServer()
    {
      _ignite = DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Mutable);
    }

    [Fact]
    public void Test_TAGFileBufferQueue_Creation()
    {
      EnsureServer();

      var queue = new TAGFileBufferQueue();
      Assert.NotNull(queue);
    }

    [Fact]
    public void Test_TAGFileBufferQueue_AddingTAGFile()
    {
      EnsureServer();

      var queue = new TAGFileBufferQueue();
      Assert.NotNull(queue);

      // Load a TAG file and add it to the queue. Verify the TAG file appears in the cache

      var tagFileName = "TestTAGFile-TAGFile-Read-Stream.tag";

      var projectUid = Guid.NewGuid();
      var assetUid = Guid.NewGuid();

      byte[] tagContent;
      using (var tagFileStream =
        new FileStream(Path.Combine("TestData", "TAGFiles", tagFileName),
          FileMode.Open, FileAccess.Read))
      {
        tagContent = new byte[tagFileStream.Length];
        tagFileStream.Read(tagContent, 0, (int) tagFileStream.Length);
      }

      var tagKey = new TAGFileBufferQueueKey(tagFileName, projectUid, assetUid);
      var tagItem = new TAGFileBufferQueueItem
      {
        InsertUTC = DateTime.UtcNow,
        ProjectID = projectUid,
        AssetID = assetUid,
        FileName = tagFileName,
        Content = tagContent
      };

      // Perform the actual add
      queue.Add(tagKey, tagItem);

      // Read it back from the cache to ensure it was added as expected.
      var queueCache = _ignite.GetCache<ITAGFileBufferQueueKey, TAGFileBufferQueueItem>(TRexCaches.TAGFileBufferQueueCacheName());

      var tagItem2 = queueCache.Get(tagKey);

      Assert.True(tagItem2 != null, "Tag item read back from buffer queue cache was null");
      Assert.True(tagItem.Content.Length == tagItem2.Content.Length, "Tag content lengths different");
      Assert.True(tagItem.InsertUTC == tagItem2.InsertUTC, "Tag insert UTCs different");
      Assert.True(tagItem.AssetID == tagItem2.AssetID, "Tag AssetUIDs different");
      Assert.True(tagItem.FileName == tagItem2.FileName, "Tag FileNames different");
      Assert.True(tagItem.ProjectID == tagItem2.ProjectID, "Tag ProjectUIDs different");
    }
  }
}
