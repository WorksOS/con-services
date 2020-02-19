using FluentAssertions;
using VSS.TRex.DataSmoothing;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using Xunit;

namespace VSS.TRex.Tests.DataSmoothing
{
  public class ElevationTreeSmootherTests
  {
    [Fact]
    public void Creation()
    {
      var tools = new ConvolutionTools<float>();

      var smoother = new ElevationTreeSmoother(tools, ConvolutionMaskSize.Mask3X3, NullInfillMode.NoInfill);

      smoother.Should().NotBeNull();
    }

    [Fact]
    public void Smooth()
    {
      const float elevation = 10.0f;

      var source = DataSmoothingTestUtilities.ConstructSingleSubGridElevationSubGridTreeAtOrigin(elevation);
      var sourceSubGrid = source.LocateSubGridContaining(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.SubGridTreeLevels) as GenericLeafSubGrid<float>;
      sourceSubGrid.Should().NotBeNull();

      var tools = new ConvolutionTools<float>();
      var smoother = new ElevationTreeSmoother(tools, ConvolutionMaskSize.Mask3X3, NullInfillMode.NoInfill);

      var result = smoother.Smooth(source);

      var resultSubGrid = result.LocateSubGridContaining(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.SubGridTreeLevels) as GenericLeafSubGrid<float>;
      resultSubGrid.Should().NotBeNull();
      resultSubGrid.Items.Should().BeEquivalentTo(sourceSubGrid.Items);
    }
  }
}
