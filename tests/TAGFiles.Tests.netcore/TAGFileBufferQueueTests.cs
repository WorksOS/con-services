using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.Servers.Client;
using VSS.VisionLink.Raptor.Servers.Compute;
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
            // ....
        }
    }
}
