using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.Filters;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
    [TestClass]
    public class FilteredSinglePassInfoTests
    {
        [TestMethod]
        public void Test_FilteredSinglePass_Creation()
        {
            FilteredSinglePassInfo info = new FilteredSinglePassInfo();

            Assert.IsTrue(info.PassCount == 0, "Incorrect pass count after creation");
        }

        [TestMethod]
        public void Test_FilteredSinglePass_Clear()
        {
            Assert.Fail();
        }
    }
}
