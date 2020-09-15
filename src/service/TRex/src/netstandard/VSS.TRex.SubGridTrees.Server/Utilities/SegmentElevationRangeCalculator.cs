using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Utilities
{
    public static class SegmentElevationRangeCalculator
    {
        /// <summary>
        /// Calculates the elevation range that all the passes within the given segment span.
        /// </summary>
        /// <returns>True if the result of the calculation yields a valid elevation range</returns>
        public static bool CalculateElevationRangeOfPasses(ISubGridCellSegmentPassesDataWrapper segment,
            out double minElevation, out double maxElevation)
        {
            minElevation = 1E10;
            maxElevation = -1E10;

            var min = minElevation;
            var max = maxElevation;

            for (var i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
            {
              for (var j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
              {
                var passCount = segment.PassCount(i, j);

                for (var passIndex = 0; passIndex < passCount; passIndex++)
                {
                  // Todo: Delegate this down to the segment to avoid the PassHeight() abstraction
                  var height = segment.PassHeight(i, j, passIndex);

                  if (height > max)
                    max = height;

                  if (height < min)
                    min = height;
                }
              }
            }

            if (min <= max)
            {
                minElevation = min;
                maxElevation = max;
            }

            return minElevation <= maxElevation;
        }
    }
}
