using System;
using FluentAssertions;
using VSS.TRex.ElevationSmoothing;
using VSS.TRex.Types.CellPasses;
using Xunit;

namespace VSS.TRex.Tests.ElevationSmoothing
{
  public class FilterConvolverTests
  {
    [Fact]
    public void Creation_Base()
    {
      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight);
      var filter = new Filter<float>(accumulator, new double[3, 3]);
      filter.Should().NotBeNull();
    }

    [Fact]
    public void Creation_Base_FailWithFilterDimensionMisMatch()
    {
      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight);
      Action act = () => _ = new Filter<float>(accumulator, new double[3, 4]);
      act.Should().Throw<ArgumentException>().WithMessage($"Major dimension (3) and minor dimension (4) of filterMatrix must be the same");
    }

    [Fact]
    public void Creation_Mean1()
    {
      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight);
      var filter = new MeanFilter<float>(accumulator, new double[3, 3]);
      filter.Should().NotBeNull();
    }

    [Fact]
    public void Creation_Mean2()
    {
      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight);
      var filter = new MeanFilter<float>(accumulator, 3);
      filter.Should().NotBeNull();
    }

    [Fact]
    public void Creation_WeightedMean()
    {
      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight);
      var filter = new WeightedMeanFilter<float>(accumulator, 3, 2.0);
      filter.Should().NotBeNull();
    }

    [Fact]
    public void Creation_WeightedMean_FailWithContextSizeOutOfRange()
    {
      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight);
      Action act = () => _ = new WeightedMeanFilter<float>(accumulator, 100, 2.0);
      act.Should().Throw<ArgumentException>().WithMessage($"Context size of {100} is out of range: 3..11");
    }

    [Fact]
    public void Creation_WeightedMean_CenterWeightCorrect()
    {
      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight);
      var filter = new WeightedMeanFilter<float>(accumulator, 3, 2.0);
      filter.Should().NotBeNull();

      filter.FilterMatrix[1, 1].Should().Be(2 / 10.0d);
    }
  }
}
