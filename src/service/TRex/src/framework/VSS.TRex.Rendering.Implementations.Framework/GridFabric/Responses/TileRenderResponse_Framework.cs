using VSS.TRex.Rendering.Abstractions.GridFabric.Responses;
using VSS.TRex.Rendering.GridFabric.Responses;
using Draw = System.Drawing;
using VSS.TRex.Common;

namespace VSS.TRex.Rendering.Implementations.Framework.GridFabric.Responses
{
    public class TileRenderResponse_Framework : TileRenderResponse
    {
        public byte[] TileBitmapData { get; set; }
    
        public override ITileRenderResponse AggregateWith(ITileRenderResponse other)
        {
            // Composite the bitmap held in this response with the bitmap held in 'other'
            //  throw new NotImplementedException("Bitmap compositing not implemented");

            return null;
        }

        public override void SetBitmap(object bitmap)
        {
            TileBitmapData = ((Draw.Bitmap) bitmap)?.BitmapToByteArray();
        }
    }
}
