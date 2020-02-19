using System;
using FluentAssertions;
using VSS.TRex.DataSmoothing;
using VSS.TRex.Types.CellPasses;
using Xunit;

namespace VSS.TRex.Tests.DataSmoothing
{
  public class FilterConvolverTests
  {
    [Fact]
    public void Creation_Base()
    {
      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, ConvolutionMaskSize.Mask3X3);
      var filter = new FilterConvolver<float>(accumulator, new double[3, 3], false, false);
      filter.Should().NotBeNull();

      filter.InfillNullValuesOnly.Should().BeFalse();
      filter.UpdateNullValues.Should().BeFalse();

      filter = new FilterConvolver<float>(accumulator, new double[3, 3], true, true);
      filter.InfillNullValuesOnly.Should().BeTrue();
      filter.UpdateNullValues.Should().BeTrue();
    }

    [Fact]
    public void Creation_FailWithFilterDimensionMisMatch()
    {
      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, ConvolutionMaskSize.Mask3X3);
      Action act = () => _ = new FilterConvolver<float>(accumulator, new double[3, 4], false, false);
      act.Should().Throw<ArgumentException>().WithMessage("Major dimension (3) and minor dimension (4) of filterMatrix must be the same");
    }

    [Fact]
    public void Creation_FailWithInvalidFilterDimension()
    {
      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, ConvolutionMaskSize.Mask3X3);
      Action act = () => _ = new FilterConvolver<float>(accumulator, new double[4, 4], false, false);
      act.Should().Throw<ArgumentException>().WithMessage("Context size must be positive odd number greater than 1");
    }

    [Fact]
    public void Creation_Mean1()
    {
      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, ConvolutionMaskSize.Mask3X3);
      var filter = new MeanFilter<float>(accumulator, new double[3, 3], false, false);
      filter.Should().NotBeNull();
    }

    [Fact]
    public void Creation_Mean2()
    {
      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, ConvolutionMaskSize.Mask3X3);
      var filter = new MeanFilter<float>(accumulator, ConvolutionMaskSize.Mask3X3, false, false);
      filter.Should().NotBeNull();
    }

    [Fact]
    public void Creation_WeightedMean()
    {
      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, ConvolutionMaskSize.Mask3X3);
      var filter = new WeightedMeanFilter<float>(accumulator, ConvolutionMaskSize.Mask3X3, 2.0, false, false);
      filter.Should().NotBeNull();
    }

    [Fact]
    public void Creation_WeightedMean_FailWithContextSizeOutOfRange()
    {
      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, ConvolutionMaskSize.Mask3X3);
      Action act = () => _ = new WeightedMeanFilter<float>(accumulator, (ConvolutionMaskSize)100, 2.0, false, false);
      act.Should().Throw<ArgumentException>().WithMessage("Context size of 100 is out of range: 3..11");
    }

    [Fact]
    public void Creation_WeightedMean_CenterWeightCorrect()
    {
      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, ConvolutionMaskSize.Mask3X3);
      var filter = new WeightedMeanFilter<float>(accumulator, ConvolutionMaskSize.Mask3X3, 2.0, false, false);
      filter.Should().NotBeNull();

      filter.FilterMatrix[1, 1].Should().Be(2 / 10.0d);
    }

    [Fact]
    public void ContextSize()
    {
      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, ConvolutionMaskSize.Mask3X3);
      var filter = new WeightedMeanFilter<float>(accumulator, ConvolutionMaskSize.Mask3X3, 2.0, false, false);
      filter.Should().NotBeNull();
      filter.ContextSize.Should().Be(3);
    }
  }
}
