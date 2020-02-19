using FluentAssertions;
using VSS.TRex.DataSmoothing;
using VSS.TRex.Types.CellPasses;
using Xunit;

namespace VSS.TRex.Tests.DataSmoothing
{
  public class ArrayDataSmootherTests
  {
    [Fact]
    public void Creation()
    {
      var tools = new ConvolutionTools<float>();
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, ConvolutionMaskSize.Mask3X3);
      var filter = new double[3, 3];

      var smoother = new ArrayDataSmoother<float>(tools, ConvolutionMaskSize.Mask3X3, accum,
        (acc, size) => new FilterConvolver<float>(accum, filter, false, false));

      smoother.Should().NotBeNull();
      smoother.AdditionalBorderSize.Should().Be(3 / 2);
    }

    [Fact]
    public void Smooth()
    {
      const double oneNinth = 1d / 9d;

      var sourceArray = new float[10, 10];
      for (var i = 0; i < 10; i++)
      {
        for (var j = 0; j < 10; j++)
        {
          sourceArray[i, j] = 10.0f;
        }
      }

      var tools = new ConvolutionTools<float>();
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, ConvolutionMaskSize.Mask3X3);
      var filter = new double[3, 3] {
        {
          oneNinth, oneNinth, oneNinth
        },
        {
          oneNinth, oneNinth, oneNinth
        },
        {
          oneNinth, oneNinth, oneNinth
        }
      };

      var smoother = new ArrayDataSmoother<float>(tools, ConvolutionMaskSize.Mask3X3, accum,
        (accum, size) => new FilterConvolver<float>(accum, filter, false, false));

      var result = smoother.Smooth(sourceArray);

      result.Should().BeEquivalentTo(sourceArray);
    }
  }
}
