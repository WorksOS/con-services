using System;
using FluentAssertions;
using VSS.TRex.DataSmoothing;
using VSS.TRex.Types.CellPasses;
using Xunit;

namespace VSS.TRex.Tests.DataSmoothing
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
    public void Accumulate_NullValue_WithCoefficient()
    {
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight)
      {
        ConvolutionSourceValue = 123.0f
      };

      accum.Accumulate(CellPassConsts.NullHeight, 1.0);
      accum.Result().Should().Be(123.0f);
      accum.NumNonNullValues.Should().Be(0);
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
      accum.ConvolutionSourceValue.Should().Be(CellPassConsts.NullHeight);
    }

    [Fact]
    public void Result()
    {
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight);
      accum.Accumulate(123.0f, 1.0);
      accum.Result().Should().Be(123.0f);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    public void NullInfillResult_BelowConcensus(int contextSize)
    {
      const float accumValue = 100.0f;
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight)
      {
        ConvolutionSourceValue = CellPassConsts.NullHeight
      };

      var concensusFraction = (int)Math.Truncate(0.5 * (contextSize * contextSize));

      for (var i = 0; i < concensusFraction; i++)
      {
        accum.Accumulate(accumValue, 1.0);
      }

      accum.NullInfillResult(contextSize).Should().Be(CellPassConsts.NullHeight);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    public void NullInfillResult_AboveConcensus(int contextSize)
    {
      const float accumValue = 100.0f;

      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight)
      {
        ConvolutionSourceValue = CellPassConsts.NullHeight
      };

      float contextSizeSquare = contextSize * contextSize;
      var concensusFraction = (int)Math.Truncate(0.5f * contextSizeSquare);

      for (var i = 0; i < concensusFraction + 1; i++)
      {
        accum.Accumulate(accumValue, 1.0);
      }

      var expectedInfillResult = (float)(contextSizeSquare / (concensusFraction + 1)) * ((concensusFraction + 1) * accumValue);

      accum.NullInfillResult(contextSize).Should().Be(expectedInfillResult);
    }
  }
}
