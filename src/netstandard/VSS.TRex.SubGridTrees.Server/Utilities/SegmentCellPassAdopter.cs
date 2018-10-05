using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Utilities
{
    public static class SegmentCellPassAdopter
    {
        private static readonly ILogger Log = Logging.Logger.CreateLogger("SegmentCellPassAdopter");

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
                int thePassCount = (int)sourceSegment.PassCount(i, j);

                if (thePassCount == 0)
                    return;

                int countInCell = 0;

                for (uint PassIndex = 0; PassIndex < thePassCount; PassIndex++)
                {
                    if (sourceSegment.PassTime(i, j, PassIndex) < atAndAfterTime)
                        countInCell++;
                    else
                        break; // No more passes in the cell will satisfy the condition
                }

                // countInCell represents the number of cells that should remain in the source segment.
                // The remainder are to be moved to this segment

                int adoptedPassCount = thePassCount - countInCell;
                if (adoptedPassCount > 0)
                {
                    // Copy the adopted passes from the 'from' cell to the 'to' cell
                    for (int PassIndex = countInCell; PassIndex < thePassCount; PassIndex++)
                    {
                      if (sourceSegment.Pass(i, j, (uint)PassIndex).Time < atAndAfterTime)
                      {
                        string msg = $"Pass with inappropriate time being added to segment: {sourceSegment.Pass(i, j, (uint)PassIndex).Time} < {atAndAfterTime}";
                        Log.LogInformation(msg);
                        //Debug.Assert(false, "Pass with inappropriate time being added to segment");
                      }
                      segment.AddPass(i, j, sourceSegment.Pass(i, j, (uint)PassIndex));
                    }

                  // Set the new number of passes and reset the length of the cell passes
                  // in the cell the passes were adopted from
                  sourceSegment.SegmentPassCount -= adoptedPassCount;
                  sourceSegment.AllocatePasses(i, j, (uint)countInCell);
                }
            });
        }
    }
}
