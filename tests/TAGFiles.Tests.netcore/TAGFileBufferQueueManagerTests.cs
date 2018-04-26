using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Ignite.Core;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.Servers.Client;
using Xunit;

namespace TAGFiles.Tests.netcore
{
    public class TAGFileBufferQueueManagerTests
    {
        private static RaptorMutableClientServer TAGClientServer = null;
        private static IIgnite ignite;

        private static void EnsureServer()
        {
            try
            {
                ignite = Ignition.GetIgnite(RaptorGrids.RaptorMutableGridName());
            }
            catch
            {
                TAGClientServer = TAGClientServer ?? new RaptorMutableClientServer(ServerRoles.TAG_PROCESSING_NODE_CLIENT);
                ignite = Ignition.GetIgnite(RaptorGrids.RaptorMutableGridName());
            }
        }

        [Fact]
        public void Test_TAGFileBufferQueueManager_Creation()
        {
            EnsureServer();

            TAGFileBufferQueueManager manager = new TAGFileBufferQueueManager();

            Assert.True(null != manager, "Failed to construct TAG file buffer queue manager");
        }
    }
}
