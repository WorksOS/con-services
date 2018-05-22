using VSS.TRex.Exports.Patches.GridFabric.ComputeFuncs;
using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Exports.Patches.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a patch of subgrids
  /// </summary>
  public class PatchRequest : GenericASNodeRequest<PatchRequestArgument, PatchRequestComputeFunc, PatchRequestResponse>
    // Declare class like this to delegate the request to the cluster compute layer
    //    public class TileRenderRequest : GenericPSNodeBroadcastRequest<TileRenderRequestArgument, TileRenderRequestComputeFunc, TileRenderResponse>
  {
  }
}
