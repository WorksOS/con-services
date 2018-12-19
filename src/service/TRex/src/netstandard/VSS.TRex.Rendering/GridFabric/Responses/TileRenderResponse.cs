using VSS.TRex.Common;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Rendering.Abstractions.GridFabric.Responses;

namespace VSS.TRex.Rendering.GridFabric.Responses
{
    /// <summary>
    /// Contains the response bitmap for a tile request. Supports compositing of another bitmap with this one
    /// </summary>
    public class TileRenderResponse : SubGridsPipelinedResponseBase, ITileRenderResponse, IAggregateWith<ITileRenderResponse>
    {
//        public Bitmap Bitmap { get; set; }

        public virtual ITileRenderResponse AggregateWith(ITileRenderResponse other)
        {
            // Composite the bitmap held in this response with the bitmap held in 'other'
            // ....

            return null;
        }

        public virtual void SetBitmap(object bitmap)
        {
            // No implementation in base class
        }
    }
}
