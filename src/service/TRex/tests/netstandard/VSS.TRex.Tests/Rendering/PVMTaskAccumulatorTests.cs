using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using VSS.TRex.Rendering.Executors.Tasks;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.SubGridTrees;
using Xunit;

namespace VSS.TRex.Tests.Rendering
{
  public class PVMTaskAccumulatorTests
  {
    private ClientLeafSubGrid NewClientSubGrid()
    {
      var subGrid = new ClientHeightLeafSubGrid();
      subGrid.OriginX = SubGridTreeConsts.DefaultIndexOriginOffset;
      subGrid.OriginY = SubGridTreeConsts.DefaultIndexOriginOffset;
      subGrid.FillWithTestPattern();

      return subGrid;
    }

    [Fact]
    public void Creation()
    {
      var accum = new PVMTaskAccumulator(1, 1, 1, 0, 0, 0, 0, 0);

      accum.Should().NotBeNull();
    }

    [Fact]
    public void FailWithNullSubGridArray()
    {
      var accum = new PVMTaskAccumulator(SubGridTreeConsts.DefaultCellSize, SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension, 0, 0, 0, 0, 0);
      accum.Transcribe(null).Should().Be(false);
    }

    [Fact]
    public void FailWithNullSubGrid()
    {
      var accum = new PVMTaskAccumulator(SubGridTreeConsts.DefaultCellSize, SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension, 0, 0, 0, 0, 0);
      accum.Transcribe(new IClientLeafSubGrid[] { null }).Should().Be(false);
    }

    [Fact]
    public void FailWithTooManySubGrids()
    {
      var accum = new PVMTaskAccumulator(SubGridTreeConsts.DefaultCellSize, SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension, 0, 0, 0, 0, 0);
      accum.Transcribe(new IClientLeafSubGrid[] { null, null }).Should().Be(false);
    }

    [Fact]
    public void AllCellsAssignment_AtOrigin()
    {
      var accum = new PVMTaskAccumulator(SubGridTreeConsts.DefaultCellSize,
        SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension,
        SubGridTreeConsts.SubGridTreeDimension * SubGridTreeConsts.DefaultCellSize, SubGridTreeConsts.SubGridTreeDimension * SubGridTreeConsts.DefaultCellSize, 
        0, 0, 0);
      var subGrid = NewClientSubGrid();

      accum.Transcribe(new IClientLeafSubGrid[] {subGrid}).Should().Be(true);

      SubGridUtilities.SubGridDimensionalIterator((x, y) => Math.Abs(accum.ValueStore[x, y] - (x + y)).Should().BeLessThan(0.01f));
    }

    [Fact]
    public void SkippedCellsAssignment_AtOrigin()
    {
      var accum = new PVMTaskAccumulator(SubGridTreeConsts.DefaultCellSize, SubGridTreeConsts.SubGridTreeDimension / 2, SubGridTreeConsts.SubGridTreeDimension / 2,
        SubGridTreeConsts.SubGridTreeDimension * SubGridTreeConsts.DefaultCellSize, SubGridTreeConsts.SubGridTreeDimension * SubGridTreeConsts.DefaultCellSize,
        0, 0, 0);
      var subGrid = NewClientSubGrid();

      accum.Transcribe(new IClientLeafSubGrid[] { subGrid }).Should().Be(true);

      for (var i = 0; i < 15; i++)
      for (var j = 0; j < 15; j++)
        (accum.ValueStore[i, j] - ((i + j + 1) * 2)).Should().BeLessThan(0.01f);
    }
  }
}
