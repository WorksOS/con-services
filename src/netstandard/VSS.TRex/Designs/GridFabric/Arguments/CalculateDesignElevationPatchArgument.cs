using System;
using VSS.TRex.Designs;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Designs.GridFabric.Arguments
{
    [Serializable]
    public class CalculateDesignElevationPatchArgument : BaseApplicationServiceRequestArgument
    {
        /// <summary>
        /// The ID of the SiteModel to execute the request against
        /// </summary>
        public Guid SiteModelID { get; set; } = Guid.Empty;

        /// <summary>
        /// The X origin location for the patch of elevations to be computed from
        /// </summary>
        public uint OriginX { get; set; }

        /// <summary>
        /// The Y origin location for the patch of elevations to be computed from
        /// </summary>
        public uint OriginY { get; set; }

        /// <summary>
        /// The cell stepping size to move between points in the patch being interpolated
        /// </summary>
        public double CellSize { get; set; }

        /// <summary>
        /// The descriptor of the design file the elevations are to be interpolated from
        /// </summary>
        public DesignDescriptor DesignDescriptor { get; set; }

        /// <summary>
        /// A map of the cells within the subgrid patch to be computed
        /// </summary>
//        public SubGridTreeBitmapSubGridBits ProcessingMap { get; set; }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public CalculateDesignElevationPatchArgument()
        {
        }

        /// <summary>
        /// Constructor taking the full state of the elevation patch computation operation
        /// </summary>
        /// <param name="siteModelID"></param>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        /// <param name="cellSize"></param>
        /// <param name="designDescriptor"></param>
        // /// <param name="processingMap"></param>
        public CalculateDesignElevationPatchArgument(Guid siteModelID,
                                         uint originX,
                                         uint originY,
                                         double cellSize,
                                         DesignDescriptor designDescriptor/*,
                                         SubGridTreeBitmapSubGridBits processingMap*/) : this()
        {
            SiteModelID = siteModelID;
            OriginX = originX;
            OriginY = originY;
            CellSize = cellSize;
            DesignDescriptor = designDescriptor;
//            ProcessingMap = processingMap;
        }

        /// <summary>
        /// Overloaded ToString to add argument properties
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return base.ToString() + $" -> SiteModel:{SiteModelID}, Origin:{OriginX}/{OriginY}, CellSize:{CellSize}, Design:{DesignDescriptor}";
        }
    }
}
