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
        /// <summary>
        /// An array (or always 7) values represnting the counts of cells within each of the cut fill bands defined in the request.
        /// </summary>
        public long[] Counts
        {
            get
            {
                return Counts;
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
            Counts = new long[value.Length];
            Array.Copy(value, Counts, value.Length);

            Percents = new double[Counts.Length];

            long sum = Counts.Sum();
            for (int i = 0; i < Counts.Length; i++)
                Percents[i] = Counts[i] == 0 ? 0 : (Counts[i] / sum) * 100;
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
            }
        }
    }
}

