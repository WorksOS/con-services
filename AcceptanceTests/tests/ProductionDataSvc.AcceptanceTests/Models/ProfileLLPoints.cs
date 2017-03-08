using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionDataSvc.AcceptanceTests.Models
{
    /// <summary>
    /// The two end points of a stright line used for a profile calculation, defined in WGS84 latitude longitude coordinates.
    /// </summary>
    /// 
    public class ProfileLLPoints
    {
        /// <summary>
        /// Latitude ordinate of the first profile end point. Values are expressed in radians.
        /// </summary>
        /// 
        public double lat1 { get; set; }

        /// <summary>
        /// Longitude ordinate of the first profile end point. Values are expressed in radians.
        /// </summary>
        /// 
        public double lon1 { get; set; }

        /// <summary>
        /// Latitude ordinate of the second profile end point. Values are expressed in radians.
        /// </summary>
        /// 
        public double lat2 { get; set; }

        /// <summary>
        /// Longitude ordinate of the second profile end point. Values are expressed in radians.
        /// </summary>
        /// 
        public double lon2 { get; set; }
    }
}
