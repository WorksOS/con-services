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
      var sourceArray = new float[10, 10];
      var tools = new ConvolutionTools<float>();
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight);
      var filter = new double[3, 3];

      var smoother = new ArrayDataSmoother<float>(sourceArray, tools, 3, accum,
        (acc, size) => new FilterConvolver<float>(accum, filter, false, false));

      smoother.Should().NotBeNull();
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
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight);
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

      var smoother = new ArrayDataSmoother<float>(sourceArray, tools, 3, accum,
        (accum, size) => new FilterConvolver<float>(accum, filter, false, false));

      var result = smoother.Smooth();

      result.Should().BeEquivalentTo(sourceArray);
    }
  }
}
