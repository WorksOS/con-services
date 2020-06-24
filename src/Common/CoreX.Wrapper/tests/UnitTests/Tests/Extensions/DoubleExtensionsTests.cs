using CoreX.Wrapper.Extensions;
using FluentAssertions;
using Xunit;

namespace CoreX.Wrapper.UnitTests.Tests.Extensions
{
  public class DoubleExtensionsTests
  {
    [Theory]
    [InlineData(1, 0.0174533)]
    public void DegreesToRadians_should_return_expected_result(double degrees, double radians)
    {
      degrees.DegreesToRadians().Should().BeApproximately(radians, 0.0000001);
    }

    [Theory]
    [InlineData(1, 57.2958)]
    public void RadiansToDegrees_should_return_expected_result(double radians, double degrees)
    {
      radians.RadiansToDegrees().Should().BeApproximately(degrees, 0.0001);
    }
  }
}
