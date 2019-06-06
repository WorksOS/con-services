using System;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Utilities
{
    /// <summary>
    /// Supports calculating elevation ranges across cell passes
    /// </summary>
    public static class SegmentTimeRangeCalculator
    {
        /// <summary>
        /// Calculates the time range covering all the cell passes within the given sub grid segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public static void CalculateTimeRange(ISubGridCellSegmentPassesDataWrapper segment, out DateTime startTime, out DateTime endTime)
        {
            startTime = Consts.MAX_DATETIME_AS_UTC;
            endTime = Consts.MIN_DATETIME_AS_UTC;

            DateTime _startTime = startTime;
            DateTime _endTime = endTime;

            Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((i, j) =>
            {
                int ThePassCount = segment.PassCount(i, j);

                if (ThePassCount == 0)
                    return;

                for (int PassIndex = 0; PassIndex < ThePassCount; PassIndex++)
                {
                    DateTime theTime = segment.PassTime(i, j, PassIndex);

                    if (theTime > _endTime)
                        _endTime = theTime;

                    if (theTime < _startTime)
                        _startTime = theTime;
                }
            });

            startTime = _startTime;
            endTime = _endTime;
        }

        /// <summary>
        /// Calculates the number of passes in the segment that occur before searchTime
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="searchTime"></param>
        /// <param name="totalPasses"></param>
        /// <param name="maxPassCount"></param>
        public static void CalculatePassesBeforeTime(ISubGridCellSegmentPassesDataWrapper segment, DateTime searchTime,
            out int totalPasses, out int maxPassCount)
        {
            totalPasses = 0;
            maxPassCount = 0;

            int _totalPasses = totalPasses;
            int _maxPassCount = maxPassCount;

          Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((i, j) =>
            {
                int thePassCount = segment.PassCount(i, j);

                if (thePassCount == 0)
                    return;

                int countInCell = 0;

                for (int PassIndex = 0; PassIndex < thePassCount; PassIndex++)
                {
                    DateTime theTime = segment.PassTime(i, j, PassIndex);

                    if (theTime < searchTime)
                        countInCell++;
                }

                _totalPasses += countInCell;

                if (countInCell > _maxPassCount)
                    _maxPassCount = countInCell;
            });

            totalPasses = _totalPasses;
            maxPassCount = _maxPassCount;
        }
    }
}
