using VSS.VisionLink.Raptor.SubGridTrees.Server.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees.Utilities;

namespace VSS.VisionLink.Raptor.SubGridTrees.Server.Utilities
{
    public static class SegmentTotalPassesCalculator
    {
        /// <summary>
        /// Calculate the total number of passes from all the cells present in the given subgrid segment
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="TotalPasses"></param>
        /// <param name="MaxPassCount"></param>
        public static void CalculateTotalPasses(ISubGridCellSegmentPassesDataWrapper segment, out uint TotalPasses, out uint MaxPassCount)
        {
            uint _TotalPasses = 0;
            uint _MaxPassCount = 0;

            SubGridUtilities.SubGridDimensionalIterator((i, j) =>
            {
                uint ThePassCount = segment.PassCount(i, j);

                if (ThePassCount > _MaxPassCount)
                {
                    _MaxPassCount = ThePassCount;
                }

                _TotalPasses += ThePassCount;
            });

            TotalPasses = _TotalPasses;
            MaxPassCount = _MaxPassCount;
        }
    }
}
