using FluentAssertions;
using VSS.TRex.DataSmoothing;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types.CellPasses;
using Xunit;

namespace VSS.TRex.Tests.DataSmoothing
{
  public class TreeDataSmootherTests
  {
    [Fact]
    public void Creation()
    {
      var tools = new ConvolutionTools<float>();
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, ConvolutionMaskSize.Mask3X3);
      var filter = new double[3, 3];

      var smoother = new TreeDataSmoother<float>(tools, ConvolutionMaskSize.Mask3X3, accum,
        (acc, size) => new FilterConvolver<float>(accum, filter, false, false));

      smoother.Should().NotBeNull();
      smoother.AdditionalBorderSize.Should().Be(3 / 2);
    }

    [Fact]
    public void Smooth()
    {
      const float elevation = 10.0f;
      const double oneNinth = 1d / 9d;

      var source = DataSmoothingTestUtilities.ConstructSingleSubGridElevationSubGridTreeAtOrigin(elevation);
      var sourceSubGrid = source.LocateSubGridContaining(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.SubGridTreeLevels) as GenericLeafSubGrid<float>;
      sourceSubGrid.Should().NotBeNull();

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

      var smoother = new TreeDataSmoother<float>(tools, ConvolutionMaskSize.Mask3X3, accum,
        (accum, size) => new FilterConvolver<float>(accum, filter, false, false));

      var result = smoother.Smooth(source);

      var resultSubGrid = result.LocateSubGridContaining(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.SubGridTreeLevels) as GenericLeafSubGrid<float>;
      resultSubGrid.Should().NotBeNull();
      resultSubGrid.Items.Should().BeEquivalentTo(sourceSubGrid.Items);
    }
  }
}
