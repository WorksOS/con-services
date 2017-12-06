using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Volumes.GridFabric.ComputeFuncs;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests.Volumes
{
    [TestClass]
    public class SimpleVolumesRequestComputeFuncTests
    {
        [TestMethod]
        public void Test_SimpleVolumesRequestComputeFunc_Creation()
        {
            SimpleVolumesRequestComputeFunc func = new SimpleVolumesRequestComputeFunc();

            Assert.IsNotNull(func, "Simple volumes compute func did not create");
        }
    }
}
