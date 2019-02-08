using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Exports.CSV.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a grid of values
  /// </summary>
  public class CSVExportRequest : GenericASNodeRequest<CSVExportRequestArgument, CSVExportRequestComputeFunc, CSVExportRequestResponse>
  // Declare class like this to delegate the request to the cluster compute layer
  {
    public CSVExportRequest() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
    {
    }
  }
}
