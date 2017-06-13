using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Filters;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.GridFabric.Arguments
{
    [Serializable]
    public class TileRenderRequestArgument : BaseApplicationServiceRequestArgument
    {
        /// <summary>
        /// The ID of the SiteModel to execute the request against
        /// </summary>
        public long SiteModelID { get; set; } = -1;

        public DisplayMode Mode { get; set; } = DisplayMode.Height;

        public BoundingWorldExtent3D Extents = BoundingWorldExtent3D.Inverted();

        public bool CoordsAreGrid { get; set; } = false;

        public ushort PixelsX { get; set; } = 256;
        public ushort PixelsY { get; set; } = 256;

        public CombinedFilter Filter1 { get; set; } = null;
        public CombinedFilter Filter2 { get; set; } = null;

        public TileRenderRequestArgument(long siteModelID,
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
    }
}
