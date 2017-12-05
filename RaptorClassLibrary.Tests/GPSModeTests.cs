using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.Compression.Tests
{
    [TestClass]
    public class GPSModeTests
    {
        [TestMethod]
        public void Test_GPSMode()
        {
            Assert.IsTrue(Enum.GetNames(typeof(GPSMode)).Length == 10, "Number of defined GPSModes is not equal to 10");
        }
    }
}
