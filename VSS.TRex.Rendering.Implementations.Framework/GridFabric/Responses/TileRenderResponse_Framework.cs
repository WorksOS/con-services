using VSS.TRex.Rendering.Abstractions.GridFabric.Factories;
using VSS.VisionLink.Raptor.Rendering.GridFabric.Responses;

namespace VSS.TRex.Rendering.Implementations.Framework.GridFabric.Responses
{
    public class TileRenderResponse_Framework : TileRenderResponse
    {
        public System.Drawing.Bitmap TileBitmap { get; set; }
    
        public override ITileRenderResponse AggregateWith(ITileRenderResponse other)
        {
            // Composite the bitmap held in this response with the bitmap held in 'other'
            //  throw new NotImplementedException("Bitmap compositing not implemented");

            return null;
        }

        public override void SetBitmap(object bitmap)
        {
            TileBitmap = (System.Drawing.Bitmap) bitmap;
        }
    }
}
