using FluentAssertions;
using VSS.TRex.DataSmoothing;
using Xunit;

namespace VSS.TRex.Tests.DataSmoothing
{
  public class ElevationArraySmootherTests
  {
    [Fact]
    public void Creation()
    {
      var source = new float[10, 10];
      var tools = new ConvolutionTools<float>();

      var smoother = new ElevationArraySmoother(source, tools, 3, false, false);
      smoother.Should().NotBeNull();
    }

    [Fact]
    public void Smooth()
    {
      var source = new float[10, 10];
      for (var i = 0; i < 10; i++)
      {
        for (var j = 0; j < 10; j++)
        {
          source[i, j] = 10.0f;
        }
      }

      var tools = new ConvolutionTools<float>();
      var smoother = new ElevationArraySmoother(source, tools, 3, false, false);

      var result = smoother.Smooth();

      result.Should().BeEquivalentTo(source);
    }
  }
}
