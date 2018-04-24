using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaptorSvcAcceptTestsCommon.Models
{
    /// <summary>
    /// A spatial coordinate within the grid coordinate system used by a project.
    /// </summary>
    public class Point : IEquatable<Point>
    {
        #region Members
		/// <summary>
        /// The X-ordinate of the position, expressed in meters
        /// </summary>
        public double x { get; set; }

        /// <summary>
        /// The Y-ordinate of the position, expressed in meters
        /// </summary>
        public double y { get; set; }  
	    #endregion
        
        #region Equality test
        public bool Equals(Point other)
        {
            if (other == null)
                return false;

            return this.x == other.x && this.y == other.y;
        }

        public static bool operator ==(Point a, Point b)
        {
            if ((object)a == null || (object)b == null)
                return Object.Equals(a, b);

            return a.Equals(b);
        }

        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is Point && this == (Point)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}