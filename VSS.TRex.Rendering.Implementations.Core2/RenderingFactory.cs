using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Abstractions.GridFabric.Factories;
using VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses;

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
            return new Graphics(System.Drawing.Graphics.FromImage(((Bitmap) bitmap).underlyingBitmap));
        }

        public IPen CreatePen(System.Drawing.Color color)
        {
            return new Pen(color);
        }

        public IBrush CreateBrush(System.Drawing.Color color)
        {
            return new Brush(color);
        }

        public ITileRenderResponse CreateTileRenderResponse(object bmp)
        {
            return new TileRenderResponse_Core2();
        }
    }
}