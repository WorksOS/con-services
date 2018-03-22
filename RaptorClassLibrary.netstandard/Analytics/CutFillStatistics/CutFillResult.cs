using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Analytics.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.Analytics.Models
{
    /// <summary>
    /// The result obtained fcrom performing a CutFill analytics request
    /// </summary>
    public class CutFillResult : AnalyticsResult
    {
        private long[] counts = null;

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
        public double[] Percents { get; set; } = null;

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
                Percents[i] = counts[i] == 0 ? 0 : ((1.0 * counts[i]) / sum) * 100;
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

        /// <summary>
        ///  Takes a response from the cluster compuet layer and transforms it into the model to be handed back to the client context
        /// </summary>
        /// <param name="response"></param>
        public override void PopulateFromClusterComputeResponse(Object response)
        {
            if (response is CutFillStatisticsResponse)
            {
                Counts = ((CutFillStatisticsResponse)response).Counts;
                ResultStatus = ((CutFillStatisticsResponse)response).ResultStatus;
            }
        }
    }
}

