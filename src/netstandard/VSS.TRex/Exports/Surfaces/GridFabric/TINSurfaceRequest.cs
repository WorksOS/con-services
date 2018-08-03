using VSS.TRex.GridFabric.Requests;

namespace VSS.TRex.Exports.Surfaces.GridFabric
{
  /// <summary>
  /// Sends a request to the grid for a surface to be constructed from elevation data
  /// </summary>
  public class TINSurfaceRequest : GenericASNodeRequest<TINSurfaceRequestArgument, TINSurfaceRequestComputeFunc, TINSurfaceRequestResponse>
    // Declare class like this to delegate the request to the cluster compute layer
    //    public class PatchRequest : GenericPSNodeBroadcastRequest<TileRenderRequestArgument, TileRenderRequestComputeFunc, TileRenderResponse>
  {
  }
}
