using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.SiteModels.GridFabric.ComputeFuncs;

namespace VSS.TRex.SiteModels.GridFabric.Requests
{
  /// <summary>
  /// Sends a request to the grid for a grid of values
  /// </summary>
  public class RebuildSiteModelRequest : GenericASNodeRequest<RebuildSiteModelRequestArgument, RebuildSiteModelRequestComputeFunc, RebuildSiteModelRequestResponse>
  {
    /// <summary>
    /// Site model rebuild requests are coordinated from the mutable grid. 
    /// </summary>
    public RebuildSiteModelRequest() : base(TRexGrids.MutableGridName(), ServerRoles.PROJECT_REBUILDER_ROLE)
    {
    }
  }
}
