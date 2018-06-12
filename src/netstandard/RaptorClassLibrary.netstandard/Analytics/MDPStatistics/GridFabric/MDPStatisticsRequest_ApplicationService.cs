using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.MDPStatistics.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a MDP statistics request to be executed
  /// </summary>
  public class MDPStatisticsRequest_ApplicationService : GenericASNodeRequest<MDPStatisticsArgument, MDPStatisticsComputeFunc_ApplicationService, MDPStatisticsResponse>
  {
  }
}
