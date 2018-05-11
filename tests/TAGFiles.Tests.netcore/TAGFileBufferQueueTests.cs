using System;
using System.IO;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.Servers.Client;
using VSSTests.TRex.Tests.Common;
using Xunit;

namespace TAGFiles.Tests.netcore
{
    public class TAGFileBufferQueueTests
    {
        private static RaptorMutableClientServer TAGClientServer = null;
        private static IIgnite ignite;

        private static void EnsureServer()
        {
            try
            {
                ignite = Ignition.GetIgnite(RaptorGrids.RaptorMutableGridName());
            }
            catch (Exception e)
            {
                TAGClientServer = TAGClientServer ?? new RaptorMutableClientServer(ServerRoles.TAG_PROCESSING_NODE_CLIENT);
                ignite = Ignition.GetIgnite(RaptorGrids.RaptorMutableGridName());
            }
        }

        [Fact()]
        public void Test_TAGFileBufferQueue_Creation()
        {
            EnsureServer();

            TAGFileBufferQueue queue = new TAGFileBufferQueue();
            Assert.NotNull(queue);
        }

        [Fact()]
        public void Test_TAGFileBufferQueue_AddingTAGFile()
        {
            EnsureServer();

            TAGFileBufferQueue queue = new TAGFileBufferQueue();
            Assert.NotNull(queue);

            // Load a TAG file and add it to the queue. Verify the TAG file appears in the cache

            string tagFileName = "TestTAGFile - TAGFile - Read - Stream.tag";

            Guid projectID = Guid.NewGuid();
            Guid assetID = Guid.NewGuid();

            byte[] tagContent;
            using (FileStream tagFileStream =
                new FileStream(TAGTestConsts.TestDataFilePath() + "TAGFiles\\TestTAGFile-TAGFile-Read-Stream.tag",
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
                    RaptorCaches.TAGFileBufferQueueCacheName());

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
