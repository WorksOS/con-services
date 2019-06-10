using System;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Utilities
{
  public static class SegmentCellPassAdopter
  {
    /// <summary>
    /// Causes segment to adopt all cell passes from sourceSegment where those cell passes were 
    /// recorded at or later than a specific date
    /// </summary>
    /// <param name="segment"></param>
    /// <param name="sourceSegment"></param>
    /// <param name="atAndAfterTime"></param>
    public static void AdoptCellPassesFrom(ISubGridCellSegmentPassesDataWrapper segment,
      ISubGridCellSegmentPassesDataWrapper sourceSegment,
      DateTime atAndAfterTime)
    {
      Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((i, j) =>
      {
        var sourceSegmentPasses = sourceSegment.ExtractCellPasses(i, j, out int sourcePassCount);

        if (sourcePassCount == 0)
          return;

        int countInCell = 0;

        for (int PassIndex = 0; PassIndex < sourcePassCount; PassIndex++)
        {
          if (sourceSegmentPasses[PassIndex].Time < atAndAfterTime)
            countInCell++;
          else
            break; // No more passes in the cell will satisfy the condition
        }

        // countInCell represents the number of cells that should remain in the source segment.
        // The remainder are to be moved to this segment

        var adoptedPassCount = sourcePassCount - countInCell;
        if (adoptedPassCount > 0)
        {
          segment.Integrate(i, j, sourceSegmentPasses, sourcePassCount, countInCell, sourcePassCount - 1, out int AddedCount, out _);
          segment.SegmentPassCount += AddedCount;

          sourceSegment.SegmentPassCount -= adoptedPassCount;

          // Set the new number of passes using AllocatePasses(). This will reduce the tracked pass count without incurring the
          // overhead of resizing the array. Use AllocatePassesExact() if this behaviour is required.
          sourceSegment.AllocatePasses(i, j, countInCell);
        }
      });
    }
  }
}
