using System;
using System.IO;
using VSS.TRex.Common;

namespace VSS.TRex.Geometry
{
  /// <summary>
  /// T3DBoundingWorldExtent describes a plan extent (X and Y) covering a
  /// rectangular area of the world in world coordinates, and a height range
  /// within that extent
  /// </summary>
  public class BoundingWorldExtent3D : IEquatable<BoundingWorldExtent3D>
  {
    /// <summary>
    /// The Min/Max X/Y/Z values describing the 3D bounding extent
    /// </summary>
    public double MinX, MinY, MaxX, MaxY, MinZ, MaxZ;

    /// <summary>
    /// Calculates the area in square meters of the X/Y plan extent
    /// </summary>
    public double Area => (MaxX - MinX) * (MaxY - MinY);

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public BoundingWorldExtent3D()
    {
    }

    public BoundingWorldExtent3D(BoundingWorldExtent3D source)
    {
      Assign(source);
    }

    /// <summary>
    /// Assign another instance to this instance
    /// </summary>
    /// <param name="source"></param>
    public void Assign(BoundingWorldExtent3D source)
    {
      MinX = source.MinX;
      MinY = source.MinY;
      MinZ = source.MinZ;
      MaxX = source.MaxX;
      MaxY = source.MaxY;
      MaxZ = source.MaxZ;
    }

    /// <summary>
    /// Provide human readable version of instance state
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return $"MinX: {MinX}, MaxX:{MaxX}, MinY:{MinY}, MaxY:{MaxY}, MinZ: {MinZ}, MaxZ:{MaxZ}";
    }

    /// <summary>
    /// Calculate mid-point in the X axis of the bounding extent
    /// </summary>
    public double CenterX => MinX + (MaxX - MinX) / 2;

    /// <summary>
    /// Calculate mid-point in the Y axis of the bounding extent
    /// </summary>
    public double CenterY => MinY + (MaxY - MinY) / 2;

    /// <summary>
    /// Calculate mid-point in the Z axis of the bounding extent
    /// </summary>
    public double CenterZ => MinZ + (MaxZ - MinZ) / 2;

    /// <summary>
    /// Set all min/max values to zero (0)
    /// </summary>
    public void Clear()
    {
      MinX = 0;
      MaxX = 0;
      MinY = 0;
      MaxY = 0;
      MinZ = 0;
      MaxZ = 0;
    }

    /// <summary>
    /// Constructor taking min/max X & Y values but allowing Z values to default to null
    /// </summary>
    /// <param name="AMinX"></param>
    /// <param name="AMinY"></param>
    /// <param name="AMaxX"></param>
    /// <param name="AMaxY"></param>
    public BoundingWorldExtent3D(double AMinX, double AMinY, double AMaxX, double AMaxY) : this(AMinX, AMinY, AMaxX, AMaxY, Consts.NullDouble, Consts.NullDouble)
    {
    }

    /// <summary>
    /// Full 3d constructor taking min/max X, Y and Z values
    /// </summary>
    /// <param name="AMinX"></param>
    /// <param name="AMinY"></param>
    /// <param name="AMaxX"></param>
    /// <param name="AMaxY"></param>
    /// <param name="AMinZ"></param>
    /// <param name="AMaxZ"></param>
    public BoundingWorldExtent3D(double AMinX, double AMinY, double AMaxX, double AMaxY, double AMinZ, double AMaxZ)
    {
      MinX = AMinX;
      MinY = AMinY;
      MaxX = AMaxX;
      MaxY = AMaxY;
      MinZ = AMinZ;
      MaxZ = AMaxZ;
    }

    /// <summary>
    /// Creates a new bounding extent, sets its parameters to be inverted and returns the result
    /// </summary>
    /// <returns></returns>
    public static BoundingWorldExtent3D Inverted()
    {
      BoundingWorldExtent3D result = new BoundingWorldExtent3D();
      result.SetInverted();

      return result;
    }

    /// <summary>
    /// Creates a new bounding extent, sets its parameters to be null and returns the result
    /// </summary>
    /// <returns></returns>
    public static BoundingWorldExtent3D Null()
    {
      return new BoundingWorldExtent3D(Consts.NullDouble, Consts.NullDouble, Consts.NullDouble, Consts.NullDouble, Consts.NullDouble, Consts.NullDouble);
    }

    /// <summary>
    /// Creates a new bounding extent, sets its parameters to be the largest extent possible and returns the result
    /// </summary>
    /// <returns></returns>
    public static BoundingWorldExtent3D Full()
    {
      var result = new BoundingWorldExtent3D();
      result.SetMaximalCoverage();
      return result;
    }

    /// <summary>
    /// Expand the plan X/Y extent of the bounding box by the quantities in dx & dy. Expansion is isotropic on both axes.
    /// </summary>
    /// <param name="dx"></param>
    /// <param name="dy"></param>
    public void Expand(double dx, double dy)
    {
      if (IsValidPlanExtent)
      {
        Include(MinX - dx, MinY - dy);
        Include(MaxX + dx, MaxY + dy);
      }
    }

    /// <summary>
    /// Expand the Z extent of the bounding box by the supplied delta. Expansion is isotropic on the Z axis.
    /// </summary>
    /// <param name="dz"></param>
    public void Expand(double dz)
    {
      if (MinZ < MaxZ)
      {
        MinZ = MinZ - dz;
        MaxZ = MaxZ + dz;
      }
    }

    /// <summary>
    /// Extract the X/Y min/max plan extents from the instance into separate variables
    /// </summary>
    /// <param name="AMinX"></param>
    /// <param name="AMinY"></param>
    /// <param name="AMaxX"></param>
    /// <param name="AMaxY"></param>
    public void Extract2DExtents(out double AMinX, out double AMinY, out double AMaxX, out double AMaxY)
    {
      AMinX = MinX;
      AMaxX = MaxX;
      AMinY = MinY;
      AMaxY = MaxY;
    }

    /// <summary>
    /// Extract min/max values for X, Y and Z axes into individual parameters
    /// </summary>
    /// <param name="AMinX"></param>
    /// <param name="AMinY"></param>
    /// <param name="AMaxX"></param>
    /// <param name="AMaxY"></param>
    /// <param name="AMinZ"></param>
    /// <param name="AMaxZ"></param>
    public void Extract3DExtents(out double AMinX, out double AMinY, out double AMaxX, out double AMaxY, out double AMinZ, out double AMaxZ)
    {
      AMinX = MinX;
      AMaxX = MaxX;
      AMinY = MinY;
      AMaxY = MaxY;
      AMinZ = MinZ;
      AMaxZ = MaxZ;
    }

    /// <summary>
    /// Modifies the extents of the bounding box to include the supplied X, Y and Z coordinate
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <param name="Z"></param>
    public void Include(double X, double Y, double Z)
    {
      Include(X, Y);

      if (Z != Consts.NullDouble)
      {
        if (MinZ > Z) MinZ = Z;
        if (MaxZ < Z) MaxZ = Z;
      }
    }

    /// <summary>
    /// Include the extents of another BoundingWorldExtent instance into this one. If the Z ordinate is not valid, or not required, set 
    /// TwoDOnlyValid to true to cause the Z ordinate to be ignored in the calculation
    /// </summary>
    /// <param name="Extent"></param>
    /// <param name="TwoDOnlyValid"></param>
    public void Include(BoundingWorldExtent3D Extent, bool TwoDOnlyValid = false)
    {
      if (!IsValidPlanExtent)
      {
        if (Extent.IsValidPlanExtent)
        {
          MinX = Extent.MinX;
          MinY = Extent.MinY;
          MaxX = Extent.MaxX;
          MaxY = Extent.MaxY;
        }
      }
      else
      {
        if (Extent.IsValidPlanExtent)
        {
          Include(Extent.MinX, Extent.MinY);
          Include(Extent.MaxX, Extent.MaxY);
        }
      }

      if (TwoDOnlyValid)
      {
        return;
      }

      if (!IsValidHeightExtent)
      {
        if (Extent.IsValidHeightExtent)
        {
          MinZ = Extent.MinZ;
          MaxZ = Extent.MaxZ;
        }
      }
      else
      {
        if (Extent.IsValidHeightExtent)
        {
          Include(Extent.MinZ);
          Include(Extent.MaxZ);
        }
      }
    }

    /// <summary>
    /// Modifies the extents of the bounding box to include the supplied Z ordinate
    /// </summary>
    /// <param name="Z"></param>
    public void Include(double Z)
    {
      if (Z != Consts.NullDouble)
      {
        if (MinZ > Z) MinZ = Z;
        if (MaxZ < Z) MaxZ = Z;
      }
    }

    /// <summary>
    /// Determine if the supplied x, y coordinate is within the bounding box
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool Includes(double x, double y) => (x >= MinX) && (x <= MaxX) && (y >= MinY) && (y <= MaxY);

    /// <summary>
    /// Include the supplied x, y coordinate into the bounding box by increasing its size if necessary
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    public void Include(double X, double Y)
    {
      if (X != Consts.NullDouble)
      {
        if (MinX > X) MinX = X;
        if (MaxX < X) MaxX = X;
      }

      if (Y != Consts.NullDouble)
      {
        if (MinY > Y) MinY = Y;
        if (MaxY < Y) MaxY = Y;
      }
    }

    /// <summary>
    /// Compute the 3D bounding box that result from intersecting this bounding box with the one provided in Extent.
    /// </summary>
    /// <param name="Extent"></param>
    /// <returns></returns>
    public BoundingWorldExtent3D Intersect(BoundingWorldExtent3D Extent) => Intersect(Extent.MinX, Extent.MinY, Extent.MaxX, Extent.MaxY);


    /// <summary>
    /// Compute the 3D bounding box that result from intersecting this bounding box with the one described by the min/max X, Y and Z coordinates.
    /// </summary>
    /// <param name="AMinX"></param>
    /// <param name="AMinY"></param>
    /// <param name="AMaxX"></param>
    /// <param name="AMaxY"></param>
    /// <returns></returns>
    public BoundingWorldExtent3D Intersect(double AMinX, double AMinY, double AMaxX, double AMaxY)
    {
      BoundingWorldExtent3D Result = new BoundingWorldExtent3D();
      Result.Clear();

      if (AMinX > MinX)
        Result.MinX = AMinX;
      else
        Result.MinX = MinX;

      if (AMinY > MinY)
        Result.MinY = AMinY;
      else
        Result.MinY = MinY;

      if (AMaxX > MaxX)
        Result.MaxX = MaxX;
      else
        Result.MaxX = AMaxX;

      if (AMaxY > MaxY)
        Result.MaxY = MaxY;
      else
        Result.MaxY = AMaxY;

      return Result;
    }

    /* Intersection of a filter boundary is not implemented in the C# version from the Delphi version below. 
     * Instead, perform a direct intersection from the min/maz x/y coordinates as is done below
    function T3DBoundingWorldExtent.Intersect(const Fence: TObject): T3DBoundingWorldExtent;
    var
      TheFence : TFencePointList;
    begin
      Assert(Assigned(Fence) and(Fence is TFencePointList));

      TheFence := TFencePointList(Fence);
    Result := Intersect(TheFence.MinX, TheFence.MinY, TheFence.MaxX, TheFence.MaxY);
    end;
    */

    /// <summary>
    /// Determine if the extent of the bounding box is maximal in that the bounds are at extreme ranges of the coordinates
    /// </summary>
    public bool IsMaximalPlanConverage => (MinX < -1E99) && (MaxX > 1E99) && (MinY < -1E99) && (MaxY > 1E99);

    /// <summary>
    /// Delegates GetHashCode to the default object hash code
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int GetHashCode(Fence obj) => obj.GetHashCode();

    public override bool Equals(object obj)
    {
      return this == obj || Equals(obj as BoundingWorldExtent3D);
    }

    /// <summary>
    /// Determine if this bounding box is equal to another
    /// </summary>
    /// <param name="Extent"></param>
    /// <returns></returns>
    public bool Equals(BoundingWorldExtent3D Extent) => (MinX == Extent.MinX) && (MinY == Extent.MinY) && (MinZ == Extent.MinZ) &&
                                                        (MaxX == Extent.MaxX) && (MaxY == Extent.MaxY) && (MaxZ == Extent.MaxZ);

    /// <summary>
    /// Determine if the elevation bounds in the bounding box are valid and does not specify a negative interval
    /// </summary>
    public bool IsValidHeightExtent => (MinZ != Consts.NullDouble) && (MaxZ != Consts.NullDouble) && (MaxZ >= MinZ);

    /// <summary>
    /// Determine if the plan extent of the bounding box is not null and does not specify a negative interval.
    /// </summary>
    public bool IsValidPlanExtent => (MaxX != Consts.NullDouble) && (MinX != Consts.NullDouble) &&
                                     (MaxY != Consts.NullDouble) && (MinY != Consts.NullDouble) &&
                                     (MaxX >= MinX) && (MaxY >= MinY);

    /// <summary>
    /// Determine the size of the largest plan dimension (in X or Y) in the bounding box
    /// </summary>
    public double LargestPlanDimension => IsValidPlanExtent ? Math.Max(MaxX - MinX, MaxY - MinY) : Consts.NullDouble;

    /// <summary>
    /// Move (offset) the elevation (Z) range by the supplied z delta
    /// </summary>
    /// <param name="dz"></param>
    public void Offset(double dz)
    {
      MinZ += dz;
      MaxZ += dz;
    }

    /// <summary>
    /// Move (offset) the plan coordinate (X & Y) range by the supplied x and y delta
    /// </summary>
    /// <param name="dx"></param>
    /// <param name="dy"></param>
    public void Offset(double dx, double dy)
    {
      MinX += dx;
      MaxX += dx;
      MinY += dy;
      MaxY += dy;
    }

    /// <summary>
    /// Set the coordinates to in invalid (inverted) coordinate range to indicate this is a null bounding extents
    /// </summary>
    public void SetInverted()
    {
      MinX = 1E100;
      MaxX = -1E100;
      MinY = 1E100;
      MaxY = -1E100;
      MinZ = 1E100;
      MaxZ = -1E100;
    }

    /// <summary>
    /// Set the coordinates to a maximal coordinate range to indicate this is an all encompassing bounding extent
    /// </summary>
    public void SetMaximalCoverage()
    {
      MinX = -1E100;
      MaxX = 1E100;
      MinY = -1E100;
      MaxY = 1E100;
      MinZ = -1E100;
      MaxZ = 1E100;
    }

    /// <summary>
    /// Shrink the elevation range in the extent by the supplied delta. THe change is applied isotropically.
    /// </summary>
    /// <param name="dz"></param>
    public void Shrink(double dz)
    {
      MinZ += dz;
      MaxZ -= dz;
    }

    public void Shrink(double dx, double dy)
    {
      MinX += dx;
      MinY += dy;
      MaxX -= dx;
      MaxY -= dy;
    }

    /// <summary>
    /// Scale the plan size of the boundary by factor with respect to the center of the bounding rectangle
    /// </summary>
    /// <param name="factor"></param>
    public void ScalePlan(double factor)
    {
      double cx = CenterX;
      double cy = CenterY;
      double sx2 = SizeX / 2;
      double sy2 = SizeY / 2;

      MinX = cx - (sx2 * factor);
      MinY = cy - (sy2 * factor);
      MaxX = cx + (sx2 * factor);
      MaxY = cy + (sy2 * factor);
    }

    /// <summary>
    /// Compute the size of the X dimension in the bounding extents
    /// </summary>
    public double SizeX => MaxX - MinX;

    /// <summary>
    /// Compute the size of the Y dimension in the bounding extents
    /// </summary>
    public double SizeY => MaxY - MinY;

    /// <summary>
    /// Compute the size of the Z dimension in the bounding extents
    /// </summary>
    public double SizeZ => MaxZ - MinZ;

    /// <summary>
    /// Writes a binary representation of the state of the bounding extent using the supplied writer
    /// </summary>
    /// <param name="writer"></param>
    public void Write(BinaryWriter writer)
    {
      writer.Write(MinX);
      writer.Write(MinY);
      writer.Write(MinZ);
      writer.Write(MaxX);
      writer.Write(MaxY);
      writer.Write(MaxZ);
    }

    /// <summary>
    /// Reads a binary representation of the state of the bounding extent using the supplied reader
    /// </summary>
    /// <param name="reader"></param>
    public void Read(BinaryReader reader)
    {
      MinX = reader.ReadDouble();
      MinY = reader.ReadDouble();
      MinZ = reader.ReadDouble();
      MaxX = reader.ReadDouble();
      MaxY = reader.ReadDouble();
      MaxZ = reader.ReadDouble();
    }

    /// <summary>
    /// Sets the plan min/max X/Y bounds for this bounding rectangle
    /// </summary>
    /// <param name="minX"></param>
    /// <param name="minY"></param>
    /// <param name="maxX"></param>
    /// <param name="maxY"></param>
    public void Set(double minX, double minY, double maxX, double maxY)
    {
      MinX = minX;
      MinY = minY;
      MaxX = maxX;
      MaxY = maxY;
    }
  }
}
