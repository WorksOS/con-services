using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Utilities;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
    [TestClass]
    public class PassCountTests
    {
        [TestMethod]
        public void Test_PassCountSizes()
        {
            Assert.IsTrue(PassCountSize.Calculate(0) == 1, "Incorrect pass count size for count == 0");
            Assert.IsTrue(PassCountSize.Calculate(1) == 1, "Incorrect pass count size for count == 1");
            Assert.IsTrue(PassCountSize.Calculate(255) == 1, "Incorrect pass count size for count == 255");
            Assert.IsTrue(PassCountSize.Calculate(256) == 2, "Incorrect pass count size for count == 256");
            Assert.IsTrue(PassCountSize.Calculate(65535) == 2, "Incorrect pass count size for count == 65535");
            Assert.IsTrue(PassCountSize.Calculate(65536) == 3, "Incorrect pass count size for count == 65536");
            Assert.IsTrue(PassCountSize.Calculate(1000000) == 3, "Incorrect pass count size for count == 1000000");
        }
    }
}
