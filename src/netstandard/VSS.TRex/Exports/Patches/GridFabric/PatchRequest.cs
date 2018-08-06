using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.Servers;

namespace VSS.TRex.Exports.Patches.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a patch of subgrids
  /// </summary>
  public class PatchRequest : GenericASNodeRequest<PatchRequestArgument, PatchRequestComputeFunc, PatchRequestResponse>
  // Declare class like this to delegate the request to the cluster compute layer
  //    public class PatchRequest : GenericPSNodeBroadcastRequest<TileRenderRequestArgument, TileRenderRequestComputeFunc, TileRenderResponse>
  {
    public PatchRequest() : base(TRexGrids.ImmutableGridName(), ServerRoles.ASNODE)
    {
    }
  }
}
