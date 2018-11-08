using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Abstractions.GridFabric.Responses;
using VSS.TRex.Rendering.Implementations.Framework.GridFabric.Responses;
using Draw = System.Drawing;
using VSS.TRex.Common;

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
            return new Graphics(Draw.Graphics.FromImage(((Bitmap)bitmap).UnderlyingBitmap));
        }

        public IPen CreatePen(Draw.Color color)
        {
            return new Pen(color);
        }

        public IBrush CreateBrush(Draw.Color color)
        {
            return new Brush(color);
        }

        public ITileRenderResponse CreateTileRenderResponse(object bitmap)
        {
            return new TileRenderResponse_Framework()
            {
                TileBitmapData = ((Draw.Bitmap)bitmap).BitmapToByteArray()
            };
        }
    }
}
