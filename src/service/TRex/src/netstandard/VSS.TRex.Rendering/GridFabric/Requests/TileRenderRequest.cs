using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.GridFabric.Requests;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.ComputeFuncs;
using VSS.TRex.Rendering.GridFabric.Responses;

namespace VSS.TRex.Rendering.GridFabric.Requests
{
  /// <summary>
  /// Sends a request to the grid cluster compute layer for a tile to be rendered
  /// </summary>
  public class TileRenderRequest : GenericASNodeRequest<TileRenderRequestArgument, TileRenderRequestComputeFunc, TileRenderResponse>
  {
    public TileRenderRequest() : base(TRexGrids.ImmutableGridName(), ServerRoles.TILE_RENDERING_NODE)
    {

    }
  }
}
