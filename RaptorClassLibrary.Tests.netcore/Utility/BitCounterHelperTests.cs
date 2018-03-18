using System;
using VSS.VisionLink.Raptor.SubGridTrees.Helpers;
using Xunit;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
        public class BitCounterHelperTests
    {
        //[TestMethod] Hidden as this test takes a long time...
        public void Test_CountBits()
        {
            uint failedItem = 0;

            Assert.True(BitCounterHelper.TestCountSetBits(out failedItem));
        }
    }
}
