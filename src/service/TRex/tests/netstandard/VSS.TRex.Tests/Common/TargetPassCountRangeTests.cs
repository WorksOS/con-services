using System;
using FluentAssertions;
using VSS.TRex.Common.Records;
using Xunit;

namespace VSS.TRex.Tests.Common
{
  public class TargetPassCountRangeTests
  {
    [Fact]
    public void SetMinMax()
    {
      var range = new PassCountRangeRecord();
      range.SetMinMax(11, 22);
      range.Min.Should().Be(11);
      range.Max.Should().Be(22);
    }

    [Fact]
    public void SetMinMax_InvalidArgument()
    {
      var range = new PassCountRangeRecord();
      Action act = () => range.SetMinMax(22, 11);

      act.Should().Throw<ArgumentException>()
        .WithMessage("Maximum value must be greater than or equal to minimum value.");
    }
  }
}
