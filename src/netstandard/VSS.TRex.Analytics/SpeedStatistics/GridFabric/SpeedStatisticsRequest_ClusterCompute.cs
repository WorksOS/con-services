using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.SpeedStatistics.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a Speed statistics request to be computed
  /// </summary>
  public class SpeedStatisticsRequest_ClusterCompute : GenericPSNodeBroadcastRequest<SpeedStatisticsArgument, SpeedStatisticsComputeFunc_ClusterCompute, SpeedStatisticsResponse>
	{
  }
}
