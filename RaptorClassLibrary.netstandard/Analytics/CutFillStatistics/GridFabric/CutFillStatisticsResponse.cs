using System.Diagnostics;
using VSS.VisionLink.Raptor.GridFabric.Requests.Interfaces;

namespace VSS.VisionLink.Raptor.Analytics.GridFabric.Responses
{
    /// <summary>
    /// The response state returned from a cut/fill statistics request
    /// </summary>
    public class CutFillStatisticsResponse : BaseAnalyticsResponse, IAggregateWith<CutFillStatisticsResponse>
    {
        /// <summary>
        /// An array (or always 7) values represnting the counts of cells within each of the cut fill bands defined in the request.
        /// </summary>
        public long[] Counts { get; set; }

        /// <summary>
        /// Aggregate a set of cut fill statistics into this set and return the result.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public CutFillStatisticsResponse AggregateWith(CutFillStatisticsResponse other)
        {
            if (Counts == null)
            {
                Counts = new long[other.Counts.Length];
            }

            Debug.Assert(Counts.Length == other.Counts.Length);

            for (int i = 0; i < Counts.Length; i++)
                Counts[i] += other.Counts[i];

            return this;
        }
    }
}
