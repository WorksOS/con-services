using System;
using System.Linq;

namespace VSS.TRex.Analytics.Models
{
    /// <summary>
    /// The result obtained from performing a CutFill analytics request
    /// </summary>
    public class CutFillResult : AnalyticsResult
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

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public CutFillResult()
        {
        }

        public CutFillResult(long [] counts) : this()
        {
            Counts = counts;
        }
    }
}

