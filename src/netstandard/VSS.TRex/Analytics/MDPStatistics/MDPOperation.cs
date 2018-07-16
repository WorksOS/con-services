using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Analytics.Foundation;
using VSS.TRex.Analytics.MDPStatistics.GridFabric;

namespace VSS.TRex.Analytics.MDPStatistics
{
  /// <summary>
  /// Provides a client consumable operation for performing MDP analytics that returns a client model space MDP result.
  /// </summary>
  public class MDPOperation : AnalyticsOperation<MDPStatisticsRequest_ApplicationService, MDPStatisticsArgument, MDPStatisticsResponse, MDPResult>
  {
  }
}
