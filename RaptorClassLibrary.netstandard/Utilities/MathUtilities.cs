using System;

namespace VSS.TRex.Utilities
{
    /// <summary>
    /// A collection of things that aren't readily apparent in the .Net platform
    /// </summary>
    public static class MathUtilities
    {
        /// <summary>
        /// Calculates the lenth of the hypotenuse of a right angled triangle where the length of
        /// the other two sides is given by dx and dy
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <returns></returns>
        public static double Hypot(double dx, double dy) => Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
    }
}
