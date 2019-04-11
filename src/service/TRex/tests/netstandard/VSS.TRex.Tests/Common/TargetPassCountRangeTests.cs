using FluentAssertions;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Common
{
  public class TargetPassCountRangeTests
  {
    [Fact]
    public void SetMinMax()
    {
      var range = new TargetPassCountRange();
      range.SetMinMax(11, 22);
      range.Min.Should().Be(11);
      range.Max.Should().Be(22);
    }
  }
}
