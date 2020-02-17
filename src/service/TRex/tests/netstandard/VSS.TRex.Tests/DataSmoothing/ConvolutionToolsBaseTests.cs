using System;
using FluentAssertions;
using VSS.TRex.DataSmoothing;
using VSS.TRex.SubGridTrees;
using Xunit;

namespace VSS.TRex.Tests.DataSmoothing
{
  public class ConvolutionToolsBaseTests
  {
    [Fact]
    public void Creation()
    {
      var c = new ConvolutionToolsBase<float>();
      c.Should().NotBeNull();
    }

    [Fact]
    public void UnimplementedFunctionAssertions()
    {
      var c = new ConvolutionToolsBase<float>();

      Action act = () => c.Convolve((GenericLeafSubGrid<float>)null, null, null);
      act.Should().Throw<NotImplementedException>();

      act = () => c.Convolve((float[,])null, null, null);
      act.Should().Throw<NotImplementedException>();
    }
  }
}
