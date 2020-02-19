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
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, ConvolutionMaskSize.Mask3X3);

      accum.Should().NotBeNull();
      accum.NullValue.Should().Be(CellPassConsts.NullHeight);
    }

    [Fact]
    public void Accumulate_NoCoefficient()
    {
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, ConvolutionMaskSize.Mask3X3);
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
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, ConvolutionMaskSize.Mask3X3)
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
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, ConvolutionMaskSize.Mask3X3)
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
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, ConvolutionMaskSize.Mask3X3)
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
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, ConvolutionMaskSize.Mask3X3);
      accum.Accumulate(123.0f, 1.0);
      accum.Result().Should().Be(123.0f);
    }

    [Theory]
    [InlineData(ConvolutionMaskSize.Mask3X3)]
    [InlineData(ConvolutionMaskSize.Mask5X5)]
    public void NullInfillResult_BelowConcensus(ConvolutionMaskSize contextSize)
    {
      const float accumValue = 100.0f;
      var contextSizeAsInt = (int) contextSize;

      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, contextSize)
      {
        ConvolutionSourceValue = CellPassConsts.NullHeight
      };

      var concensusFraction = (int)Math.Truncate(0.5 * (contextSizeAsInt * contextSizeAsInt));

      for (var i = 0; i < concensusFraction; i++)
      {
        accum.Accumulate(accumValue, 1.0);
      }

      accum.NullInfillResult().Should().Be(CellPassConsts.NullHeight);
    }

    [Theory]
    [InlineData(ConvolutionMaskSize.Mask3X3)]
    [InlineData(ConvolutionMaskSize.Mask5X5)]
    public void NullInfillResult_AboveConcensus(ConvolutionMaskSize contextSize)
    {
      const float accumValue = 100.0f;
      var contextSizeAsInt = (int)contextSize;

      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, contextSize)
      {
        ConvolutionSourceValue = CellPassConsts.NullHeight
      };

      float contextSizeSquare = contextSizeAsInt * contextSizeAsInt;
      var concensusFraction = (int)Math.Truncate(0.5f * contextSizeSquare);

      for (var i = 0; i < concensusFraction + 1; i++)
      {
        accum.Accumulate(accumValue, 1.0);
      }

      var expectedInfillResult = (contextSizeSquare / (concensusFraction + 1)) * ((concensusFraction + 1) * accumValue);

      accum.NullInfillResult().Should().Be(expectedInfillResult);
    }
  }
}
