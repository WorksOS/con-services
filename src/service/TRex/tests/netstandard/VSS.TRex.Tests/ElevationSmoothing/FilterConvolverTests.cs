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
    public void Creation_WeightedMean_CenterWeightCorrect()
    {
      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight);
      var filter = new WeightedMeanFilter<float>(accumulator, 3, 2.0);
      filter.Should().NotBeNull();

      filter.FilterMatrix[1, 1].Should().Be(2 / 10.0d);
    }

  }
}
