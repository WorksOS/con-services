using FluentAssertions;
using VSS.TRex.DataSmoothing;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.Types.CellPasses;
using Xunit;

namespace VSS.TRex.Tests.DataSmoothing
{
  public class TreeDataSmootherTests
  {
    private void ConstructElevationSubGrid(GenericLeafSubGrid<float> subGrid, float elevation)
    {
      subGrid.ForEach((x, y) => subGrid.Items[x, y] = elevation);
    }

    private GenericSubGridTree<float, GenericLeafSubGrid<float>> ConstructSingleSubGridElevationSubGridTreeAtOrigin(float elevation)
    {
      var tree = new GenericSubGridTree<float, GenericLeafSubGrid<float>>();

      var subGrid = tree.ConstructPathToCell(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset,
        SubGridPathConstructionType.CreateLeaf) as GenericLeafSubGrid<float>;
      ConstructElevationSubGrid(subGrid, elevation);

      return tree;
    }

    [Fact]
    public void Creation()
    {
      var source = new GenericSubGridTree<float, GenericLeafSubGrid<float>>();

      var tools = new ConvolutionTools<float>();
      var accum = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight);
      var filter = new double[3, 3];

      var smoother = new TreeDataSmoother<float>(source, tools, 3, accum,
        (acc, size) => new FilterConvolver<float>(accum, filter, false, false));

      smoother.Should().NotBeNull();
    }

    [Fact]
    public void Smooth()
    {
      const float elevation = 10.0f;
      const double oneNinth = 1d / 9d;

      var source = ConstructSingleSubGridElevationSubGridTreeAtOrigin(elevation);
      var sourceSubGrid = source.LocateSubGridContaining(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.SubGridTreeLevels) as GenericLeafSubGrid<float>;
      sourceSubGrid.Should().NotBeNull();

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

      var smoother = new TreeDataSmoother<float>(source, tools, 3, accum,
        (accum, size) => new FilterConvolver<float>(accum, filter, false, false));

      var result = smoother.Smooth();

      var resultSubGrid = result.LocateSubGridContaining(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.SubGridTreeLevels) as GenericLeafSubGrid<float>;
      resultSubGrid.Should().NotBeNull();
      resultSubGrid.Items.Should().BeEquivalentTo(sourceSubGrid.Items);
    }
  }
}
