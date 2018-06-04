using System.Diagnostics;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.Analytics.Models;
using VSS.TRex.GridFabric.Requests.Interfaces;

namespace VSS.TRex.Analytics.GridFabric.Responses
{
    /// <summary>
    /// The response state returned from a cut/fill statistics request
    /// </summary>
    public class CutFillStatisticsResponse : BaseAnalyticsResponse, IAggregateWith<CutFillStatisticsResponse>, IAnalyticsOperationResponseResultConversion<CutFillResult>
    {
        /// <summary>
        /// An array (always 7) values representing the counts of cells within each of the cut fill bands defined in the request.
        /// </summary>
        public long[] Counts { get; set; }

        /// <summary>
        /// Aggregate a set of cut fill statistics into this set and return the result.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public CutFillStatisticsResponse AggregateWith(CutFillStatisticsResponse other)
        {
            Counts = Counts ?? new long[other.Counts.Length];

            Debug.Assert(Counts.Length == other.Counts.Length);

            for (int i = 0; i < Counts.Length; i++)
                Counts[i] += other.Counts[i];

            return this;
        }

      /// <summary>
      /// Constructs the Cut/Fill result
      /// </summary>
      /// <returns></returns>
      public CutFillResult ConstructResult()
      {
        return new CutFillResult
        {
          ResultStatus = ResultStatus,
          Counts = Counts
        };
      }
    }
}
