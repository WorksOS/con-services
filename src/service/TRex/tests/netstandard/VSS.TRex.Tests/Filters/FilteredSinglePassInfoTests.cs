using FluentAssertions;
using VSS.TRex.Filters.Models;
using Xunit;

namespace VSS.TRex.Tests.Filters
{
        public class FilteredSinglePassInfoTests
    {
        [Fact]
        public void Test_FilteredSinglePass_Creation()
        {
            FilteredSinglePassInfo info = new FilteredSinglePassInfo();

            Assert.Equal(0, info.PassCount);
        }

        [Fact]
        public void Test_FilteredSinglePass_Clear()
        {
          FilteredSinglePassInfo info = new FilteredSinglePassInfo
          {
            PassCount = 5
          };

          info.PassCount.Should().Be(5);

          info.Clear();
          info.PassCount.Should().Be(0);
        }
    }
}
