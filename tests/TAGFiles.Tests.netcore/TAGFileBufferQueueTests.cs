using System;
using System.IO;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Configuration;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.VisionLink.Raptor.GridFabric.Caches;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.Servers.Client;
using VSS.VisionLink.Raptor.Servers.Compute;
using VSSTests.TRex.Tests.Common;
using Xunit;

namespace TAGFiles.Tests.netcore
{
    public class TAGFileBufferQueueTests
    {
        private static RaptorMutableClientServer TAGClientServer = null;

        private static void EnsureServer()
        {
            TAGClientServer = TAGClientServer ?? new RaptorMutableClientServer(ServerRoles.TAG_PROCESSING_NODE_CLIENT);

            Assert.NotNull(TAGClientServer);
        }

        [Fact()]
        public void Test_TAGFileBufferQueue_Creation()
        {
            EnsureServer();

            TAGFileBufferQueue queue = new TAGFileBufferQueue();
            Assert.NotNull(queue);
        }

        [Fact()]
        public void Test_TAGFileBufferQueue_AddingTAGFiles()
        {
            EnsureServer();

            TAGFileBufferQueue queue = new TAGFileBufferQueue();
            Assert.NotNull(queue);

            // Load a TAG file and add it to the queue. Verify the TAG file appears in the cache

            string tagFileName = "TestTAGFile - TAGFile - Read - Stream.tag";
            Guid ProjectUID = Guid.NewGuid();
            byte[] tagContent;
            using (FileStream tagFileStream =
                new FileStream(TAGTestConsts.TestDataFilePath() + "TAGFiles\\TestTAGFile-TAGFile-Read-Stream.tag",
                    FileMode.Open, FileAccess.Read))
            {
                tagContent = new byte[tagFileStream.Length];
                tagFileStream.Read(tagContent, 0, (int) tagFileStream.Length);
            }

            TAGFileBufferQueueKey tagKey = new TAGFileBufferQueueKey(tagFileName, ProjectUID);
            TAGFileBufferQueueItem tagItem = new TAGFileBufferQueueItem
            {
                InsertUTC = DateTime.Now,
                ProjectUID = ProjectUID,
                AssetUID = Guid.NewGuid(),
                FileName = tagFileName,
                Content = tagContent
            };

            queue.Add(tagKey, tagItem);

            IIgnite ignite = Ignition.GetIgnite(RaptorGrids.RaptorMutableGridName());
            ICache<TAGFileBufferQueueKey, TAGFileBufferQueueItem> QueueCache =
                ignite.GetCache<TAGFileBufferQueueKey, TAGFileBufferQueueItem>(RaptorCaches.TAGFileBufferQueueCacheName());

            TAGFileBufferQueueItem tagItem2 = QueueCache.Get(tagKey);

            Assert.True(tagItem2 != null, "Tag item read back from buffer queue cache was null");
            Assert.True(tagItem.Content.Length == tagItem2.Content.Length, "Tag content lengths different");
            Assert.True(tagItem.InsertUTC == tagItem2.InsertUTC, "Tag insert UTCs different");
            Assert.True(tagItem.AssetUID == tagItem2.AssetUID, "Tag AssetUIDs different");
            Assert.True(tagItem.FileName == tagItem2.FileName, "Tag FileNames different");
            Assert.True(tagItem.ProjectUID == tagItem2.ProjectUID, "Tag ProjectUIDs different");
        }
    }
}
