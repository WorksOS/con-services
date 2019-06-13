using VSS.TRex.SubGridTrees.Interfaces;
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
    /// <param name="MinPassCount"></param>
    /// <param name="MaxPassCount"></param>
    public static void CalculateTotalPasses(ISubGridCellSegmentPassesDataWrapper segment, out int TotalPasses,
         out int MinPassCount, out int MaxPassCount)
      {
        TotalPasses = 0;
        MaxPassCount = 0;
        MinPassCount = int.MaxValue;

        // Todo: Push this down to the segment to avoid the PassCount abstraction
        if (segment.HasPassData())
        {
          for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
          {
            for (int j = 0; j < SubGridTreeConsts.SubGridTreeDimension; j++)
            {
              int ThePassCount = segment.PassCount(i, j);

              if (ThePassCount > MaxPassCount)
              {
                MaxPassCount = ThePassCount;
              }

              if (ThePassCount < MinPassCount)
              {
                MinPassCount = ThePassCount;
              }

              TotalPasses += ThePassCount;
            }
          }
        }

      }
    }
}
