using System;
using System.Diagnostics;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.SVL.Utilities;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.SVL
{
  public class NFFLineworkArcEntity : NFFStationedLineworkEntity
  {
    public double X1, Y1, Z1;
    public double X2, Y2, Z2;
    public double CX, CY;
    public bool SingleArcEdgePoint;
    public bool WasClockWise;

    public double StartLeftCrossSlope;
    public double StartRightCrossSlope;
    public double EndLeftCrossSlope;
    public double EndRightCrossSlope;

    public NFFLineworkArcTransitDirection TransitDirection;

    // private
    // FTransitDirection is a property used to indicate a concrete direction of
    // 'transit' along the geometry of the arc. This is not persisted to the
    // guidance alignment or background linework geometry exported to a machine
    // as it is not in the defined format for those entities
    // It is the reponsibility of the application to ensure this state is
    // preserved via a related stream or other persistency mechanism.
    // The convention of 'Start' to 'End' is with respect to the WasClockWise
    // adjusted order of the start and end point geometry (ie: it honours the
    // order of the start and end points of the originating arc used to create this
    // arc entity)
    private NFFLineworkArcTransitDirection _transitDirection;

    // Hash value identifying the original end points of the arc.
    // Used in IsSameAs
    private double OriginalEndPointsHash;

    protected override double GetEndStation()
    {
      double result = Consts.NullDouble;

      if (StartStation != Consts.NullDouble)
      {
        // Work out the bits of geometry we need for the NFF arc
        double relX = X1 - CX;
        double relY = Y1 - CY;
        double endX = X2 - CX;
        double endY = Y2 - CY;
        double radius = Math.Sqrt(relX * relX + relY * relY);
        double start_angle = Math.Atan2(relY, relX);
        double end_angle = Math.Atan2(endY, endX);

        result = StartStation + Math.Abs(NFFUtils.NormalizeDeflection(end_angle - start_angle) * radius);
      }

      return result;
    }

    protected override double GetVertexElevation(int VertexNum)
    {
      if (VertexNum == 0)
        return GetStartTransitPoint().Z;

      if (VertexNum == 1)
        return GetEndTransitPoint().Z;

      throw new TRexException("Invalid vertex index in NFFLineworkArcEntity.GetVertexElevation");
    }

    protected override void SetVertexElevation(int VertexNum,
      double Value)
    {
      if (VertexNum == 0)
      {
        var Pt = GetStartTransitPoint();
        Pt.Z = Value;
        SetStartTransitPoint(Pt);
      }
      else if (VertexNum == 1)
      {
        var Pt = GetEndTransitPoint();
        Pt.Z = Value;
        SetEndTransitPoint(Pt);
      }
      else
      {
        throw new TRexException("Invalid vertex index in NFFLineworkArcEntity.SetVertexElevation");
      }
    }

    protected override double GetVertexStation(int VertexNum)
    {
      if (VertexNum == 0)
        return GetStartStation();
      if (VertexNum == 1)
        return GetEndStation();

      throw new TRexException("Invalid vertex index in NFFLineworkArcEntity.GetVertexStation");
    }

    protected override void SetVertexStation(int VertexNum, double Value)
    {
      if (VertexNum == 0)
        SetStartStation(Value);
      else if (VertexNum == 1)
      {
      } // Ignore it - end stationing controlled by element geometry
      else
      {
        throw new TRexException("Invalid vertex index in NFFLineworkArcEntity.setVertexStation");
      }
    }

    /*
       function GetVertexLeftCrossSlope(VertexNum: Integer): Double; Override;
       procedure SetVertexLeftCrossSlope(VertexNum: Integer;
       const Value: Double); Override;

       function GetVertexRightCrossSlope(VertexNum: Integer): Double; Override;
       procedure SetVertexRightCrossSlope(VertexNum: Integer;
       const Value: Double); Override;
   */

    public NFFLineworkArcEntity()
    {
      ElementType = NFFLineWorkElementType.kNFFLineWorkArcElement;

      StartLeftCrossSlope = Consts.NullDouble;
      StartRightCrossSlope = Consts.NullDouble;
      EndLeftCrossSlope = Consts.NullDouble;
      EndRightCrossSlope = Consts.NullDouble;

      // By definition, an arc in an NFF/SVL file is define as clockwise. Unless
      // an outside agent independently determines that the arc was in an anticlockwise
      // sense when the arc was constructed we must assume the arc was clockwise in
      // orientation
      WasClockWise = true;

      TransitDirection = NFFLineworkArcTransitDirection.atdStartToEnd;

      OriginalEndPointsHash = Consts.NullDouble;
    }

    public NFFLineworkArcEntity(int AColour,
      double AX1, double AY1, double AZ1, double AX2, double AY2, double AZ2,
      double ACX, double ACY, double ACZ,
      bool AClockwise,
      bool ASingleArcEdgePoint) : this()
    {
      Colour = AColour;

      X1 = AX1;
      Y1 = AY1;
      Z1 = AZ1;
      X2 = AX2;
      Y2 = AY2;
      Z2 = AZ2;
      CX = ACX;
      CY = ACY;
      //  CZ = ACZ;

      if (!AClockwise)
      {
        MinMax.Swap(ref X1, ref X2);
        MinMax.Swap(ref Y1, ref Y2);
        MinMax.Swap(ref Z1, ref Z2);
      }

      WasClockWise = AClockwise;
      SingleArcEdgePoint = ASingleArcEdgePoint;

      if (AClockwise)
        TransitDirection = NFFLineworkArcTransitDirection.atdStartToEnd;
      else
        TransitDirection = NFFLineworkArcTransitDirection.atdEndToStart;

      OriginalEndPointsHash = X1 + (Y1 * 2) + (X2 * 4) + (Y2 * 8) + (CX * 16) + (CY * 32);
    }

    public override void Assign(NFFLineworkEntity Entity)
    {
      base.Assign(Entity);

      var ArcEty = Entity as NFFLineworkArcEntity;

      X1 = ArcEty.X1;
      Y1 = ArcEty.Y1;
      Z1 = ArcEty.Z1;
      X2 = ArcEty.X2;
      Y2 = ArcEty.Y2;
      Z2 = ArcEty.Z2;
      CX = ArcEty.CX;
      CY = ArcEty.CY;
      //  CZ                   = ArcEty.CZ;
      WasClockWise = ArcEty.WasClockWise;
      SingleArcEdgePoint = ArcEty.SingleArcEdgePoint;
      StartLeftCrossSlope = ArcEty.StartLeftCrossSlope;
      StartRightCrossSlope = ArcEty.StartRightCrossSlope;
      EndLeftCrossSlope = ArcEty.EndLeftCrossSlope;
      EndRightCrossSlope = ArcEty.EndRightCrossSlope;

      TransitDirection = ArcEty.TransitDirection;
    }

    // Procedure SaveToStream(Stream : TStream); Override;

    /*
    public override void LoadFromStream(BinaryReader reader)
    {
      base.LoadFromStream(reader);

      NFFUtils.ReadXYZFromStream(reader, out X1, out Y1, out Z1);
      NFFUtils.ReadXYZFromStream(reader, out X2, out Y2, out Z2);
      NFFUtils.ReadXYFromStream(reader, out CX, out CY);
      SingleArcEdgePoint = reader.ReadBoolean();
      WasClockWise = reader.ReadBoolean();

      byte ReadByte = reader.ReadByte();
      if (Range.InRange(ReadByte, (byte)NFFLineworkArcTransitDirection.atdUnknown, (byte)NFFLineworkArcTransitDirection.atdEndToStart))
      TransitDirection = (NFFLineworkArcTransitDirection)ReadByte;
      else
      TransitDirection = NFFLineworkArcTransitDirection.atdUnknown;

      OriginalEndPointsHash = reader.ReadDouble();

      StartLeftCrossSlope= reader.ReadDouble();
      EndLeftCrossSlope = reader.ReadDouble();
      StartRightCrossSlope = reader.ReadDouble();
      EndRightCrossSlope = reader.ReadDouble();
    }
    */

    // procedure DumpToText(Stream: TTextDumpStream; const OriginX, OriginY : Double); override;
    //Procedure SaveToNFFStream(Stream : TStream;
    //const OriginX, OriginY : Double;
    //                          FileVersion : NFFFileVersion); Override;

    public override void LoadFromNFFStream(BinaryReader reader,
      double OriginX, double OriginY,
      bool HasGuidanceID,
      NFFFileVersion FileVersion)
    {
      // There is no need to read the entity type as this will have already been
      // read in order to determine we should be reading this type of entity!

      if (HasGuidanceID)
        _guidanceID = reader.ReadUInt16();

      byte _ = reader.ReadByte(); //{ LineWidth= }

      Colour = NFFUtils.ReadColourFromStream(reader);
      _entityFlags = reader.ReadByte();

      NFFUtils.ReadRectFromStream(reader, out double MinX, out double MinY, out double MaxX, out double MaxY, OriginX, OriginY);

      NFFUtils.ReadCoordFromStream(reader, out CX, out CY, OriginX, OriginY);

      //  CZ = NullReal;
      //SPR 8763: Remove height field from arcs in SVL files
      //  if (FFlags and kNFFElementHeaderHasElevation) <> 0 then
      //    ReadFixedPoint32FromStream(Stream);

      NFFUtils.ReadCoordFromStream(reader, out X1, out Y1, OriginX, OriginY);
      if ((HeaderFlags & NFFConsts.kNFFElementHeaderHasElevation) != 0)
        Z1 = NFFUtils.ReadFixedPoint32FromStream(reader);
      else
        Z1 = Consts.NullDouble;

      if ((HeaderFlags & NFFConsts.kNFFElementHeaderHasCrossSlope) != 0)
        NFFUtils.ReadCrossSlopeInformationFromStream(reader, out StartLeftCrossSlope, out StartRightCrossSlope);
      else
      {
        StartLeftCrossSlope = Consts.NullDouble;
        StartRightCrossSlope = Consts.NullDouble;
      }

      NFFUtils.ReadCoordFromStream(reader, out X2, out Y2, OriginX, OriginY);
      if ((HeaderFlags & NFFConsts.kNFFElementHeaderHasElevation) != 0)
        Z2 = NFFUtils.ReadFixedPoint32FromStream(reader);
      else
        Z2 = Consts.NullDouble;

      if ((HeaderFlags & NFFConsts.kNFFElementHeaderHasCrossSlope) != 0)
        NFFUtils.ReadCrossSlopeInformationFromStream(reader, out EndLeftCrossSlope, out EndRightCrossSlope);
      else
      {
        EndLeftCrossSlope = Consts.NullDouble;
        EndRightCrossSlope = Consts.NullDouble;
      }

      double start_angle = reader.ReadSingle();
      double end_angle = reader.ReadSingle();
      double radius = reader.ReadSingle();

      if ((_headerFlags & NFFConsts.kNFFElementHeaderHasStationing) != 0)
      {
        // Read the Start Station
        StartStation = reader.ReadDouble();

        // Read and discard EndStation value (it can be re-calculated any time it is needed)
        var TempEndStation = reader.ReadDouble();

        // Within SVD/ SVL no record is kept of whether arc was initially described as
        //   CW or CCW, Stationing is stored with convention that arc is CW.When
        //   creating / reading a arc entity from stream, we need to figure out and restore
        //      <WasClockWise> value in order for any subsequent write to stream to write

        //     Stationing around the right way 
        if (StartStation > TempEndStation)
        {
          // Arc was initially defined CCW
          WasClockWise = false;
          StartStation = TempEndStation;
          TransitDirection = NFFLineworkArcTransitDirection.atdEndToStart;
        }
        else
        {
          WasClockWise = true;
          TransitDirection = NFFLineworkArcTransitDirection.atdStartToEnd;
        }
      }

      // Convert the start/end angle into end points. Even though we have
      // read in the end points, we recalculate then to undo loss of precision when written
      // into the NFF file
      // Angles are mathematical angles, PolarToRect wants azimuth angles

      GeometryUtils.PolarToRect(CY, CX, out Y1, out X1,
        Math.PI / 2 - start_angle,
        radius);
      GeometryUtils.PolarToRect(CY, CX, out Y2, out X2,
        Math.PI / 2 - end_angle,
        radius);
    }

    public override BoundingWorldExtent3D BoundingBox() => ArcUtils.ArcBoundingRectangle(X1, Y1, X2, Y2, CX, CY, true, true); // ###Check??? ClockwiseCoordSystem : Boolean;

    public override bool HasValidHeight()
    {
      return ControlFlag_NullHeightAllowed ||
             ((Z1 != Consts.NullDouble) && (Z2 != Consts.NullDouble)) /* && (CZ != Consts.NullDouble))*/;
    }

    public override XYZ GetStartPoint() => new XYZ(X1, Y1, Z1);
    public override XYZ GetEndPoint() => new XYZ(X2, Y2, Z2);

    public override void SetStartPoint(XYZ Value)
    {
      X1 = Value.X;
      Y1 = Value.Y;
      Z1 = Value.Z;
    }

    public override void SetEndPoint(XYZ Value)
    {
      X2 = Value.X;
      Y2 = Value.Y;
      Z2 = Value.Z;
    }

    public override XYZ GetStartTransitPoint()
    {
      switch (TransitDirection)
      {
        case NFFLineworkArcTransitDirection.atdUnknown:
          return GetStartPoint();
        case NFFLineworkArcTransitDirection.atdStartToEnd:
          return GetStartPoint();
        case NFFLineworkArcTransitDirection.atdEndToStart:
          return GetEndPoint();
        default:
          return new XYZ();
      }
    }

    public override XYZ GetEndTransitPoint()
    {
      switch (TransitDirection)
      {
        case NFFLineworkArcTransitDirection.atdUnknown:
          return GetEndPoint();
        case NFFLineworkArcTransitDirection.atdStartToEnd:
          return GetEndPoint();
        case NFFLineworkArcTransitDirection.atdEndToStart:
          return GetStartPoint();
        default:
          return new XYZ();
      }
    }

    public override void SetStartTransitPoint(XYZ Value)
    {
      switch (TransitDirection)
      {
        case NFFLineworkArcTransitDirection.atdUnknown:
          SetStartPoint(Value);
          break;
        case NFFLineworkArcTransitDirection.atdStartToEnd:
          SetStartPoint(Value);
          break;
        case NFFLineworkArcTransitDirection.atdEndToStart:
          SetEndPoint(Value);
          break;
      }
    }

    public override void SetEndTransitPoint(XYZ Value)
    {
      switch (TransitDirection)
      {
        case NFFLineworkArcTransitDirection.atdUnknown:
          SetEndPoint(Value);
          break;
        case NFFLineworkArcTransitDirection.atdStartToEnd:
          SetEndPoint(Value);
          break;
        case NFFLineworkArcTransitDirection.atdEndToStart:
          SetStartPoint(Value);
          break;
      }
    }

    // Function GetStartCrossSlope : Double; Override;
    // Function GetEndCrossSlope : Double; Override;

    // procedure SetStartCrossSlope(Value : Double); Override;
    // procedure SetEndCrossSlope(Value : Double); Override;

    public override void Reverse()
    {
      MinMax.Swap(ref StartLeftCrossSlope, ref EndRightCrossSlope);
      MinMax.Swap(ref StartRightCrossSlope, ref EndLeftCrossSlope);

      switch (TransitDirection)
      {
        case NFFLineworkArcTransitDirection.atdUnknown: // do nothing
          break;
        case NFFLineworkArcTransitDirection.atdStartToEnd:
          TransitDirection = NFFLineworkArcTransitDirection.atdEndToStart;
          break;
        case NFFLineworkArcTransitDirection.atdEndToStart:
          TransitDirection = NFFLineworkArcTransitDirection.atdStartToEnd;
          break;
      }
    }

    public double Radius()
    {
      double Result = MathUtilities.Hypot(CX - X1, CY - Y1);

      switch (TransitDirection)
      {
        case NFFLineworkArcTransitDirection.atdUnknown:
        case NFFLineworkArcTransitDirection.atdStartToEnd:
          // It's a clockwise turning arc
          break;
        case NFFLineworkArcTransitDirection.atdEndToStart:
          Result = -Result;
          TransitDirection = NFFLineworkArcTransitDirection.atdStartToEnd;
          break;
      }

      return Result;
    }

    // TransitDirectionIsCW determines if the transit direction along the arc
    // is turning clockwise
    public bool TransitDirectionIsCW()
    {
      switch (TransitDirection)
      {
        case NFFLineworkArcTransitDirection.atdUnknown: // do nothing
          return true;
        case NFFLineworkArcTransitDirection.atdStartToEnd:
          return true;
        case NFFLineworkArcTransitDirection.atdEndToStart:
          return false;
        default:
          throw new TRexException("Unknown transit direction");
      }
    }

    public override void ComputeStnOfs(double X, double Y, out double Stn, out double Ofs)
    {
      Stn = Consts.NullDouble;
      Ofs = Consts.NullDouble;

      XYZ StartTransitPoint = GetStartTransitPoint();
      XYZ EndTransitPoint = GetEndTransitPoint();

      double bearing1 = Math.Atan2(StartTransitPoint.X - CX, StartTransitPoint.Y - CY);
      double bearing2 = Math.Atan2(EndTransitPoint.X - CX, EndTransitPoint.Y - CY);
      double testBearing = Math.Atan2(X - CX, Y - CY);

      if (!TransitDirectionIsCW())
        MinMax.Swap(ref bearing1, ref bearing2);

      if (bearing2 < bearing1)
        bearing2 += (2 * Math.PI);

      if (!GeometryUtils.BetweenAngle(bearing1, testBearing, bearing2))
        testBearing += (2 * Math.PI);

      // Calculate the station
      if (GeometryUtils.BetweenAngle(bearing1, testBearing, bearing2))
      {
        double Angle = (testBearing < bearing1 ? testBearing + 2 * Math.PI : testBearing) - bearing1;

        if (Angle < 0)
        {
          Debug.Assert(Angle > -0.001, "Probable arc element transit angle calculation error in NFFLineworkArcEntity.ComputeStnOfs");
          Angle = 0;
        }

        var radius = Radius();
        Stn = Angle * Math.Abs(radius);

        // Calculate the offset
        var RadialDistance = MathUtilities.Hypot(X - CX, Y - CY);
        Ofs = Math.Abs(radius) - RadialDistance;

        var elementLength = ElementLength();
        if (!TransitDirectionIsCW())
        {
          Ofs = -Ofs;
          Stn = elementLength - Stn;
        }

        if (Stn > -0.0001 && Stn < elementLength + 0.0001)
          Stn = StartStation + Stn;
        else
          Stn = Consts.NullDouble;
      }

      //  writeln(LogFile, Format('[Arc] Calcing Stn:Ofs from %.4f/%.4f [Result: Stn=%.4f, Ofs=%.4f]', {SKIP}
      //                          [X, Y, Stn, Ofs]));
    }

    public override void ComputeXY(double Stn, double Ofs, out double X, out double Y)
    {
      // Calculate the deflection angle to the point on the curve
      var Deflection = (Stn - StartStation) / Math.Abs(Radius());

      GeometryUtils.RectToPolar(CY, CX, GetStartTransitPoint().Y, GetStartTransitPoint().X, out double InitialBrng, out double Dist);

      double Bearing = TransitDirectionIsCW() ? InitialBrng + Deflection : InitialBrng - Deflection;

      GeometryUtils.CleanAngle(ref Bearing);

      if (TransitDirectionIsCW())
        GeometryUtils.PolarToRect(CY, CX, out Y, out X, Bearing, Math.Abs(Radius()) - Ofs);
      else
        GeometryUtils.PolarToRect(CY, CX, out Y, out X, Bearing, Math.Abs(Radius()) + Ofs);

//  writeln(LogFile, Format('[Arc] Calcing XY from %.4f/%.4f, [Result: X=%.4f, Y=%.4f]', {SKIP}
//                          [stn, ofs, X, Y]));
    }

    // IncludedAngle calculates the included angle subtended by the
    // arc. The result in in radians and by definition is a clockwise
    // turning angle consistent with the clockwise direction or arcs in
    // SVL
    public double IncludedAngle()
    {
      switch (TransitDirection)
      {
        case NFFLineworkArcTransitDirection.atdUnknown:
        case NFFLineworkArcTransitDirection.atdStartToEnd:
          return ArcUtils.CalcIncludedAngle(X1, Y1, X2, Y2, CX, CY, true);
        case NFFLineworkArcTransitDirection.atdEndToStart:
          return ArcUtils.CalcIncludedAngle(X2, Y2, X1, Y1, CX, CY, false);
        default:
          throw new TRexException("Unknown transit direction");
      }
    }

//    Procedure UpdateHeight(const UpdateIfNullOnly : Boolean;
//  const Position : TXYZ;
//                           const Station : Double;
//                           const Index : Integer); Override;

    public override double ElementLength() => Math.Abs(IncludedAngle() * Radius());

//    procedure SetDefaultStationing(const AStartStation : Double;
//  AIndex : Integer); Override;

//    Function IsSameAs(const Other : NFFLineworkEntity) : Boolean; Override;
  }
}
