using System;
using VSS.TRex.SubGridTrees.Helpers;
using Xunit;

namespace VSS.TRex.Tests.Utility
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
