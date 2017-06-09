using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Executors;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.GridFabric.Arguments;
using VSS.VisionLink.Raptor.GridFabric.ComputeFuncs;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.GridFabric.Requests
{
    public static class TileRenderRequest
    {
/*
        /// <summary>
        /// The ID of the SiteModel to execute the request against
        /// </summary>
        public long SiteModelID { get; set; } = -1;

        public DisplayMode Mode { get; set; } = DisplayMode.Height;

        public BoundingWorldExtent3D Extents = BoundingWorldExtent3D.Inverted();

        public bool CoordsAreGrid { get; set;  } = false;

        public ushort PixelsX { get; set; } = 256;
        public ushort PixelsY { get; set; } = 256;

        public CombinedFilter Filter1 { get; set; } = null;
        public CombinedFilter Filter2 { get; set; } = null;

        public TileRenderRequest(long siteModelID,
                                 DisplayMode mode,
                                 BoundingWorldExtent3D extents,
                                 bool coordsAreGrid,
                                 ushort pixelsX,
                                 ushort pixelsY,
                                 CombinedFilter filter1,
                                 CombinedFilter filter2)
        {
            SiteModelID = siteModelID;
            Mode = mode;
            Extents = extents;
            CoordsAreGrid = coordsAreGrid;
            PixelsX = pixelsX;
            PixelsY = pixelsY;
            Filter1 = filter1;
            Filter2 = filter2;
        }

        public Bitmap Execute()
        {
            RenderOverlayTile render = new RenderOverlayTile
                (SiteModelID,
                 Mode,
                 new XYZ(Extents.MinX, Extents.MinY),
                 new XYZ(Extents.MaxX, Extents.MaxY),
                 CoordsAreGrid,
                 PixelsX, PixelsY,
                 Filter1, Filter2);

            return render.Execute();
        }
*/
        public static Bitmap Execute(TileRenderRequestArgument arg)
        {
//            Console.WriteLine("Mask in argument to renderer contains {0} subgrids", Mask.CountBits());

            // Construct the function to be used
            IComputeFunc<TileRenderRequestArgument, Bitmap> func = new TileRenderRequestComputeFunc();

            // Get a reference to the Ignite cluster
            IIgnite ignite = Ignition.GetIgnite("Raptor");

            // Get a reference to the compute cluster group and send the request to it for processing
            // Note: Broadcast will block until all compute nodes receiving the request have responded, or
            // until the internal Ignite timeout expires


            IClusterGroup group = ignite.GetCluster().ForRemotes().ForServers().ForAttribute("Role", "PSNode");
            ICompute compute = group.GetCompute();
            Bitmap result = compute.Apply(func, arg);

            // Send the appropriate response to the caller
            return result;
        }

    }
}
