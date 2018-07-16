using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Abstractions.GridFabric.Responses;
using VSS.TRex.Rendering.Implementations.Framework.GridFabric.Responses;

namespace VSS.TRex.Rendering.Implementations.Framework
{
    public class RenderingFactory : IRenderingFactory
    {
        public IBitmap CreateBitmap(int x, int y)
        {
            return new Bitmap(x, y);
        }

        public IGraphics CreateGraphics(IBitmap bitmap)
        {
            return new Graphics(System.Drawing.Graphics.FromImage(((Bitmap)bitmap).UnderlyingBitmap));
        }

        public IPen CreatePen(System.Drawing.Color color)
        {
            return new Pen(color);
        }

        public IBrush CreateBrush(System.Drawing.Color color)
        {
            return new Brush(color);
        }

        public ITileRenderResponse CreateTileRenderResponse(object bitmap)
        {
            return new TileRenderResponse_Framework()
            {
                TileBitmap = (System.Drawing.Bitmap)bitmap
            };
        }
    }
}
