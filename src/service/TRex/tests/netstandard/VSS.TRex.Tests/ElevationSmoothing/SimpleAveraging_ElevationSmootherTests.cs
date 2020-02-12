﻿using System.Collections.Generic;
using FluentAssertions;
using VSS.TRex.ElevationSmoothing;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex.Types.CellPasses;
using Xunit;

namespace VSS.TRex.Tests.ElevationSmoothing
{
  public class SimpleAveraging_ElevationSmootherTests
  {
    private GenericLeafSubGrid_Float ConstructElevationSubGrid(float elevation)
    {
      var subGrid = new GenericLeafSubGrid_Float
      {
        Level = SubGridTreeConsts.SubGridTreeLevels
      };
      subGrid.ForEach((x, y) => subGrid.Items[x, y] = elevation);

      return subGrid;
    }

    private void ConstructElevationSubGrid(GenericLeafSubGrid_Float subGrid, float elevation)
    {
      subGrid.ForEach((x, y) => subGrid.Items[x, y] = elevation);
    }

    private GenericSubGridTree<float, GenericLeafSubGrid_Float> ConstructSingleSubGridElevationSubGridTreeAtOrigin(float elevation)
    {
      var tree = new GenericSubGridTree<float, GenericLeafSubGrid_Float>();

      var subGrid = tree.ConstructPathToCell(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset,
        SubGridPathConstructionType.CreateLeaf) as GenericLeafSubGrid_Float;
      ConstructElevationSubGrid(subGrid, elevation);

      return tree;
    }

    [Fact]
    public void Creation()
    {
      var smoother = new SimpleAveraging_ElevationSmoother();
      smoother.Should().NotBeNull();
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    public void SingleSubGrid_AtOrigin_ContextOf3(int contextSize)
    {
      const float ELEVATION = 10.0f;

      var tree = ConstructSingleSubGridElevationSubGridTreeAtOrigin(ELEVATION);
      var subGrid = tree.LocateSubGridContaining(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.SubGridTreeLevels) as GenericLeafSubGrid_Float;
      subGrid.Should().NotBeNull();

      var result = ConstructElevationSubGrid(CellPassConsts.NullHeight);
      result.Should().NotBeNull();

      var smoother = new SimpleAveraging_ElevationSmoother();
      smoother.SmoothLeaf(subGrid, result, contextSize);

      // All cell values should remain unchanged due to null values around perimeter of subgrid in smoothing context
      // Check all acquired values in the single subgrid are zero
      for (var i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
      {
        for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
        {
          result.Items[i, j].Should().Be(ELEVATION);
        }
      }
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    public void MultipleSubGrids_AtOrigin(int contextSize)
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
        ConstructElevationSubGrid(subGrid, ELEVATION);

        subGrids.Add(subGrid);
      }

      foreach (var subGrid in subGrids)
      {
        var result = ConstructElevationSubGrid(CellPassConsts.NullHeight);
        result.Should().NotBeNull();

        var smoother = new SimpleAveraging_ElevationSmoother();
        smoother.SmoothLeaf(subGrid, result, contextSize);

        // All cell values should remain unchanged due to null values around perimeter of subgrid in smoothing context
        // Check all acquired values in the single subgrid are zero
        for (var i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
        {
          for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
          {
            result.Items[i, j].Should().Be(ELEVATION);
          }
        }
      }
    }

    [Theory]
    [InlineData(3, 10.0f, 10.0f / 4.0f, 10.0f / 6.0f, 10.0f / 9.0f)]
    public void SingleSubGrid_SingleSpikeELevation_OriginOfSubGrid(int contextSize, float elevation, float elevationResult1, float elevationResult2, float elevationResult3)
    {
      var tree = ConstructSingleSubGridElevationSubGridTreeAtOrigin(0.0f);
      var subGrid = tree.LocateSubGridContaining(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.SubGridTreeLevels) as GenericLeafSubGrid_Float;
      subGrid.Should().NotBeNull();

      subGrid.Items[0, 0] = elevation;

      var result = ConstructElevationSubGrid(CellPassConsts.NullHeight);
      result.Should().NotBeNull();

      var smoother = new SimpleAveraging_ElevationSmoother();
      smoother.SmoothLeaf(subGrid, result, contextSize);

      result.Items[0, 0].Should().Be(elevationResult1);

      result.Items[0, 1].Should().Be(elevationResult2);
      result.Items[1, 1].Should().Be(elevationResult3);
      result.Items[1, 0].Should().Be(elevationResult2);

      for (var i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
      {
        for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
        {
          if (!(i <= contextSize / 2 && j <= contextSize / 2))
            result.Items[i, j].Should().Be(0.0f);
        }
      }
    }

    [Theory]
    [InlineData(3, 10.0f, 10.0f / 9.0f)]
    [InlineData(5, 10.0f, 10.0f / 25.0f)]
    public void SingleSubGrid_SingleSpikeELevation_CenterOfSubGrid(int contextSize, float elevation, float elevationResult)
    {
      var tree = ConstructSingleSubGridElevationSubGridTreeAtOrigin(0.0f);
      var subGrid = tree.LocateSubGridContaining(SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.SubGridTreeLevels) as GenericLeafSubGrid_Float;
      subGrid.Should().NotBeNull();

      subGrid.Items[15, 15] = elevation;

      var result = ConstructElevationSubGrid(CellPassConsts.NullHeight);
      result.Should().NotBeNull();

      var smoother = new SimpleAveraging_ElevationSmoother();
      smoother.SmoothLeaf(subGrid, result, contextSize);

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
          if (!(i >= 15 - contextSize / 2 && i <= 15 + contextSize / 2 && j >= 15 - contextSize / 2 && j <= 15 + contextSize / 2))
            result.Items[i, j].Should().Be(0.0f);
        }
      }
    }
  }
}
