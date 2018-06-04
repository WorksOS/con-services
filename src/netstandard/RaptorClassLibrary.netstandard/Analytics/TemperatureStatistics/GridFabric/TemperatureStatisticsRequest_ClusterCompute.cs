using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.Analytics.SpeedStatistics.GridFabric;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Analytics.TemperatureStatistics.GridFabric
{
	/// <summary>
	/// Sends a request to the grid for a Temperature statistics request to be executed
	/// </summary>
	public class TemperatureStatisticsRequest_ClusterCompute : GenericPSNodeBroadcastRequest<TemperatureStatisticsArgument, TemperatureStatisticsComputeFunc_ClusterCompute, TemperatureStatisticsResponse>
	{
  }
}
