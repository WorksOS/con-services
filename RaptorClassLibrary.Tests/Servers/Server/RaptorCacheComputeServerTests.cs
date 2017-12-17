using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Servers.Compute;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests.Servers.Server
{
    [TestClass]
    public class RaptorCacheComputeServerTests
    {
        [TestMethod]
        public void Test_RaptorCacheComputeServer_Creation()
        {
            RaptorCacheComputeServer server = new RaptorCacheComputeServer();

            Assert.IsTrue(server != null, "Server instance not instantiated");
        }
    }
}
