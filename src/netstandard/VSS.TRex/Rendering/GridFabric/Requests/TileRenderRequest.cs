using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.ComputeFuncs;
using VSS.TRex.Rendering.GridFabric.Responses;
using VSS.TRex.Servers;

namespace VSS.TRex.Rendering.GridFabric.Requests
{
    /// <summary>
    /// Sends a request to the grid for a tile to be rendered
    /// </summary>
    public class TileRenderRequest : GenericASNodeRequest<TileRenderRequestArgument, TileRenderRequestComputeFunc, TileRenderResponse>
    // Declare class like this to delegate the request to the cluster compute layer
    //    public class TileRenderRequest : GenericPSNodeBroadcastRequest<TileRenderRequestArgument, TileRenderRequestComputeFunc, TileRenderResponse>
    {
      public TileRenderRequest() : base(TRexGrids.ImmutableGridName(), ServerRoles.TILE_RENDERING_NODE)
      {

      }
  }
}
