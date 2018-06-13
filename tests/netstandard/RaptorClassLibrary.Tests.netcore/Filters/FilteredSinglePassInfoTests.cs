using System;
using VSS.TRex.Filters;
using Xunit;

namespace VSS.TRex.Tests
{
        public class FilteredSinglePassInfoTests
    {
        [Fact]
        public void Test_FilteredSinglePass_Creation()
        {
            FilteredSinglePassInfo info = new FilteredSinglePassInfo();

            Assert.Equal(0, info.PassCount);
        }

        [Fact(Skip = "Not Implemented")]
        public void Test_FilteredSinglePass_Clear()
        {
            Assert.True(false);
        }
    }
}
