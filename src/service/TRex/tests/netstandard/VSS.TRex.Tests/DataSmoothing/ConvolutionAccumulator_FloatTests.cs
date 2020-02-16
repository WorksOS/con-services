using FluentAssertions;
using VSS.TRex.ElevationSmoothing;
using VSS.TRex.Types.CellPasses;
using Xunit;

namespace VSS.TRex.Tests.ElevationSmoothing
{
  public class ConvolutionAccumulator_FloatTests
  {
    [Fact]
    public void Creation()
    {
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight);

      accum.Should().NotBeNull();
      accum.NullValue.Should().Be(CellPassConsts.NullHeight);
    }

    [Fact]
    public void Accumulate_NoCoefficient()
    {
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight);
      accum.Accumulate(123.0f);
      accum.Result().Should().Be(123.0f);
      accum.NumNonNullValues.Should().Be(1);

      accum.Accumulate(177.0f);
      accum.Result().Should().Be(300.0f);
      accum.NumNonNullValues.Should().Be(2);

      accum.Accumulate(CellPassConsts.NullHeight);
      accum.Result().Should().Be(300.0f);
      accum.NumNonNullValues.Should().Be(2);

    }

    [Fact]
    public void Accumulate_WithCoefficient()
    {
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight)
      {
        ConvolutionSourceValue = 123.0f
      };

      accum.Accumulate(123.0f, 1.0);
      accum.Result().Should().Be(123.0f);
      accum.NumNonNullValues.Should().Be(1);

      accum.Accumulate(100.0f, 0.1);
      accum.Result().Should().Be(133.0f);
      accum.NumNonNullValues.Should().Be(2);

      accum.Accumulate(CellPassConsts.NullHeight, 0.1);
      accum.Result().Should().Be(145.3f);
      accum.NumNonNullValues.Should().Be(2);
    }

    [Fact]
    public void Clear()
    {
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight)
      {
        ConvolutionSourceValue = 123.0f
      };
      accum.Accumulate(123.0f, 1.0);
      accum.Result().Should().Be(123.0f);

      accum.Clear();

      accum.Result().Should().Be(CellPassConsts.NullHeight);
      accum.NumNonNullValues.Should().Be(0);
      accum.ConvolutionSourceValue.Should().Be(0);
    }

    [Fact]
    public void Result()
    {
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight);
      accum.Accumulate(123.0f, 1.0);
      accum.Result().Should().Be(123.0f);
    }
  }
}
