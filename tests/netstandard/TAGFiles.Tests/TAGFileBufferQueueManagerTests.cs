using Apache.Ignite.Core;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Servers;
using VSS.TRex.Servers.Client;
using Xunit;

namespace TAGFiles.Tests
{
    public class TAGFileBufferQueueManagerTests
    {
        private static MutableClientServer TAGClientServer = null;
        private static IIgnite ignite;

        private static void EnsureServer()
        {
            try
            {
                ignite = Ignition.GetIgnite(TRexGrids.MutableGridName());
            }
            catch
            {
                TAGClientServer = TAGClientServer ?? new MutableClientServer(ServerRoles.TAG_PROCESSING_NODE_CLIENT);
                ignite = Ignition.GetIgnite(TRexGrids.MutableGridName());
            }
        }

        [Fact(Skip = "Requires live Ignite node")]
        public void Test_TAGFileBufferQueueManager_Creation()
        {
            EnsureServer();

            TAGFileBufferQueueManager manager = new TAGFileBufferQueueManager(true);

            Assert.True(null != manager, "Failed to construct TAG file buffer queue manager");
        }
    }
}
