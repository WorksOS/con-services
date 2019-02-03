using VSS.TRex.GridFabric.Requests;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.Reports.StationOffset.GridFabric.ComputeFuncs;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Requests
{
  /// <summary>
  /// Defines the contract for the stationOffset request made to the compute cluster
  /// </summary>
  public class StationOffsetReportRequest_ClusterCompute
    : GenericPSNodeBroadcastRequest
      <StationOffsetReportRequestArgument_ClusterCompute, StationOffsetReportRequestComputeFunc_ClusterCompute, StationOffsetReportRequestResponse_ClusterCompute>
  {
  }
}
