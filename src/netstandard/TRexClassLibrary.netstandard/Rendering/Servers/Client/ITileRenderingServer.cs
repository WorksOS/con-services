using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.Responses;

namespace VSS.TRex.Rendering.Servers.Client
{
  public interface ITileRenderingServer
  {
    TileRenderResponse RenderTile(TileRenderRequestArgument argument);
  }
}
