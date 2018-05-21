using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Analytics.GridFabric.ComputeFuncs;

namespace VSS.TRex.Analytics.SpeedStatistics.GridFabric
{
	/// <summary>
	/// Speed statistics specific request to make to the cluster compute context
	/// </summary>
  public class SpeedStatisticsComputeFunc_ClusterCompute : AnalyticsComputeFunc_ClusterCompute<SpeedStatisticsArgument, SpeedStatisticsResponse, SpeedCoordinator>
	{
  }
}
