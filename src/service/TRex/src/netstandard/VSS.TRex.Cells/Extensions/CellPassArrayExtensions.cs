using System;

namespace VSS.TRex.Cells.Extensions
{
  public static class CellPassArrayExtensions
  {
    /// <summary>
    /// Locates the earliest time contained in the set of cell passes.
    /// </summary>
    /// <param name="passes"></param>
    /// <param name="startIndex"></param>
    /// <param name="endIndex"></param>
    /// <returns></returns>
    public static DateTime MinimumTime(this CellPass[] passes, int startIndex, int endIndex)
    {
      var minValue = DateTime.MaxValue;

      for (int i = startIndex; i <= endIndex; i++)
        if (passes[i].Time < minValue)
          minValue = passes[i].Time;

      return minValue;
    }

    /// <summary>
    /// Locates the earliest time contained in the set of cell passes.
    /// </summary>
    /// <param name="passes"></param>
    /// <param name="startIndex"></param>
    /// <param name="endIndex"></param>
    /// <param name="minTime"></param>
    /// <param name="maxTime"></param>
    /// <returns></returns>
    public static void TimeRange(this CellPass[] passes, int startIndex, int endIndex, out DateTime minTime, out DateTime maxTime)
    {
      minTime = DateTime.MaxValue;
      maxTime = DateTime.MinValue;

      for (int i = startIndex; i <= endIndex; i++)
      {
        if (passes[i].Time < minTime)
          minTime = passes[i].Time;
        if (passes[i].Time > maxTime)
          maxTime = passes[i].Time;
      }
    }

    /// <summary>
    /// Locates the earliest time contained in the set of cell passes.
    /// </summary>
    /// <param name="passes"></param>
    /// <param name="startIndex"></param>
    /// <param name="endIndex"></param>
    /// <returns></returns>
    public static short MaxInternalSiteModelMachineIndex(this CellPass[] passes, int startIndex, int endIndex)
    {
      short maxValue = short.MinValue;

      for (int i = startIndex; i <= endIndex; i++)
        if (passes[i].InternalSiteModelMachineIndex > maxValue)
          maxValue = passes[i].InternalSiteModelMachineIndex;

      return maxValue;
    }
  }
}
