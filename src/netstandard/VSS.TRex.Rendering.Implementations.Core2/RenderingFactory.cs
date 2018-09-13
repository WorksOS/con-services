using Draw = System.Drawing;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Abstractions.GridFabric.Responses;
using VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses;
using VSS.TRex.Common;

namespace VSS.TRex.Rendering.Implementations.Core2
{
    public class RenderingFactory : IRenderingFactory
    {
        public IBitmap CreateBitmap(int x, int y)
        {
            return new Bitmap(x, y);
        }

        public IGraphics CreateGraphics(IBitmap bitmap)
        {
            return new Graphics(Draw.Graphics.FromImage(((Bitmap) bitmap).UnderlyingBitmap));
        }

        public IPen CreatePen(Draw.Color color)
        {
            return new Pen(color);
        }

        public IBrush CreateBrush(Draw.Color color)
        {
            return new Brush(color);
        }

        public ITileRenderResponse CreateTileRenderResponse(object bmp)
        {
          return new TileRenderResponse_Core2
          {
            TileBitmapData = ((Draw.Bitmap)bmp)?.BitmapToByteArray()
          }; ;
        }
    }
}
