using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Volumes.GridFabric.Arguments;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests.Volumes
{
    [TestClass]
    public class SimpleVolumesRequestArgumentTests
    {
        [TestMethod]
        public void Test_SimpleVolumesRequestArgument_Creation()
        {
            SimpleVolumesRequestArgument arg = new SimpleVolumesRequestArgument();

            Assert.IsNotNull(arg, "Simple volumes request arg did not create");
        }
    }
}
