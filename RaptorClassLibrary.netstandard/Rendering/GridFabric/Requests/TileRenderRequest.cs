using VSS.VisionLink.Raptor.GridFabric.Requests;
using VSS.VisionLink.Raptor.Rendering.GridFabric.Arguments;
using VSS.VisionLink.Raptor.Rendering.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.Rendering.GridFabric.Responses;

namespace VSS.VisionLink.Raptor.Rendering.GridFabric.Requests
{
    /// <summary>
    /// Sends a request to the grid for a tile to be rendered
    /// </summary>
    public class TileRenderRequest : GenericASNodeRequest<TileRenderRequestArgument, TileRenderRequestComputeFunc, TileRenderResponse>
    // Declare class like this to delegate the request to the cluster compute layer
    //    public class TileRenderRequest : GenericPSNodeBroadcastRequest<TileRenderRequestArgument, TileRenderRequestComputeFunc, TileRenderResponse>
    {
    }
}
