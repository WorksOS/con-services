using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.VisionLink.Raptor.Servers.Compute;
using Xunit;

namespace TAGFiles.Tests.netcore
{
    public class TAGFileBufferQueueTests
    {
        private static RaptorTAGProcComputeServer TAGProcessingServer = null;

//        private static void EnsureServer()
//        {
//            TAGProcessingServer = TAGProcessingServer ?? new RaptorTAGProcComputeServer();
//
//            Assert.NotNull(TAGProcessingServer);
//        }

        [Fact()]
        public void Test_TAGFileBufferQueue_Creation()
        {
//            EnsureServer();

            TAGFileBufferQueue queue = new TAGFileBufferQueue();
            Assert.NotNull(queue);
        }

        [Fact()]
        public void Test_TAGFileBufferQueue_AddingTAGFiles()
        {
//            EnsureServer();

            TAGFileBufferQueue queue = new TAGFileBufferQueue();
            Assert.NotNull(queue);

            // Load a TAG file and add it to the queue. Verify the TAG file appears in the cache
            // ....
        }
    }
}
