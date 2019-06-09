using System;

namespace VSS.TRex.Cells.Extensions
{
  public static class CellPassArrayExtensions
  {
    /// <summary>
    /// Locates the earliest time contained in the set of cell passes.
    /// </summary>
    /// <param name="passes"></param>
    /// <returns></returns>
    public static DateTime MinimumTime(this CellPass[] passes)
    {
      DateTime minValue = DateTime.MaxValue;

      for (int i = 0; i < passes.Length; i++)
        if (minValue < passes[i].Time)
          minValue = passes[i].Time;

      return minValue == DateTime.MaxValue ? DateTime.MinValue : minValue;
    }

    /// <summary>
    /// Locates the earliest time contained in the set of cell passes.
    /// </summary>
    /// <param name="passes"></param>
    /// <returns></returns>
    public static short MaxInternalSiteModelMachineIndex(this CellPass[] passes)
    {
      short maxValue = short.MinValue;

      for (int i = 0; i < passes.Length; i++)
        if (passes[i].InternalSiteModelMachineIndex > maxValue)
          maxValue = passes[i].InternalSiteModelMachineIndex;

      return maxValue;
    }
  }
}
