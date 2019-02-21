using FluentAssertions;
using VSS.TRex.SubGridTrees.Core.Helpers;
using Xunit;

namespace VSS.TRex.Tests.Utility
{
    public class BitCounterHelperTests
    {
        [Fact]
        public void Test_CountBits()
        {
            BitCounterHelper.TestCountSetBits(out _).Should().BeTrue();
        }
    }
}
