using VSS.TRex.GridFabric.Requests;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.ComputeFuncs;
using VSS.TRex.Rendering.GridFabric.Responses;

namespace VSS.TRex.Rendering.GridFabric.Requests
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
