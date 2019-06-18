using System;

namespace VSS.TRex.Cells.Extensions
{
  public static class CellPassArrayExtensions
  {
    /// <summary>
    /// Locates the earliest time contained in the set of cell passes.
    /// </summary>
    /// <param name="passes"></param>
    /// <param name="arrayLength"></param>
    /// <returns></returns>
    public static DateTime MinimumTime(this CellPass[] passes, int arrayLength)
    {
      var minValue = DateTime.MaxValue;

      for (int i = 0; i < arrayLength; i++)
        if (passes[i].Time < minValue)
          minValue = passes[i].Time;

      return minValue;
    }

    /// <summary>
    /// Locates the earliest time contained in the set of cell passes.
    /// </summary>
    /// <param name="passes"></param>
    /// <param name="arrayLength"></param>
    /// <param name="minTime"></param>
    /// <param name="maxTime"></param>
    /// <returns></returns>
    public static void TimeRange(this CellPass[] passes, int arrayLength, out DateTime minTime, out DateTime maxTime)
    {
      minTime = DateTime.MaxValue;
      maxTime = DateTime.MinValue;

      for (int i = 0; i < arrayLength; i++)
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
    /// <param name="arrayLength"></param>
    /// <returns></returns>
    public static short MaxInternalSiteModelMachineIndex(this CellPass[] passes, int arrayLength)
    {
      short maxValue = short.MinValue;

      for (int i = 0; i < arrayLength; i++)
        if (passes[i].InternalSiteModelMachineIndex > maxValue)
          maxValue = passes[i].InternalSiteModelMachineIndex;

      return maxValue;
    }
  }
}
