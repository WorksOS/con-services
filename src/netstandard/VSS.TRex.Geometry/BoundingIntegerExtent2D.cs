using System;

namespace VSS.TRex.Geometry
{
  [Serializable]
  public struct BoundingIntegerExtent2D : IEquatable<BoundingIntegerExtent2D>
  {
    public int MinX, MinY, MaxX, MaxY;

    /// <summary>
    /// Calculates the area in square meters of the X/Y plan extent. If the extent is too large to represent
    /// as a 64 bit number of square meters, -1 is returned
    /// </summary>
    public long Area()
    {
      try
      {
        return (long) (MaxX - MinX) * (long) (MaxY - MinY);
      }
      catch
      {
        return -1;
      }
    }

    /// <summary>
    /// Assign the context of another 3D bounding extent to this one
    /// </summary>
    /// <param name="source"></param>
    public void Assign(BoundingIntegerExtent2D source)
    {
      MinX = source.MinX;
      MinY = source.MinY;
      MaxX = source.MaxX;
      MaxY = source.MaxY;
    }

    /// <summary>
    /// Produce as human readable form of the state in this bounding extent
    /// </summary>
    /// <returns></returns>
    public override string ToString() => string.Format("MinX: {0}, MinY:{1}, MaxX: {2}, MaxY:{3}", MinX, MinY, MaxX, MaxY);

    /// <summary>
    /// Construct a 2D bounding extent from the supplied parameters
    /// </summary>
    /// <param name="AMinX"></param>
    /// <param name="AMinY"></param>
    /// <param name="AMaxX"></param>
    /// <param name="AMaxY"></param>
    public BoundingIntegerExtent2D(int AMinX, int AMinY, int AMaxX, int AMaxY)
    {
      MinX = AMinX;
      MinY = AMinY;
      MaxX = AMaxX;
      MaxY = AMaxY;
    }

    /// <summary>
    /// Determine is this bounding extent encloses the extent provided as a parameter
    /// </summary>
    /// <param name="AExtent"></param>
    /// <returns></returns>
    public bool Encloses(BoundingIntegerExtent2D AExtent)
    {
      return IsValidExtent && AExtent.IsValidExtent &&
             (MinX <= AExtent.MinX) && (MinY <= AExtent.MinY) &&
             (MaxX >= AExtent.MaxX) && (MaxY >= AExtent.MaxY);
    }

    /// <summary>
    /// Expand the extent covered in X & Y isotropically using the supplied Delta
    /// </summary>
    /// <param name="Delta"></param>
    public void Expand(int Delta)
    {
      MinX -= Delta;
      MaxX += Delta;
      MinY -= Delta;
      MaxY += Delta;
    }

    /// <summary>
    /// Include the integer location coordinate specified by the X and Y parameters into the 2D bounding extent
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    public void Include(int X, int Y)
    {
      if (MinX > X) MinX = X;
      if (MaxX < X) MaxX = X;
      if (MinY > Y) MinY = Y;
      if (MaxY < Y) MaxY = Y;
    }

    /// <summary>
    /// Include the extent contained in the parameter into the 2D bounding extent
    /// </summary>
    /// <param name="Extent"></param>
    public void Include(BoundingIntegerExtent2D Extent)
    {
      if (Extent.IsValidExtent)
      {
        Include(Extent.MinX, Extent.MinY);
        Include(Extent.MaxX, Extent.MaxY);
      }
    }

    /// <summary>
    /// Determine if the 2D bounding extent inludes the coorindate given by the X and Y parameters
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <returns>A boolean indicating where the boundign extent includes the given position</returns>
    public bool Includes(int x, int y) => (x >= MinX) && (x <= MaxX) && (y >= MinY) && (y <= MaxY);

    public bool Includes(uint x, uint y) => (x >= MinX) && (x <= MaxX) && (y >= MinY) && (y <= MaxY);

    /// <summary>
    /// Determing if the extent defined is valid in that it does not define a negative area
    /// </summary>
    public bool IsValidExtent => (MaxX >= MinX) && (MaxY >= MinY);

    /// <summary>
    /// Move the 2D bounding extent int he X and Y dimenions by the delta X & Y supplied in the parameters
    /// </summary>
    /// <param name="DX"></param>
    /// <param name="DY"></param>
    public void Offset(int DX, int DY)
    {
      MinX += DX;
      MaxX += DX;
      MinY += DY;
      MaxY += DY;
    }

    public void SetInverted()
    {
      MinX = int.MaxValue;
      MaxX = int.MinValue;
      MinY = int.MaxValue;
      MaxY = int.MinValue;
    }

    /// <summary>
    /// Compute the size of the X dimension in the bounding extents
    /// </summary>
    public int SizeX => MaxX - MinX;

    /// <summary>
    /// Compute the size of the Y dimension in the bounding extents
    /// </summary>
    public int SizeY => MaxY - MinY;

    /// <summary>
    /// Determine if this bounding extent equals another bounding extent instance
    /// </summary>
    /// <param name="extent"></param>
    /// <returns></returns>
    public bool Equals(BoundingIntegerExtent2D extent) => (MinX == extent.MinX) && (MinY == extent.MinY) && (MaxX == extent.MaxX) && (MaxY == extent.MaxY);

    /// <summary>
    /// Creates a new 2D bounding extents structure with the corner points 'inverted'
    /// </summary>
    /// <returns></returns>
    public static BoundingIntegerExtent2D Inverted()
    {
      BoundingIntegerExtent2D result = new BoundingIntegerExtent2D();
      result.SetInverted();

      return result;
    }
  }
}
