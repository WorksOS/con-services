using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaptorSvcAcceptTestsCommon.Models
{
    /// <summary>
    ///     A fence (or boundary) polygon with vertices expressed as WGS84 points
    ///     This is copied from ...\RaptorServicesCommon\Models\WGS84Fence.cs
    /// </summary>
    public class WGS84Fence
    {
        /// <summary>
        ///     Array of WGS84 points defining the polygon. The polygon is implicitly closed (first and last points are not
        ///     required to be the same).
        /// </summary>
        public WGSPoint[] points { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="wgsPoints"></param>
        public WGS84Fence(WGSPoint[] wgsPoints)
        {
            points = wgsPoints;
        }
    }
}
