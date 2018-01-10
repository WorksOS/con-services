using System;

namespace TestUtility.Model.WebApi
{
  /// <summary>
  ///Total, target and average cycle counts.
  /// </summary>
  public class CycleSummaryData
  {
    /// <summary>
    /// Total cycle count.
    /// </summary>
    public int? totalCycleCount { get; set; }

    /// <summary>
    /// Target cycle count.
    /// </summary>
    public int? targetCycleCount { get; set; }

    /// <summary>
    /// Average cycle count.
    /// </summary>
    public double? averageCycleCount { get; set; }


    public override bool Equals(object obj)
    {
      var actual = obj as CycleSummaryData;

      if (totalCycleCount != null)
      {
        if (totalCycleCount != actual.totalCycleCount)
          return false;
      }


      if (targetCycleCount != null)
      {
        if (targetCycleCount != actual.targetCycleCount)
          return false;
      }

      if (averageCycleCount != null)
      {
        if (Math.Abs(Math.Round((double) averageCycleCount, 2) - Math.Round((double) actual.averageCycleCount, 2)) >
            0.0001)
          return false;
      }
      return true;
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }

}
