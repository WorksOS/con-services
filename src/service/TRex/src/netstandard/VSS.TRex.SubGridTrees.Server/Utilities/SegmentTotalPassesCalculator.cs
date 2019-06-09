using VSS.TRex.SubGridTrees.Server.Interfaces;

namespace VSS.TRex.SubGridTrees.Server.Utilities
{
    public static class SegmentTotalPassesCalculator
    {
        /// <summary>
        /// Calculate the total number of passes from all the cells present in the given sub grid segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="TotalPasses"></param>
        /// <param name="MaxPassCount"></param>
        public static void CalculateTotalPasses(ISubGridCellSegmentPassesDataWrapper segment, out int TotalPasses, out int MaxPassCount)
        {
            int _TotalPasses = 0;
            int _MaxPassCount = 0;

            if (segment.HasPassData())
            {
                Core.Utilities.SubGridUtilities.SubGridDimensionalIterator((i, j) =>
                {
                    int ThePassCount = segment.PassCount(i, j);
                 
                    if (ThePassCount > _MaxPassCount)
                    {
                      _MaxPassCount = ThePassCount;
                    }
                 
                    _TotalPasses += ThePassCount;
                });
            }

            TotalPasses = _TotalPasses;
            MaxPassCount = _MaxPassCount;
        }
    }
}
