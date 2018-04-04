using System;
using VSS.VisionLink.Raptor.Filters;
using Xunit;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
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
