using Apache.Ignite.Core.Compute;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Executors;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.GridFabric.Arguments;

namespace VSS.VisionLink.Raptor.GridFabric.ComputeFuncs
{
    [Serializable]
    public class TileRenderRequestComputeFunc : IComputeFunc<TileRenderRequestArgument, Bitmap>
    {
        public Bitmap Invoke(TileRenderRequestArgument arg)
        {
            RenderOverlayTile render = new RenderOverlayTile
                (arg.SiteModelID,
                 arg.Mode,
                 new XYZ(arg.Extents.MinX, arg.Extents.MinY),
                 new XYZ(arg.Extents.MaxX, arg.Extents.MaxY),
                 arg.CoordsAreGrid,
                 arg.PixelsX, arg.PixelsY,
                 arg.Filter1, arg.Filter2);

            return render.Execute();
        }
    }
}
