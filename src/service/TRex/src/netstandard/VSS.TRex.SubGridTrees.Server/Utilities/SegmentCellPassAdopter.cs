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
        var thePassCount = (uint) sourceSegmentPasses.Length;

        if (thePassCount == 0)
          return;

        uint countInCell = 0;

        for (uint PassIndex = 0; PassIndex < thePassCount; PassIndex++)
        {
          if (sourceSegmentPasses[PassIndex].Time < atAndAfterTime)
            countInCell++;
          else
            break; // No more passes in the cell will satisfy the condition
        }

        // countInCell represents the number of cells that should remain in the source segment.
        // The remainder are to be moved to this segment

        var adoptedPassCount = thePassCount - countInCell;
        if (adoptedPassCount > 0)
        {
          segment.Integrate(i, j, sourceSegmentPasses, countInCell, thePassCount - 1, out uint AddedCount, out _);
          segment.SegmentPassCount += AddedCount;

          // Set the new number of passes and reset the length of the cell passes in the cell the passes were adopted from
          sourceSegment.SegmentPassCount -= adoptedPassCount;
          sourceSegment.AllocatePasses(i, j, countInCell);
        }
      });
    }
  }
}
