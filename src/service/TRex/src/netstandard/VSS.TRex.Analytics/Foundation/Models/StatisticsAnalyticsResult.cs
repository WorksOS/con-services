using System;
using System.Linq;

namespace VSS.TRex.Analytics.Foundation.Models
{
  /// <summary>
  /// Base class for statistics results sent to client calling contexts for analytics functions
  /// </summary>
  public class StatisticsAnalyticsResult : AnalyticsResult
  {
    private long[] counts;

    /// <summary>
    /// The percentage of the cells that are below the target data range
    /// </summary>
    public double BelowTargetPercent { get; set; }

    /// <summary>
    /// The percentage of cells that are within the target data range
    /// </summary>
    public double WithinTargetPercent { get; set; }

    /// <summary>
    /// The percentage of the cells that are above the target data range
    /// </summary>
    public double AboveTargetPercent { get; set; }

    /// <summary>
    /// The total area covered by non-null cells in the request area
    /// </summary>
    public double TotalAreaCoveredSqMeters { get; set; }

    /// <summary>
    /// An array (or always 7) values represnting the counts of cells within each of the cut fill bands defined in the request.
    /// </summary>
    public long[] Counts
    {
      get
      {
        return counts;
      }
      set
      {
        SetCounts(value);
      }
    }

    /// <summary>
    /// An array (or always 7) values represnting the percentages of cells within each of the cut fill bands defined in the request.
    /// </summary>
    public double[] Percents { get; set; }

    /// <summary>
    /// Sets the array of Counts into the result. The array is copied and the percentages are 
    /// calculated from the overall counts.
    /// </summary>
    /// <param name="value"></param>
    private void SetCounts(long[] value)
    {
      if (value == null)
      {
        counts = null;
        return;
      }

      counts = new long[value.Length];
      Array.Copy(value, counts, value.Length);

      Percents = new double[counts.Length];

      long sum = counts.Sum();
      for (int i = 0; i < counts.Length; i++)
        Percents[i] = counts[i] == 0 ? 0 : ((double)counts[i] / sum) * 100;
    }
  }
}
