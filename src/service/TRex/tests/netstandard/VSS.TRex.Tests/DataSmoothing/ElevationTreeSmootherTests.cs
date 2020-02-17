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
      var source = new GenericSubGridTree<float, GenericLeafSubGrid<float>>();
      var tools = new ConvolutionTools<float>();

      var smoother = new ElevationTreeSmoother(source, tools, 3, false, false);

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
      var smoother = new ElevationTreeSmoother(source, tools, 3, false, false);

      var result = smoother.Smooth();

      var resultSubGrid = result.LocateSubGridContaining(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.SubGridTreeLevels) as GenericLeafSubGrid<float>;
      resultSubGrid.Should().NotBeNull();
      resultSubGrid.Items.Should().BeEquivalentTo(sourceSubGrid.Items);
    }
  }
}
