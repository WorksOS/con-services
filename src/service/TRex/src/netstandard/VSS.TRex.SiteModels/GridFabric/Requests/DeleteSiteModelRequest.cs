using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.SiteModels.GridFabric.ComputeFuncs;

namespace VSS.TRex.SiteModels.GridFabric.Requests
{
  /// <summary>
  /// Sends a request to the grid for a grid of values
  /// </summary>
  public class DeleteSiteModelRequest : GenericASNodeRequest<DeleteSiteModelRequestArgument, DeleteSiteModelRequestComputeFunc, DeleteSiteModelRequestResponse>
  {
    /// <summary>
    /// Site model deletion requests are coordinated from the mutable grid. All project information in the mutable grid, and all projections
    /// of that data present in the immutable grid are removed.
    /// </summary>
    public DeleteSiteModelRequest() : base(TRexGrids.MutableGridName(), ServerRoles.TAG_PROCESSING_NODE)
    {
    }
  }
}
