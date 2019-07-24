using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.SVL
{
  public class TNFFLineworkPolyLineEntity : TNFFStationedLineworkEntity
  { 
    public List<TNFFLineworkPolyLineVertexEntity> Vertices; // TNFFLineworkPolyLineVertexEntityList;

    public TNFFLineworkPolyLineEntity()
    {
      ElementType = TNFFLineWorkElementType.kNFFLineWorkPolyLineElement;
      Vertices = new List<TNFFLineworkPolyLineVertexEntity>();
      //Vertices.Parent = this;
    }

    protected override double GetStartStation()
    {
      double result = Consts.NullDouble;

      if (Vertices.Count > 0)
      {
        result = Vertices.First().Chainage;
      }

      return result;
    }

    protected override void SetStartStation(double Value)
    {
      // Illegal to set StartStation as it is determined by the chainage of the first Vertex
      // Ignore it...
    }

    protected override double GetEndStation()
    {
      double result = Consts.NullDouble;
      if (Vertices.Count > 0)
        result = Vertices.Last().Chainage;
      return result;
    }

    protected override double GetVertexElevation(int VertexNum)
    {
      Debug.Assert(Range.InRange(VertexNum, 0, Vertices.Count - 1),
        "VertexNum out of range in TNFFLineworkPolyLineEntity.GetVertexElevation");

      return Vertices[VertexNum].Z;
    }

    protected override void SetVertexElevation(int VertexNum, double Value)
    {
      Debug.Assert(Range.InRange(VertexNum, 0, Vertices.Count - 1),
        "VertexNum out of range in TNFFLineworkPolyLineEntity.SetVertexElevation");

      Vertices[VertexNum].Z = Value;
    }

    protected override double GetVertexStation(int VertexNum)
    {
      Debug.Assert(Range.InRange(VertexNum, 0, Vertices.Count - 1),
        "VertexNum out of range in TNFFLineworkPolyLineEntity.GetVertexStation");

      return Vertices[VertexNum].Chainage;
    }

    protected override void SetVertexStation(int VertexNum, double Value)
    {
      Debug.Assert(Range.InRange(VertexNum, 0, Vertices.Count - 1),
        "VertexNum out of range in TNFFLineworkPolyLineEntity.SetVertexStation");

      Vertices[VertexNum].Chainage = Value;
    }

    /*
    function GetVertexLeftCrossSlope(VertexNum: Integer): Double; Override;
    procedure SetVertexLeftCrossSlope(VertexNum: Integer;
    const Value: Double); Override;

    function GetVertexRightCrossSlope(VertexNum: Integer): Double; Override;
    procedure SetVertexRightCrossSlope(VertexNum: Integer;
    const Value: Double); Override;
    */

    public override void Assign(TNFFLineworkEntity Entity)
    {
      base.Assign(Entity);

      Vertices.Clear();

      for (int I = 0; I < (Entity as TNFFLineworkPolyLineEntity).Vertices.Count; I++)
        Vertices.Add((Entity as TNFFLineworkPolyLineEntity).Vertices[I].Clone());

      for (int I = 0; I < Vertices.Count; I++)
        Vertices[I].Parent = this;
    }

    //    procedure DumpToText(Stream: TTextDumpStream; const OriginX, OriginY : Double); override;
    //    Procedure SaveToNFFStream(Stream : TStream;
    //    const OriginX, OriginY : Double;
    //                              FileVersion : TNFFFileVersion); Override;

    public override void LoadFromNFFStream(BinaryReader reader,
      double OriginX, double OriginY,
      bool HasGuidanceID,
      TNFFFileVersion FileVersion)
    {
      // There is no need to read the entity type as this will have already been
      // read in order to determine we should be reading this type of entity!

      if (HasGuidanceID)
        GuidanceID = reader.ReadInt16();

      // Linewidth is always 1
      byte _ = reader.ReadByte(); // LineWidth:= 

      Colour = NFFUtils.ReadColourFromStream(reader);
      EntityFlags = reader.ReadByte();

      //Read the bounding box
      NFFUtils.ReadRectFromStream(reader, out double MinX, out double MinY, out double MaxX, out double MaxY, OriginX, OriginY);

      // Read in number of points
      var EntCount = reader.ReadUInt16();

      // Read the vertices constructing line elements in the polygon entity
      for (int I = 1; I < EntCount; I++)
      {
        NFFUtils.ReadCoordFromStream(reader, out double X1, out double Y1, OriginX, OriginY);
        double Z1;
        if ((HeaderFlags & NFFConsts.kNFFElementHeaderHasElevation) != 0)
          Z1 = NFFUtils.ReadFixedPoint32FromStream(reader);
        else
          Z1 = Consts.NullDouble;

        double FLeftCrossSlope;
        double FRightCrossSlope;
        if ((HeaderFlags & NFFConsts.kNFFElementHeaderHasCrossSlope) != 0)
          NFFUtils.ReadCrossSlopeInformationFromStream(reader, out FLeftCrossSlope, out FRightCrossSlope);
        else
        {
          FLeftCrossSlope = Consts.NullDouble;
          FRightCrossSlope = Consts.NullDouble;
        }

        double Chainage;
        if ((HeaderFlags & NFFConsts.kNFFElementHeaderHasStationing) != 0)
          Chainage = reader.ReadDouble();
        else
          Chainage = Consts.NullDouble;

        Vertices.Add(new TNFFLineworkPolyLineVertexEntity(this, X1, Y1, Z1, Chainage));
        Vertices.Last().LeftCrossSlope = FLeftCrossSlope;
        Vertices.Last().RightCrossSlope = FRightCrossSlope;
        Vertices.Last().Parent = this;
      }

    }

    //    Procedure SaveToStream(Stream : TStream); override;
    //    Procedure LoadFromStream(Stream : TStream); override;

    public override BoundingWorldExtent3D BoundingBox()
    {
      if (Vertices.Count == 0)
        return new BoundingWorldExtent3D(Consts.NullDouble, Consts.NullDouble, Consts.NullDouble, Consts.NullDouble);

      if (Vertices.Count == 1)
        return new BoundingWorldExtent3D(Vertices.First().X, Vertices.First().Y, Vertices.First().X, Vertices.First().Y);

      var Result = new BoundingWorldExtent3D(Math.Min(Vertices[0].X, Vertices[1].X),
        Math.Min(Vertices[0].Y, Vertices[1].Y),
        Math.Max(Vertices[0].X, Vertices[1].X),
        Math.Max(Vertices[0].Y, Vertices[1].Y));

      for (int I = 2; I < Vertices.Count; I++)
        Result.Include(Vertices[I].X, Vertices[I].Y);

      return Result;
    }

    public override bool HasValidHeight()
    {
      if (ControlFlag_NullHeightAllowed)
        return false;

      for (int I = 0; I < Vertices.Count; I++)
        if (!Vertices[I].HasValidHeight())
          return false;

      return true;
    }

    public override XYZ GetStartPoint()
    {
      if (Vertices.Count == 0)
        return base.GetStartPoint();

      return Vertices.First().AsXYZ();
    }

    public override XYZ GetEndPoint()
    {
      if (Vertices.Count == 0)
        return base.GetStartPoint();

      return Vertices.Last().AsXYZ();
    }

    public override void SetStartPoint(XYZ Value)
    {
      if (Vertices.Count > 0)
      {
        Vertices.First().FromXYZ(Value);
      }
    }

    public override void SetEndPoint(XYZ Value)
    {
      if (Vertices.Count > 0)
      {
        Vertices.Last().FromXYZ(Value);
      }
    }

    public override void Reverse() => throw new Exception("Reverse not implemented");

    public override void ComputeStnOfs(double X, double Y, out double Stn, out double Ofs)
    {
      Stn = Consts.NullDouble;
      Ofs = Consts.NullDouble;

      int ClosestVertex = -1;
      double ClosestDistance = 1E99;

      // Locate the interval in the polyline with the closest match
      for (int I = 0; I < Vertices.Count - 1; I++)
      {
        var Distance = GeometryUtils.DistToLine(X, Y,
          Vertices[I].X, Vertices[I].Y,
          Vertices[I + 1].X, Vertices[I + 1].Y);

        if (Math.Abs(Distance) < Math.Abs(ClosestDistance))
        {
          ClosestVertex = I;
          ClosestDistance = Distance;
        }
      }

      if (ClosestVertex == -1)
        return;

      var Length = MathUtilities.Hypot(Vertices[ClosestVertex].X - Vertices[ClosestVertex + 1].X,
        Vertices[ClosestVertex].Y - Vertices[ClosestVertex + 1].Y);

      GeometryUtils.LineClosestPoint(X, Y,
        Vertices[ClosestVertex].X, Vertices[ClosestVertex].Y,
        Vertices[ClosestVertex + 1].X, Vertices[ClosestVertex + 1].Y,
        out double _x, out double _y, out double _Chainage, out double _Offset);

      if (_Chainage > -0.0001 && _Chainage < (Length + 0.0001) && Vertices[ClosestVertex].Chainage != Consts.NullDouble)
      {
        Stn = Vertices[ClosestVertex].Chainage + _Chainage;
        Ofs = _Offset;
      }
    }

    public override void ComputeXY(double Stn, double Ofs, out double X, out double Y)
    {
      int index = -1;
      X = Consts.NullDouble;
      Y = Consts.NullDouble;

      // Locate the interval in the polyline the station value refers to.
      for (int I = 0; I < Vertices.Count - 1; I++)
        if ((Vertices[I].Chainage <= (Stn + 0.0001)) &&
            (Vertices[I + 1].Chainage > (Stn - 0.0001)))
        {
          index = I;
          break;
        }

      if (index == -1)
        return;

      // Compute bearing of segment
      NFFUtils.rect_to_polar(Vertices[index].Y, Vertices[index].X,
        Vertices[index + 1].Y, Vertices[index + 1].X, out double bearing, out double length);

      // Compute the plan position for this segment;
      var distance = (Stn - Vertices[index].Chainage);

      NFFUtils.polar_to_rect(Vertices[index].Y, Vertices[index].X, out Y, out X, bearing, distance);
      NFFUtils.polar_to_rect(Y, X, out Y, out X, bearing + (Math.PI / 2), Ofs);
    }

    //    procedure ResetStartStation(const NewStartStation : Double); Override;

    //    Procedure UpdateHeight(const UpdateIfNullOnly : Boolean;
    //                           const Position : TXYZ;
    //                           const Station : Double;
    //                           const Index : Integer); Override;

    public override double ElementLength()
    {
      double Result = 0;

      for (int I = 0; I < Vertices.Count - 1; I++)
         Result += MathUtilities.Hypot(Vertices[I].X - Vertices[I + 1].X,
        Vertices[I].Y - Vertices[I + 1].Y);

      return Result;
    }

    public override double ElementLength(int Index)
    {
      if (!Range.InRange(Index, 0, Vertices.Count - 1))
      {
        Debug.Assert(false, "Out of range vertex index in TNFFLineworkPolyLineEntity.ElementLength");
        return 0;
      }

      if (Index == Vertices.Count - 1)
      {
        return 0;
      }

      return MathUtilities.Hypot(Vertices[Index].X - Vertices[Index + 1].X,
        Vertices[Index].Y - Vertices[Index + 1].Y);
    }

    //    procedure SetDefaultStationing(const AStartStation : Double;
    // AIndex : Integer); Override;

    public override bool HasInternalStructure() => false;

    public override bool HasVertexAtStation(double AStation, double Tolerance)
    {
      for (int I = 0; I < Vertices.Count; I++)
        if (Vertices[I].Chainage != Consts.NullDouble && Math.Abs(Vertices[I].Chainage - AStation) < Tolerance)
          return true;

      return false;
    }

    //    Function IsSameAs(const Other : TNFFLineworkEntity) : Boolean; Override;

    public override int VertexCount() => Vertices.Count;

    public override TNFFLineworkPolyLineVertexEntity GetVertex(int VertexNum)
    {
      Debug.Assert(Range.InRange(VertexNum, 0, Vertices.Count - 1), "Vertex index out of range"); 

      return Vertices[VertexNum];
    }

//Procedure InsertVertex(Vertex : TNFFLineworkPolyLineVertexEntity;
//    InsertAt : Integer); Override;

    // CreateVertexAtStation creates a new vertex at the requested station. The station value must
    // lie between the station values of two surrounding vertices. The other values for the vertex are
    // calculated from those of the surrounding vertices.
//    Function CreateVertexAtStation(const Chainage : Double) : TNFFLineworkPolyLineVertexEntity; Override;
//    function InsertVertexAtStation(const Chainage : Double): Integer;

  //  Function CreateNewVertex : TNFFLineworkPolyLineVertexEntity; Override;

//    Function InMemorySize : Longint; Override;

  }
}
