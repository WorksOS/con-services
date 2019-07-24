﻿using System;
using System.Diagnostics;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.SVL
{
  public class TNFFLineworkPolyLineVertexEntity
  {
    public TNFFLineworkEntity Parent;

    public double X, Y, Z, Chainage;
    public double LeftCrossSlope;
    public double RightCrossSlope;

    private void SetZ(double Value)
    {
      if ((Value > 1E10) && (Value < 9.999E307))
      {
        Debug.Assert(false, "Unexpectedly large, non-null, height assigned to vertex");
      }

      Z = Value;
    }

    // Assign could be public, but only used by Clone method at this point, so hide.
    public virtual void Assign(TNFFLineworkPolyLineVertexEntity VertexEntity)
    {
      X = VertexEntity.X;
      Y = VertexEntity.Y;
      Z = VertexEntity.Z;
      Chainage = VertexEntity.Chainage;
      LeftCrossSlope = VertexEntity.LeftCrossSlope;
      RightCrossSlope = VertexEntity.RightCrossSlope;

      Parent = VertexEntity.Parent;
    }

    public TNFFLineworkPolyLineVertexEntity()
    {
      Parent = null;

      X = Consts.NullDouble;
      Y = Consts.NullDouble;
      Z = Consts.NullDouble;
      Chainage = Consts.NullDouble;

      LeftCrossSlope = Consts.NullDouble;
      RightCrossSlope = Consts.NullDouble;
    }

    public TNFFLineworkPolyLineVertexEntity(TNFFLineworkEntity parent) : this()
    {
      Parent = parent;
    }

    public TNFFLineworkPolyLineVertexEntity(TNFFLineworkEntity parent,
      double x, double y, double z, double chainage) : this(parent)
    {
      X = x;
      Y = y;
      Z = z;
      Chainage = chainage;
    }

    public TNFFLineworkPolyLineVertexEntity Clone()
    {
      var Result = new TNFFLineworkPolyLineVertexEntity(Parent);
      Result.Assign(this);
      return Result;
    }

    public bool HasValidHeight() => Parent.ControlFlag_NullHeightAllowed || (Z != Consts.NullDouble);

//  Procedure SaveToStream(Stream : TStream);
    public void LoadFromStream(BinaryReader reader)
    {
      //base.LoadFromStream(reader);

      NFFUtils.ReadXYZFromStream(reader, out X, out Y, out Z);
      Chainage = reader.ReadDouble();

      LeftCrossSlope = reader.ReadDouble();
      RightCrossSlope = reader.ReadDouble();
    }

    public void SwapCrossSlopes() => MinMax.Swap(ref LeftCrossSlope, ref RightCrossSlope);

    public double GetCrossSlope() => LeftCrossSlope;

    public void SetCrossSlope(double Value)
    {
      LeftCrossSlope = Value;
      RightCrossSlope = -Value;
    }

    public XYZ AsXYZ() => new XYZ(X, Y, Z);

    public void FromXYZ(XYZ Value)
    {
      X = Value.X;
      Y = Value.Y;
      Z = Value.Z;
    }

 //   public bool IsSameAs(TNFFLineworkPolyLineVertexEntity Other)
 //   {
 //     return Math.Abs(X - Other.X) < 1E-12 && Math.Abs(Y - Other.Y) < 1E-12;
 //   }
  }
}
