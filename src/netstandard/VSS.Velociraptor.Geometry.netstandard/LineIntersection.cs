using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.TRex.Common;

namespace VSS.TRex.Geometry
{
    /// <summary>
    /// Provides a simpel line intersection geometry helper method
    /// </summary>
    public static class LineIntersection
    {
        /// <summary>
        /// Returns true if the line between line1start and line1end intersects with
        /// the line between line2start and line2end.If there is an
        /// intersection then its position is returned in 'position', which is
        /// otherwise undefined.  If considerEnds is true, two lines starting or
        /// ending at the same point are considered intersecting }
        /// 
        /// </summary>
        /// <param name="line1StartX"></param>
        /// <param name="line1StartY"></param>
        /// <param name="line1EndX"></param>
        /// <param name="line1EndY"></param>
        /// <param name="line2StartX"></param>
        /// <param name="line2StartY"></param>
        /// <param name="line2EndX"></param>
        /// <param name="line2EndY"></param>
        /// <param name="PositionX"></param>
        /// <param name="PositionY"></param>
        /// <param name="considerEnds"></param>
        /// <param name="LinesAreColinear"></param>
        /// <returns></returns>
        public static bool LinesIntersect(double line1StartX,
                                          double line1StartY,
                                          double line1EndX,
                                          double line1EndY,
                                          double line2StartX,
                                          double line2StartY,
                                          double line2EndX,
                                          double line2EndY,
                                          out double PositionX,
                                          out double PositionY,
                                          bool considerEnds,
                                          out bool LinesAreColinear)
        {
            const double eps = 1e-10;

            double DeltaL2x = line2EndX - line2StartX;
            double DeltaL2y = line2EndY - line2StartY;
            double DeltaL1x = line1EndX - line1StartX;
            double DeltaL1y = line1EndY - line1StartY;
            double DeltaL1L2x = line2StartX - line1StartX;
            double DeltaL1L2y = line2StartY - line1StartY;

            double Denominator = DeltaL2x * DeltaL1y - DeltaL2y * DeltaL1x;

            if (Math.Abs(Denominator) <= eps)
            {
                LinesAreColinear = true;
                PositionX = Consts.NullDouble;
                PositionY = Consts.NullDouble;

                return false;
            }

            LinesAreColinear = false;

            Denominator = 1 / Denominator;
            double Param1 = ((DeltaL2x * DeltaL1L2y) - (DeltaL2y * DeltaL1L2x)) * Denominator;
            double Param2 = ((DeltaL1x * DeltaL1L2y) - (DeltaL1y * DeltaL1L2x)) * Denominator;

            if ((considerEnds && ((Param1 < 0.0) || (Param1 > 1.0) || (Param2 < 0.0) || (Param2 > 1.0)))
             || (!considerEnds && ((Param1 <= eps) || (Param1 >= (1.0 - eps)) ||  (Param2 <= eps) || (Param2 >= (1.0 - eps)))))
            {
                PositionX = Consts.NullDouble;
                PositionY = Consts.NullDouble;

                return false;
            }
            else
            {
                // Intersection found
                PositionX = line1StartX + DeltaL1x * Param1; // Round
                PositionY = line1StartY + DeltaL1y * Param1; // Round

                return true;
            }
        }
    }
}
