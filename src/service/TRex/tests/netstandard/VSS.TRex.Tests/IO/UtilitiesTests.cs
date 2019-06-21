using FluentAssertions;
using Xunit;

namespace VSS.TRex.Tests.IO
{
  public class UtilitiesTests
  {
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(255, 8)]
    [InlineData(1 << 8, 9)]
    [InlineData(1023, 10)]
    [InlineData(1 << 10, 11)]
    [InlineData(32767, 15)]
    [InlineData(32768, 16)]
    [InlineData((1 << 20) - 1, 20)]
    [InlineData(1 << 20, 21)]
    public void Log2(int number, int expectedLog2)
    {
      VSS.TRex.IO.Utilities.Log2(number).Should().Be(expectedLog2);
    }
  }
}
