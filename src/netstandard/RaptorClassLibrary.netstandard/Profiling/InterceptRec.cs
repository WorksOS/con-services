using System;

namespace VSS.TRex.Profiling
{
    /// <summary>
    /// Records the attributes of an intercept between a profile line and a cell
    /// </summary>
    public struct InterceptRec : IEquatable<InterceptRec>
    {
      public float OriginX;
      public float OriginY;
      public float MidPointX;
      public float MidPointY;
      public float ProfileItemIndex;
      public float InterceptLength;

      /// <summary>
      /// Constructs a full specified intercept rec
      /// </summary>
      /// <param name="originX"></param>
      /// <param name="originY"></param>
      /// <param name="midPointX"></param>
      /// <param name="midPointY"></param>
      /// <param name="profileItemIndex"></param>
      /// <param name="interceptLength"></param>
      public InterceptRec(float originX, float originY, float midPointX, float midPointY, float profileItemIndex, float interceptLength)
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
      public bool Equals(float x, float y, float ind)
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
    }
}
