﻿using VSS.TRex.Analytics.CMVStatistics.Details;
using VSS.TRex.Analytics.GridFabric.ComputeFuncs;

namespace VSS.TRex.Analytics.CMVStatistics.GridFabric.Details
{
  /// <summary>
  /// CMV details specific request to make to the cluster compute context
  /// </summary>
  public class CMVdetailsComputeFunc_ClusterCompute : AnalyticsComputeFunc_ClusterCompute<CMVDetailsArgument, CMVDetailsResponse, CMVDetailsCoordinator>
  {
  }
}
