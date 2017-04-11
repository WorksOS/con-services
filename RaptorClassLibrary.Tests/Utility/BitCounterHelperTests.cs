using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.SubGridTrees.Helpers;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
    [TestClass]
    public class BitCounterHelperTests
    {
        //[TestMethod] Hidden as this test takes a long time...
        public void Test_CountBits()
        {
            uint failedItem = 0;

            Assert.IsTrue(BitCounterHelper.TestCountSetBits(out failedItem) == true, "Subgrid tree BitCOunterHelper self test failed at {0}", failedItem);
        }
    }
}
