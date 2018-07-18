using Draw = System.Drawing;
using VSS.TRex.Rendering.Abstractions.GridFabric.Responses;

namespace VSS.TRex.Rendering.Abstractions
{
  public interface IRenderingFactory
  {
    IBitmap CreateBitmap(int x, int y);
    IGraphics CreateGraphics(IBitmap bitmap);
    IPen CreatePen(Draw.Color color);
    IBrush CreateBrush(Draw.Color color);

    ITileRenderResponse CreateTileRenderResponse(object bmp);
  }
}
