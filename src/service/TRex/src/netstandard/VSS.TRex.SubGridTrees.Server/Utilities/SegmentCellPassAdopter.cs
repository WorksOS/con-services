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
        var sourceSegmentPasses = sourceSegment.ExtractCellPasses(i, j);
        var passes = sourceSegmentPasses.Passes;

        if (passes.Count == 0)
          return;

        int countInCell = 0;
        var elements = passes.Elements;

        for (int cpi = passes.Offset, limit = passes.OffsetPlusCount; cpi < limit; cpi++)
        {
          if (elements[cpi].Time < atAndAfterTime)
            countInCell++;
          else
            break; // No more passes in the cell will satisfy the condition
        }

        // countInCell represents the number of cells that should remain in the source segment.
        // The remainder are to be moved to this segment

        var adoptedPassCount = passes.Count - countInCell;
        if (adoptedPassCount > 0)
        {
          segment.Integrate(i, j, sourceSegmentPasses, countInCell, passes.Count - 1, out int AddedCount, out _);
          segment.SegmentPassCount += AddedCount;

          sourceSegment.SegmentPassCount -= adoptedPassCount;
          sourceSegment.TrimPassCount(i, j, countInCell);
        }
      });
    }
  }
}
