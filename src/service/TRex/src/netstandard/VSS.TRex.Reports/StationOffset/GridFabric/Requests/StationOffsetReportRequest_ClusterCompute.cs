using VSS.TRex.GridFabric.Requests;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;
using VSS.TRex.Reports.StationOffset.GridFabric.ComputeFuncs;

namespace VSS.TRex.Reports.StationOffset.Executors
{
  /// <summary>
  /// Defines the contract for the profile request made to the compute cluster
  /// </summary>
  public class StationOffsetReportRequest_ClusterCompute
    : GenericPSNodeBroadcastRequest
      <StationOffsetReportRequestArgument_ClusterCompute, StationOffsetReportRequestComputeFunc_ClusterCompute, StationOffsetReportRequestResponse_ClusterCompute>
  {
  }
}
