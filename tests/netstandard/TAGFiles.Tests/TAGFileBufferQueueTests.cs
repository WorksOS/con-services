using System;
using System.IO;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Affinity;
using VSS.TRex.Servers;
using VSS.TRex.Servers.Client;
using VSS.TRex.Storage.Caches;
using VSS.TRex.TAGFiles.Models;
using Xunit;

namespace TAGFiles.Tests
{
    public class TAGFileBufferQueueTests
    {
        private static MutableClientServer TAGClientServer = null;
        private static IIgnite ignite;

        private static void EnsureServer()
        {
            try
            {
                ignite = Ignition.GetIgnite(TRexGrids.MutableGridName());
            }
            catch (Exception)
            {
                TAGClientServer = TAGClientServer ?? new MutableClientServer(ServerRoles.TAG_PROCESSING_NODE_CLIENT);
                ignite = Ignition.GetIgnite(TRexGrids.MutableGridName());
            }
        }

        [Fact(Skip = "Requires live Ignite node")]
        public void Test_TAGFileBufferQueue_Creation()
        {
            EnsureServer();

            TAGFileBufferQueue queue = new TAGFileBufferQueue();
            Assert.NotNull(queue);
        }

        [Fact(Skip = "Requires live Ignite node")]
        public void Test_TAGFileBufferQueue_AddingTAGFile()
        {
            EnsureServer();

            TAGFileBufferQueue queue = new TAGFileBufferQueue();
            Assert.NotNull(queue);

      // Load a TAG file and add it to the queue. Verify the TAG file appears in the cache

            string tagFileName = "TestTAGFile-TAGFile-Read-Stream.tag";

            Guid projectID = Guid.NewGuid();
            Guid assetID = Guid.NewGuid();

            byte[] tagContent;
            using (FileStream tagFileStream =
                new FileStream(Path.Combine("TestData", "TAGFiles", tagFileName),
                    FileMode.Open, FileAccess.Read))
            {
                tagContent = new byte[tagFileStream.Length];
                tagFileStream.Read(tagContent, 0, (int) tagFileStream.Length);
            }

            TAGFileBufferQueueKey tagKey = new TAGFileBufferQueueKey(tagFileName, projectID, assetID);
            TAGFileBufferQueueItem tagItem = new TAGFileBufferQueueItem
            {
                InsertUTC = DateTime.Now,
                //ProjectUID = projectUID,
                //AssetUID = assetUID,
                ProjectID = projectID,
                AssetID = assetID,
                FileName = tagFileName,
                Content = tagContent
            };

            // Perform the actual add
            queue.Add(tagKey, tagItem);

            // Read it back from the cache to ensure it was added as expected.
            ICache<TAGFileBufferQueueKey, TAGFileBufferQueueItem> QueueCache =
                ignite.GetCache<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(
                    TRexCaches.TAGFileBufferQueueCacheName());

            TAGFileBufferQueueItem tagItem2 = QueueCache.Get(tagKey);

            Assert.True(tagItem2 != null, "Tag item read back from buffer queue cache was null");
            Assert.True(tagItem.Content.Length == tagItem2.Content.Length, "Tag content lengths different");
            Assert.True(tagItem.InsertUTC == tagItem2.InsertUTC, "Tag insert UTCs different");
            Assert.True(tagItem.AssetID == tagItem2.AssetID, "Tag AssetUIDs different");
            Assert.True(tagItem.FileName == tagItem2.FileName, "Tag FileNames different");
            Assert.True(tagItem.ProjectID == tagItem2.ProjectID, "Tag ProjectUIDs different");
        }
    }
}
