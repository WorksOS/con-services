using System;
using System.Collections.Generic;

namespace VSS.TRex.Profiling.Models
{
    /// <summary>
    /// Records the attributes of an intercept between a profile line and a cell
    /// </summary>
    public struct InterceptRec : IEquatable<InterceptRec>, IComparable<InterceptRec>, IComparer<InterceptRec>
  {
      public double OriginX;
      public double OriginY;
      public double MidPointX;
      public double MidPointY;
      public double ProfileItemIndex;
      public double InterceptLength;

      /// <summary>
      /// Constructs a full specified intercept rec
      /// </summary>
      /// <param name="originX"></param>
      /// <param name="originY"></param>
      /// <param name="midPointX"></param>
      /// <param name="midPointY"></param>
      /// <param name="profileItemIndex"></param>
      /// <param name="interceptLength"></param>
      public InterceptRec(double originX, double originY, double midPointX, double midPointY, double profileItemIndex, double interceptLength)
      {
        OriginX = originX;
        OriginY = originY;
        MidPointX = midPointX;
        MidPointY = midPointY;
        ProfileItemIndex = profileItemIndex;
        InterceptLength = interceptLength;
      }

      /// <summary>
      /// Compares the locations of this intercept with another
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <param name="ind"></param>
      /// <returns></returns>
      public bool Equals(double x, double y, double ind)
      {
        const double Epsilon = 0.0001;

        return Math.Abs(OriginX - x) < Epsilon && Math.Abs(OriginY - y) < Epsilon &&
               Math.Abs(ProfileItemIndex - ind) < Epsilon;
      }

      /// <summary>
      /// Compares this intercept with another
      /// </summary>
      /// <param name="other"></param>
      /// <returns></returns>
      public bool Equals(InterceptRec other)
      {
        return  Equals(other.OriginX, other.OriginY, other.ProfileItemIndex);
      }

      /// <summary>
      /// Implementation of IComparer that sorts the elements based on the ProfileItemIndex field
      /// </summary>
      /// <param name="x"></param>
      /// <param name="y"></param>
      /// <returns></returns>
      public int Compare(InterceptRec x, InterceptRec y) => x.ProfileItemIndex.CompareTo(y.ProfileItemIndex);

      public int CompareTo(InterceptRec other) => ProfileItemIndex.CompareTo(other.ProfileItemIndex);
    }
}
