using System;
using VSS.TRex.Designs.TTM;

namespace VSS.TRex.Exports.Surfaces
{
    public static class TinningUtils
    {
      private static double noCotangent = -1E20;

      public static double XProduct(TriVertex thePoint, TriVertex basePt1, TriVertex basePt2)
      {
        return (thePoint.X - basePt2.X) * (thePoint.Y - basePt1.Y) - (thePoint.Y - basePt2.Y) * (thePoint.X - basePt1.X);
      }

      public static double DotProduct(TriVertex thePoint, TriVertex basePt1, TriVertex basePt2)
      {
          return (thePoint.X - basePt2.X) * (thePoint.X - basePt1.X) + (thePoint.Y - basePt2.Y) * (thePoint.Y - basePt1.Y);
      }

      public static double Cotangent(TriVertex thePoint, TriVertex basePt1, TriVertex basePt2)
      {
        double Eps = 0.000001;
        double crossProduct = XProduct(thePoint, basePt1, basePt2);

        return (Math.Abs(crossProduct) < Eps) ? noCotangent : DotProduct(thePoint, basePt1, basePt2) / crossProduct;
      }

      public static bool DefinitelyLeftOfBaseLine(TriVertex thePoint, TriVertex basePt1, TriVertex basePt2)
      {
        double Eps = 0.00000001;

        return XProduct(thePoint, basePt1, basePt2) < -Eps;
      }
    }
}
