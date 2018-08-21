using System;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Models.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Rendering.GridFabric.Arguments
{
    [Serializable]
    public class TileRenderRequestArgument : BaseApplicationServiceRequestArgument
    {
        /// <summary>
        /// The ID of the SiteModel to execute the request against
        /// </summary>
        public Guid SiteModelID { get; set; } = Guid.Empty;

        public DisplayMode Mode { get; set; } = DisplayMode.Height;

        public BoundingWorldExtent3D Extents = BoundingWorldExtent3D.Inverted();

        public bool CoordsAreGrid { get; set; }

        public ushort PixelsX { get; set; } = 256;
        public ushort PixelsY { get; set; } = 256;

        public ICombinedFilter Filter1 { get; set; }
        public ICombinedFilter Filter2 { get; set; }

        public TileRenderRequestArgument(Guid siteModelID,
                                         DisplayMode mode,
                                         BoundingWorldExtent3D extents,
                                         bool coordsAreGrid,
                                         ushort pixelsX,
                                         ushort pixelsY,
                                         ICombinedFilter filter1,
                                         ICombinedFilter filter2,
                                         Guid cutFillDesignID /*DesignDescriptor cutFillDesign*/)
        {
            SiteModelID = siteModelID;
            Mode = mode;
            Extents = extents;
            CoordsAreGrid = coordsAreGrid;
            PixelsX = pixelsX;
            PixelsY = pixelsY;
            Filter1 = filter1;
            Filter2 = filter2;
            CutFillDesignID = cutFillDesignID; // CutFillDesign = cutFillDesign;
        }
    }
}
