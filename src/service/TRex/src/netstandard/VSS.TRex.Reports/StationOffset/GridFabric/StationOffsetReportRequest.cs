using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Reports.StationOffset.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a grid of values
  /// </summary>
  public class StationOffsetReportRequest : GenericASNodeRequest<StationOffsetReportRequestArgument, StationOffsetReportRequestComputeFunc, StationOffsetReportRequestResponse>
  // Declare class like this to delegate the request to the cluster compute layer
  {
    public StationOffsetReportRequest() : base(TRexGrids.ImmutableGridName(), ServerRoles.REPORTING_ROLE)
    {
    }
  }
}
