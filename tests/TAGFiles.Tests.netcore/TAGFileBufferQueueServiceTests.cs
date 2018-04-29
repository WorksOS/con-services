using System;
using System.Collections.Generic;
using System.Text;
using Apache.Ignite.Core;
using VSS.TRex.TAGFiles.Classes.Queues;
using VSS.TRex.TAGFiles.GridFabric.Services;
using VSS.VisionLink.Raptor.GridFabric.Grids;
using VSS.VisionLink.Raptor.Servers;
using VSS.VisionLink.Raptor.Servers.Client;
using Xunit;

namespace TAGFiles.Tests.netcore
{
    /// <summary>
    /// Tests to ensure the grid deployed service that takes TAG files in the buffer queue and sends them to the grouper 
    /// functions as expected.
    /// </summary>
    public class TAGFileBufferQueueServiceTests
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
        public void Test_TAGFileBufferQueueServiceTests_Creation()
        {
            EnsureServer();

            TAGFileBufferQueueServiceProxy serviceProxy = new TAGFileBufferQueueServiceProxy();

            Assert.True(serviceProxy != null);
        }

        [Fact]
        public void Test_TAGFileBufferQueueServiceTests_Deployinh()
        {
            EnsureServer();

            TAGFileBufferQueueServiceProxy serviceProxy = new TAGFileBufferQueueServiceProxy();
            serviceProxy.Deploy();

            Assert.True(true);
        }

    }
}
