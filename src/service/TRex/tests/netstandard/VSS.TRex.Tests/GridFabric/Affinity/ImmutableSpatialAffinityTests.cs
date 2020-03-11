using System;
using System.Collections.Generic;
using FluentAssertions;
using VSS.TRex.GridFabric.Affinity;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.GridFabric.Affinity
{
  public class ImmutableSpatialAffinityTests : IClassFixture<DILoggingFixture>
  {
    [Fact]
    public void Creation()
    {
      var aff = new SubGridBasedSpatialAffinityFunction();
      aff.Should().NotBeNull();
    }

    [Fact]
    public void CalculateSinglePartition_AtGridAbsoluteOrigin()
    {
      var aff = new SubGridBasedSpatialAffinityFunction();

      var partition = aff.GetPartition(new SubGridSpatialAffinityKey(0, Guid.NewGuid(), 0, 0));
      partition.Should().Be(0);
    }

    [Fact]
    public void CalculateSinglePartition_AtGridFalseOrigin()
    {
      var aff = new SubGridBasedSpatialAffinityFunction();

      var partition = aff.GetPartition(new SubGridSpatialAffinityKey(0, Guid.NewGuid(), SubGridTreeConsts.DefaultIndexOriginOffset, SubGridTreeConsts.DefaultIndexOriginOffset));
      partition.Should().Be(0);
    }

    [Fact]
    public void CalculatePartitions_32x32SubGridPatch_AtGridAbsoluteOrigin()
    {
      var aff = new SubGridBasedSpatialAffinityFunction();

      var partitions = new List<int>();

      for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
      {
        for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
        {
          partitions.Add(aff.GetPartition(new SubGridSpatialAffinityKey(0, Guid.NewGuid(),
            i * SubGridTreeConsts.SubGridTreeDimension, 
            j * SubGridTreeConsts.SubGridTreeDimension)));
        }
      }

      partitions.Count.Should().Be(SubGridTreeConsts.SubGridTreeCellsPerSubGrid);

      var hashSet = new HashSet<int>();
      partitions.ForEach(x => hashSet.Add(x));

      hashSet.Count.Should().Be(SubGridTreeConsts.SubGridTreeCellsPerSubGrid);
    }
  }
}
