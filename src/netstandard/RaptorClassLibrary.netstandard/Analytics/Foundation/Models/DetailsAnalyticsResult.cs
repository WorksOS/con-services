using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.TRex.Analytics.Models;

namespace VSS.TRex.Analytics.Foundation.Models
{
  /// <summary>
  /// Base class for details results sent to client calling contexts for analytics functions
  /// </summary>
  public class DetailsAnalyticsResult : AnalyticsResult
  {
    private long[] counts;

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
