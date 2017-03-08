using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaptorSvcAcceptTestsCommon.Models
{
    /// <summary>
    /// Defines a bounding box representing a 2D grid coorindate area
    /// </summary>
    public class BoundingBox2DGrid
    {
        /// <summary>
        /// The bottom left corner of the bounding box, expressed in meters
        /// </summary>
        public double bottomLeftX { get; set; }
        /// <summary>
        /// The bottom left corner of the bounding box, expressed in meters
        /// </summary>
        public double bottomleftY { get; set; }
        /// <summary>
        /// The top right corner of the bounding box, expressed in meters
        /// </summary>
        public double topRightX { get; set; }
        /// <summary>
        /// The top right corner of the bounding box, expressed in meters
        /// </summary>
        public double topRightY { get; set; }
    }

    /// <summary>
    /// Defines a bounding box representing a WGS84 latitude/longitude coorindate area
    /// </summary>
    public class BoundingBox2DLatLon
    {
        /// <summary>
        /// The bottom left corner of the bounding box, expressed in radians
        /// </summary>
        public double bottomLeftLon { get; set; }

        /// <summary>
        /// The bottom left corner of the bounding box, expressed in radians
        /// </summary>
        public double bottomleftLat { get; set; }

        /// <summary>
        /// The top right corner of the bounding box, expressed in radians
        /// </summary>
        public double topRightLon { get; set; }

        /// <summary>
        /// The top right corner of the bounding box, expressed in radians
        /// </summary>
        public double topRightLat { get; set; }
    }

    /// <summary>
    /// A 3D spatial extents structure
    /// </summary>
    public class BoundingBox3DGrid
    {
        /// <summary>
        /// Maximum X value, in the cartesian grid coordinate system, expressed in meters
        /// </summary>
        public double maxX;

        /// <summary>
        /// Maximum Y value, in the cartesian grid coordinate system, expressed in meters
        /// </summary>
        public double maxY;

        /// <summary>
        /// Maximum Z value, in the cartesian grid coordinate system, expressed in meters
        /// </summary>
        public double maxZ;

        /// <summary>
        /// Minimum X value, in the cartesian grid coordinate system, expressed in meters
        /// </summary>
        public double minX;

        /// <summary>
        /// Minimum Y value, in the cartesian grid coordinate system, expressed in meters
        /// </summary>
        public double minY;

        /// <summary>
        /// Minimum Z value, in the cartesian grid coordinate system, expressed in meters
        /// </summary>
        public double minZ;
    }
}
