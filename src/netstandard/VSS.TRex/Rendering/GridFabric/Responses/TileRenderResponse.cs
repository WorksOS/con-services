using VSS.TRex.GridFabric.Requests.Interfaces;
using VSS.TRex.Rendering.Abstractions.GridFabric.Responses;

namespace VSS.TRex.Rendering.GridFabric.Responses
{
    /// <summary>
    /// Contains the response bitmap for a tile request. Supports compositing of another bitmap with this one
    /// </summary>
    public class TileRenderResponse : ITileRenderResponse, IAggregateWith<ITileRenderResponse>
    {
//        public Bitmap Bitmap { get; set; }

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
