using VSS.TRex.SubGridTrees.Server.Iterators;
using System;
using VSS.TRex.Storage.Models;
using Xunit;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Core;
using VSS.TRex.SubGridTrees.Factories;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.Tests.TestFixtures;

namespace VSS.TRex.Tests.SubGridTrees
{
  public class SubGridSegmentCellPassIterator_NonStaticTests : IClassFixture<DILoggingFixture>
  {
    [Fact()]
    public void Test_SubGridSegmentCellPassIterator_NonStaticTests_Creation()
    {
      SubGridSegmentCellPassIterator_NonStatic iterator = new SubGridSegmentCellPassIterator_NonStatic();

      Assert.True(iterator.CellX == 0 && iterator.CellY == 0, "CellX/Y not initialised correctly");
      Assert.Equal(iterator.MaxNumberOfPassesToReturn, int.MaxValue);
      Assert.Null(iterator.SegmentIterator);
    }

    [Fact()]
    public void Test_SubGridSegmentCellPassIterator_NonStaticTests_SetCellCoordinatesInSubgrid()
    {
      SubGridSegmentCellPassIterator_NonStatic iterator = new SubGridSegmentCellPassIterator_NonStatic();

      iterator.SetCellCoordinatesInSubGrid(12, 23);

      Assert.True(iterator.CellX == 12 && iterator.CellY == 23, "CellX/Y not set correctly");
    }

    [Fact()]
    public void Test_SubGridSegmentCellPassIterator_NonStaticTests_SetIteratorElevationRange()
    {
      var tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
      var iterator = new SubGridSegmentCellPassIterator_NonStatic(new SubGridSegmentIterator(new ServerSubGridTreeLeaf(tree, null, SubGridTreeConsts.SubGridTreeLevels, StorageMutability.Mutable), new SubGridDirectory(), null));

      iterator.SetIteratorElevationRange(12.0, 23.0);

      Assert.True(iterator.SegmentIterator.IterationState.MinIterationElevation == 12.0 &&
                  iterator.SegmentIterator.IterationState.MaxIterationElevation == 23.0, "CellX/Y not set correctly");
    }

    [Fact()]
    public void Test_SubGridSegmentCellPassIterator_NonStaticTests_Initialise()
    {
      var tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
      var iterator = new SubGridSegmentCellPassIterator_NonStatic(new SubGridSegmentIterator(new ServerSubGridTreeLeaf(tree, null, SubGridTreeConsts.SubGridTreeLevels, StorageMutability.Mutable), new SubGridDirectory(), null));

      iterator.SegmentIterator.IterationDirection = IterationDirection.Forwards;
      iterator.Initialise();

      iterator.SegmentIterator.IterationDirection = IterationDirection.Backwards;
      iterator.Initialise();
    }

    [Fact()]
    public void Test_SubGridSegmentCellPassIterator_NonStaticTests_SetTimeRangeTest()
    {
      var tree = new SubGridTree(SubGridTreeConsts.SubGridTreeLevels, 1, new SubGridFactory<NodeSubGrid, ServerSubGridTreeLeaf>());
      var iterator = new SubGridSegmentCellPassIterator_NonStatic(new SubGridSegmentIterator(new ServerSubGridTreeLeaf(tree, null, SubGridTreeConsts.SubGridTreeLevels, StorageMutability.Mutable), new SubGridDirectory(), null));

      iterator.SetTimeRange(true, DateTime.SpecifyKind(new DateTime(2000, 1, 1), DateTimeKind.Utc), DateTime.SpecifyKind(new DateTime(2000, 1, 2),DateTimeKind.Utc));

      Assert.True(iterator.IteratorStartTime == DateTime.SpecifyKind(new DateTime(2000, 1, 1), DateTimeKind.Utc) 
                  && iterator.IteratorEndTime == DateTime.SpecifyKind(new DateTime(2000, 1, 2), DateTimeKind.Utc),
        "Iteration start and end date not set correctly");
    }
  }
}
