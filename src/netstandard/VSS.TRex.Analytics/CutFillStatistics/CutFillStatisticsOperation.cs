using VSS.TRex.Analytics.CutFillStatistics.GridFabric;
using VSS.TRex.Analytics.Foundation;
namespace VSS.TRex.Analytics.CutFillStatistics
{
  /// <summary>
  /// Provides a client consumable operation for performing cut fill analytics that returns a client model space cut fill result.
  /// </summary>
  public class CutFillStatisticsOperation : AnalyticsOperation<CutFillStatisticsRequest_ApplicationService, CutFillStatisticsArgument, CutFillStatisticsResponse, CutFillStatisticsResult>
    { }

    /*
    /// <summary>
    /// Provides a client consumable operation for performing cut fill analytics that returns a client model space cut fill result.
    /// </summary>
    public class CutFillOperation
    {
        /// <summary>
        /// Execute the cut fill operation with the supplied argument
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public CutFillResult Execute(CutFillStatisticsArgument arg)
        {
            var request = new CutFillStatisticsRequest_ApplicationService();

            CutFillResult result = new CutFillResult();

            return result;
        }
    }
    */
}
