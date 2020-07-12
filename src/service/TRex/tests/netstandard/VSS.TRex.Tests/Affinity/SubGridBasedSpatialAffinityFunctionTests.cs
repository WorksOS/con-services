using System;
using FluentAssertions;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.Affinity
{
  public class SubGridBasedSpatialAffinityFunctionTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Creation()
    {
      var func = new SubGridBasedSpatialAffinityFunction();
      func.Should().NotBeNull();
    }

    [Fact]
    public void OriginLocationGivesPartitionZero()
    {
      var func = new SubGridBasedSpatialAffinityFunction();

      var partition = func.GetPartition(new SubGridSpatialAffinityKey(0, Guid.NewGuid(), new SubGridCellAddress(0, 0)));
      partition.Should().Be(0);
    }

    [Fact]
    public void AllCellsInOriginSubGridGivePartitionZero()
    {
      var func = new SubGridBasedSpatialAffinityFunction();

      SubGridUtilities.SubGridDimensionalIterator((x, y) =>
      {
        var partition = func.GetPartition(new SubGridSpatialAffinityKey(0, Guid.NewGuid(), new SubGridCellAddress(x, y)));
        partition.Should().Be(0);
      });
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(SubGridTreeConsts.SubGridTreeDimension, 0, 32)]
    [InlineData(0, SubGridTreeConsts.SubGridTreeDimension, 1)]
    [InlineData(SubGridTreeConsts.SubGridTreeDimension, SubGridTreeConsts.SubGridTreeDimension, 33)]
    public void SubGridsAboutOriginGiveCorrectPartition(int x, int y, int expectedPartition)
    {
      var func = new SubGridBasedSpatialAffinityFunction();

      var partition = func.GetPartition(new SubGridSpatialAffinityKey(0, Guid.NewGuid(), new SubGridCellAddress(x, y)));
      partition.Should().Be(expectedPartition);
    }
  }
}
