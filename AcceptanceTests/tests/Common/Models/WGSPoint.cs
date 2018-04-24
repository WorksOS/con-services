using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaptorSvcAcceptTestsCommon.Models
{
    /// <summary>
    /// A point specified in WGS 84 latitude/longtitude coordinates.
    /// </summary>
    public class WGSPoint : IEquatable<WGSPoint>
    {
        #region Members
        /// <summary>
        ///     WGS84 latitude, expressed in radians
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        ///     WSG84 longitude, expressed in radians
        /// </summary>
        public double Lon { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Lat"></param>
        /// <param name="Lon"></param>
        public WGSPoint(double Lat, double Lon)
        {
            this.Lat = Lat;
            this.Lon = Lon;
        }  
        #endregion
        
        #region Equality test
        public bool Equals(WGSPoint other)
        {
            if (other == null)
                return false;

            return this.Lat == other.Lat && this.Lon == other.Lon;
        }

        public static bool operator ==(WGSPoint a, WGSPoint b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(WGSPoint a, WGSPoint b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is WGSPoint && this == (WGSPoint)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
