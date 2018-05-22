using VSS.TRex.GridFabric.Requests;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.ComputeFuncs;
using VSS.TRex.Rendering.GridFabric.Responses;

namespace VSS.TRex.Exports.Patches.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a tile to be rendered
  /// </summary>
  public class PatchRequest : GenericASNodeRequest<PatchRequestArgument, PatchRequestComputeFunc, PatchRequestResponse>
    // Declare class like this to delegate the request to the cluster compute layer
    //    public class TileRenderRequest : GenericPSNodeBroadcastRequest<TileRenderRequestArgument, TileRenderRequestComputeFunc, TileRenderResponse>
  {
  }
}
