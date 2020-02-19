using System;
using System.Collections.Generic;
using FluentAssertions;
using VSS.TRex.DataSmoothing;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.Types.CellPasses;
using Xunit;

namespace VSS.TRex.Tests.DataSmoothing
{
  public class ConvolutionToolsTests
  {
    [Fact]
    public void Creation()
    {
      var smoother = new ConvolutionTools<float>();
      smoother.Should().NotBeNull();
    }

    [Fact]
    public void FilterConvolverAssertsDimensionsMatch()
    {
      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, ConvolutionMaskSize.Mask3X3);
      var filter = new FilterConvolver<float>(accumulator, new double[3, 3], false, false);
      var smoother = new ConvolutionTools<float>();

      Action act = () => smoother.Convolve(new float[3, 3], new float[4, 4], filter);
      act.Should().Throw<ArgumentException>().WithMessage("Dimensions of source and destination data are not the same");
    }

    [Theory]
    [InlineData(ConvolutionMaskSize.Mask3X3)]
    [InlineData(ConvolutionMaskSize.Mask5X5)]
    public void SingleSubGrid_AtOrigin(ConvolutionMaskSize contextSize)
    {
      const float ELEVATION = 10.0f;

      var tree = DataSmoothingTestUtilities.ConstructSingleSubGridElevationSubGridTreeAtOrigin(ELEVATION);
      var subGrid = tree.LocateSubGridContaining(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.SubGridTreeLevels) as GenericLeafSubGrid<float>;
      subGrid.Should().NotBeNull();

      var result = DataSmoothingTestUtilities.ConstructElevationSubGrid(CellPassConsts.NullHeight);
      result.Should().NotBeNull();

      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, contextSize);
      var filter = new MeanFilter<float>(accumulator, contextSize, false, false);
      var smoother = new ConvolutionTools<float>();
      smoother.Convolve(subGrid, result, filter);

      // All cell values should remain mostly unchanged due to non-null values around perimeter of subgrid in smoothing context
      // Check all acquired values in the single subgrid are the same elevation, except for the perimeter values which 
      // will be 2/3 * Elevation due to null values. Some corner vales will have 0.44444 * ElEVATION for same reason
      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        var ok = Math.Abs(result.Items[x, y] = ELEVATION) < 0.0001 || 
                 Math.Abs(result.Items[x, y] = (2 / 3) * ELEVATION) < 0.0001 ||
                 Math.Abs(result.Items[x, y] = 0.44444f * ELEVATION) < 0.0001;
        ok.Should().BeTrue();
      });
    }

    [Theory]
    [InlineData(ConvolutionMaskSize.Mask3X3)]
    [InlineData(ConvolutionMaskSize.Mask5X5)]
    public void MultipleSubGrids_AtOrigin(ConvolutionMaskSize contextSize)
    {
      const float ELEVATION = 10.0f;

      var subGridMap = new[]
      {
         (-1, 1), (0, 1), (1, 1),
         (-1, 0), (0, 0), (1, 0),
         (-1, -1), (0, -1), (1, -1) 
      };

      var subGrids = new List<GenericLeafSubGrid_Float>();

      var tree = new GenericSubGridTree<float, GenericLeafSubGrid_Float>();

      foreach (var map in subGridMap)
      {
        var subGrid = tree.ConstructPathToCell(
          SubGridTreeConsts.DefaultIndexOriginOffset + map.Item1 * SubGridTreeConsts.SubGridTreeDimension, 
          SubGridTreeConsts.DefaultIndexOriginOffset + map.Item2 * SubGridTreeConsts.SubGridTreeDimension,
          SubGridPathConstructionType.CreateLeaf) as GenericLeafSubGrid_Float;
        DataSmoothingTestUtilities.ConstructElevationSubGrid(subGrid, ELEVATION);

        subGrids.Add(subGrid);
      }

      foreach (var subGrid in subGrids)
      {
        var result = DataSmoothingTestUtilities.ConstructElevationSubGrid(CellPassConsts.NullHeight);
        result.Should().NotBeNull();

        var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, contextSize);
        var filter = new MeanFilter<float>(accumulator, contextSize, false, false);
        var smoother = new ConvolutionTools<float>();
        smoother.Convolve(subGrid, result, filter);

        // All cell values should remain mostly unchanged due to non-null values around perimeter of subgrid in smoothing context
        // Check all acquired values in the single subgrid are the same elevation, except for the perimeter values which 
        // will be 2/3 * Elevation due to null values
        SubGridUtilities.SubGridDimensionalIterator((x, y) =>
        {
          var ok = Math.Abs(result.Items[x, y] = ELEVATION) < 0.0001 || Math.Abs(result.Items[x, y] = (2 / 3) * ELEVATION) < 0.0001;
          ok.Should().BeTrue();
        });
      }
    }

    [Theory]
    [InlineData(ConvolutionMaskSize.Mask3X3, 10.0f, (2/3f) * 10.0f, (1 /9f) * 10.0f, (1/9f) * 10.0f, false)]
    [InlineData(ConvolutionMaskSize.Mask3X3, 10.0f, (2 / 3f) * 10.0f, (1 / 9f) * 10.0f, (1 / 9f) * 10.0f, true)]
    public void SingleSubGrid_SingleSpikeELevation_OriginOfSubGrid(ConvolutionMaskSize contextSize, float elevation, float elevationResult1, float elevationResult2, float elevationResult3, bool updateNullValues)
    {
      var tree = DataSmoothingTestUtilities.ConstructSingleSubGridElevationSubGridTreeAtOrigin(0.0f);
      var subGrid = tree.LocateSubGridContaining(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.SubGridTreeLevels) as GenericLeafSubGrid<float>;
      subGrid.Should().NotBeNull();

      subGrid.Items[0, 0] = elevation;

      var result = DataSmoothingTestUtilities.ConstructElevationSubGrid(CellPassConsts.NullHeight);
      result.Should().NotBeNull();

      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, contextSize);
      var filter = new MeanFilter<float>(accumulator, contextSize, updateNullValues, false);
      var smoother = new ConvolutionTools<float>();
      smoother.Convolve(subGrid, result, filter);

      result.Items[0, 0].Should().Be(elevationResult1);

      result.Items[0, 1].Should().Be(elevationResult2);
      result.Items[1, 1].Should().Be(elevationResult3);
      result.Items[1, 0].Should().Be(elevationResult2);

      for (var i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
      {
        for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
        {
          if (!(i <= (int)contextSize / 2 && j <= (int)contextSize / 2))
            result.Items[i, j].Should().Be(0.0f);
        }
      }
    }

    [Theory]
    [InlineData(ConvolutionMaskSize.Mask3X3, 10.0f, 10.0f / 9.0f)]
    [InlineData(ConvolutionMaskSize.Mask5X5, 10.0f, 10.0f / 25.0f)]
    public void SingleSubGrid_SingleSpikeELevation_CenterOfSubGrid(ConvolutionMaskSize contextSize, float elevation, float elevationResult)
    {
      var tree = DataSmoothingTestUtilities.ConstructSingleSubGridElevationSubGridTreeAtOrigin(0.0f);
      var subGrid = tree.LocateSubGridContaining(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.SubGridTreeLevels) as GenericLeafSubGrid<float>;
      subGrid.Should().NotBeNull();

      subGrid.Items[15, 15] = elevation;

      var result = DataSmoothingTestUtilities.ConstructElevationSubGrid(CellPassConsts.NullHeight);
      result.Should().NotBeNull();

      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, contextSize);
      var filter = new MeanFilter<float>(accumulator, contextSize, false, false);
      var smoother = new ConvolutionTools<float>();
      smoother.Convolve(subGrid, result, filter);

      result.Items[15, 15].Should().Be(elevationResult);

      result.Items[15, 14].Should().Be(elevationResult);
      result.Items[15, 16].Should().Be(elevationResult);
      result.Items[14, 14].Should().Be(elevationResult);
      result.Items[14, 15].Should().Be(elevationResult);
      result.Items[14, 16].Should().Be(elevationResult);
      result.Items[16, 14].Should().Be(elevationResult);
      result.Items[16, 15].Should().Be(elevationResult);
      result.Items[16, 16].Should().Be(elevationResult);

      for (var i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
      {
        for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
        {
          if (!(i >= 15 - (int)contextSize / 2 && i <= 15 + (int)contextSize / 2 && j >= 15 - (int)contextSize / 2 && j <= 15 + (int)contextSize / 2))
            result.Items[i, j].Should().Be(0.0f);
        }
      }
    }

    [Theory]
    [InlineData(ConvolutionMaskSize.Mask3X3, 100)]
    [InlineData(ConvolutionMaskSize.Mask5X5, 100)]
    public void SingleSubGrid_SingleNullELevation_CenterOfSubGrid_NullInfillOnly(ConvolutionMaskSize contextSize, float elevation)
    {
      var tree = DataSmoothingTestUtilities.ConstructSingleSubGridElevationSubGridTreeAtOrigin(elevation);
      var subGrid = tree.LocateSubGridContaining(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.SubGridTreeLevels) as GenericLeafSubGrid<float>;
      subGrid.Should().NotBeNull();

      subGrid.Items[15, 15] = CellPassConsts.NullHeight;

      var result = DataSmoothingTestUtilities.ConstructElevationSubGrid(CellPassConsts.NullHeight);
      result.Should().NotBeNull();

      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, contextSize);
      var filter = new MeanFilter<float>(accumulator, contextSize, true, true);
      var smoother = new ConvolutionTools<float>();
      smoother.Convolve(subGrid, result, filter);

      result.Items[15, 15].Should().Be(elevation);

      SubGridUtilities.SubGridDimensionalIterator((x, y) => result.Items[x, y].Should().Be(elevation));
    }

    [Theory]
    [InlineData(ConvolutionMaskSize.Mask3X3, 1, 1)]
    [InlineData(ConvolutionMaskSize.Mask3X3, 10, 1)]
    [InlineData(ConvolutionMaskSize.Mask3X3, 1, 10)]
    [InlineData(ConvolutionMaskSize.Mask3X3, 100, 50)]
    [InlineData(ConvolutionMaskSize.Mask3X3, 50, 100)]
    [InlineData(ConvolutionMaskSize.Mask5X5, 100, 100)]
    public void ArrayConvolver(ConvolutionMaskSize contextSize, int width, int height)
    {
      const float ELEVATION = 10.0f;

      var sourceArray = new float[width, height];
      for (var i = 0; i < sourceArray.GetLength(0); i++)
      {
        for (var j = 0; j < sourceArray.GetLength(1); j++)
        {
          sourceArray[i, j] = 100.0f;
        }
      }

      var destArray = new float[width, height];

      var accumulator = new ConvolutionAccumulator_Float(CellPassConsts.NullHeight, contextSize);
      var filter = new MeanFilter<float>(accumulator, contextSize, false, false);
      var smoother = new ConvolutionTools<float>();
      smoother.Convolve(sourceArray, destArray, filter);

      // All cell values should remain mostly unchanged due to non-null values around perimeter of array in smoothing context
      // Check all acquired values in the single subgrid are the same elevation, except for the perimeter values which 
      // will be 2/3 * Elevation due to null values. Some corner vales will have 0.44444 * ElEVATION for same reason
      for (var x = 0; x < destArray.GetLength(0); x++)
      {
        for (var y = 0; y < destArray.GetLength(1); y++)
        {
          var ok = Math.Abs(destArray[x, y] = ELEVATION) < 0.0001 ||
                   Math.Abs(destArray[x, y] = (2 / 3) * ELEVATION) < 0.0001 ||
                   Math.Abs(destArray[x, y] = 0.44444f * ELEVATION) < 0.0001;
          ok.Should().BeTrue();
        }
      }
    }
  }
}
