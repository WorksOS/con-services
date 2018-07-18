using Draw = System.Drawing;
using VSS.TRex.Rendering.Abstractions.GridFabric.Responses;
using VSS.TRex.Rendering.GridFabric.Responses;

namespace VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses
{
    public class TileRenderResponse_Core2 : TileRenderResponse
    {
        public Draw.Bitmap TileBitmap { get; set; }

        public override ITileRenderResponse AggregateWith(ITileRenderResponse other)
        {
            // Composite the bitmap held in this response with the bitmap held in 'other'

            //            throw new NotImplementedException("Bitmap compositing not implemented");

            return null;
        }

        public override void SetBitmap(object bitmap)
        {
            TileBitmap = (Draw.Bitmap) bitmap;
        }
    }
}
