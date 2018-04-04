using VSS.TRex.Rendering.Abstractions.GridFabric.Factories;
using VSS.VisionLink.Raptor.GridFabric.Requests.Interfaces;

namespace VSS.VisionLink.Raptor.Rendering.GridFabric.Responses
{
    /// <summary>
    /// Contains the response bitmap for a tile request. Supports compositing of another bitmap with this one
    /// </summary>
    public class TileRenderResponse : ITileRenderResponse, IAggregateWith<ITileRenderResponse>
    {
//        public System.Drawing.Bitmap Bitmap { get; set; }

        public virtual ITileRenderResponse AggregateWith(ITileRenderResponse other)
        {
            // Composite the bitmap held in this response with the bitmap held in 'other'

//            throw new NotImplementedException("Bitmap compositing not implemented");

            return null;
        }

        public virtual void SetBitmap(object bitmap)
        {
            // No implementaion in base class
        }
    }
}
