using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Reports.Gridded.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a grid of values
  /// </summary>
  public class GriddedReportRequest : GenericASNodeRequest<GriddedReportRequestArgument, GriddedReportRequestComputeFunc, GriddedReportRequestResponse>
  // Declare class like this to delegate the request to the cluster compute layer
  //    public class GridRequest : GenericPSNodeBroadcastRequest<GriddedReportRequestArgument, GridRequestComputeFunc, GridRequestResponse>
  {
    public GriddedReportRequest() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
    {
    }
  }
}
