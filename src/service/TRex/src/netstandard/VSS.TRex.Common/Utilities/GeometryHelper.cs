using System;

namespace VSS.TRex.Common.Utilities
{
    /// <summary>
    /// Some handy geometry helper methods
    /// </summary>
    public static class GeometryHelper
    {
        /// <summary>
        /// Rotates a point in cartesian coordinates about an origin point by a specified rotation in radians in an anti clockwise direction
        /// (ie: a positive rotation rotates the point in a mathematical sense in an anti-clockwise direction)
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="fromX"></param>
        /// <param name="fromY"></param>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
        /// <param name="CX"></param>
        /// <param name="CY"></param>
        public static void RotatePointAbout(double rotation,
                                            double fromX, double fromY,
                                            out double toX, out double toY,
                                            double CX, double CY)
        {
            double CosOfRotation = Math.Cos(rotation);
            double SinOfRotation = Math.Sin(rotation);

            toX = CX + (fromX - CX) * CosOfRotation - (fromY - CY) * SinOfRotation;
            toY = CY + (fromY - CY) * CosOfRotation + (fromX - CX) * SinOfRotation;
        }
    }
}
