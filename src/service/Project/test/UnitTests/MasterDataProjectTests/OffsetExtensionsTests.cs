using VSS.Productivity3D.Project.Abstractions.Extensions;
using Xunit;

namespace VSS.MasterData.ProjectTests
{
  public class OffsetExtensionsTests
  {
    [Theory]
    [InlineData(null, null)]
    [InlineData(null, 0.0)]
    [InlineData(0.0, null)]
    [InlineData(0.0, 0.0)]
    [InlineData(0.1, 0.1)]
    [InlineData(1.2349, 1.2341)]
    public void NullableOffsetsShouldBeEqual(double? offset1, double? offset2)
    {
      Assert.True(offset1.EqualsToNearestMillimeter(offset2));
    }

    [Theory]
    [InlineData(null, 1.0)]
    [InlineData(1.0, null)]
    [InlineData(0.0, 1.0)]
    [InlineData(1.234, 1.235)]
    public void NullableOffsetsShouldBeNotEqual(double? offset1, double? offset2)
    {
      Assert.False(offset1.EqualsToNearestMillimeter(offset2));
    }

    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(0.1, 0.1)]
    [InlineData(1.2349, 1.2341)]
    public void OffsetsShouldBeEqual(double offset1, double offset2)
    {
      Assert.True(offset1.EqualsToNearestMillimeter(offset2));
    }

    [Theory]
    [InlineData(0.0, 1.0)]
    [InlineData(1.234, 1.235)]
    public void OffsetsShouldBeNotEqual(double offset1, double offset2)
    {
      Assert.False(offset1.EqualsToNearestMillimeter(offset2));
    }
  }
}
