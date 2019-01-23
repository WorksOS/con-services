using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.Reports.StationOffset.GridFabric.Arguments;
using VSS.TRex.Reports.StationOffset.GridFabric.ComputeFuncs;
using VSS.TRex.Reports.StationOffset.GridFabric.Responses;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Requests
{
  /// <summary>
  /// Defines the contract for the stationOffsetReport request made to the applications service
  /// </summary>
  public class StationOffsetReportRequest_ApplicationService 
    : GenericASNodeRequest<StationOffsetReportRequestArgument_ApplicationService, StationOffsetReportRequestComputeFunc_ApplicationService, StationOffsetReportRequestResponse> 
  {
    public StationOffsetReportRequest_ApplicationService() : base(TRexGrids.ImmutableGridName(), ServerRoles.REPORTING_ROLE)
    {
    }
  }
}
