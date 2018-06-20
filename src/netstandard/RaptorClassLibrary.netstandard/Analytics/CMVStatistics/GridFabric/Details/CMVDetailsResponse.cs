using System.Diagnostics;
using VSS.TRex.Analytics.CMVStatistics.Details;
using VSS.TRex.Analytics.Foundation.GridFabric.Responses;
using VSS.TRex.Analytics.Foundation.Interfaces;
using VSS.TRex.GridFabric.Requests.Interfaces;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric.Details
{
  /// <summary>
  /// The response state returned from a CMV details request
  /// </summary>
  public class CMVDetailsResponse : StatisticAnalyticsResponse, IAggregateWith<CMVDetailsResponse>, IAnalyticsOperationResponseResultConversion<CMVDetailsResult>
  {
    /// <summary>
    /// An array values representing the counts of cells within each of the CMV details bands defined in the request.
    /// The array's size is the same as the number of the CMV details bands.
    /// </summary>
    public long[] Counts { get; set; }

    public CMVDetailsResponse AggregateWith(CMVDetailsResponse other)
    {
      return base.AggregateWith(other) as CMVDetailsResponse;
    }

    /// <summary>
    /// Aggregate a set of CMV details into this set and return the result.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    protected override void AggregateBaseDataWith(StatisticAnalyticsResponse other)
    {
      base.AggregateBaseDataWith(other);

      var otherResponse = (CMVDetailsResponse) other;

      Counts = Counts ?? new long[otherResponse.Counts.Length];

      Debug.Assert(Counts.Length == otherResponse.Counts.Length);

      for (int i = 0; i < Counts.Length; i++)
        Counts[i] += otherResponse.Counts[i];
    }

    public CMVDetailsResult ConstructResult()
    {
      return new CMVDetailsResult()
      {
        ResultStatus = ResultStatus,
        Counts = Counts
      };
    }
  }
}
