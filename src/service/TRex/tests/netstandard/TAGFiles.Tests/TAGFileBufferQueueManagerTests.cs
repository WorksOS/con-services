using VSS.TRex.DI;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Servers.Client;
using VSS.TRex.Storage.Models;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace TAGFiles.Tests
{
    public class TAGFileBufferQueueManagerTests : IClassFixture<DILoggingFixture>
  {
        private static MutableClientServer TAGClientServer;

        private static void EnsureServer()
        {
            try
            {
                _ = DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Mutable);
            }
            catch
            {
                TAGClientServer = TAGClientServer ?? new MutableClientServer(ServerRoles.TAG_PROCESSING_NODE_CLIENT);
                _ = DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Mutable);
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
