﻿using System;
using System.IO;
using System.Linq;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.SVL.Utilities
{
  public static class NFFUtils
  {
    // This function converts an ordinate expressed in an integer number
    // of 0.1 steps to a real value
    public static double ConvertIntegerOrdinateToRealOrdinate(int Num)
    {
      double Result = Consts.NullDouble;

      if (Math.Abs(Num) != int.MaxValue)
        Result = Num / 10000.0;

      return Result;
    }

    public static void ReadXYZFromStream(BinaryReader reader, out double X, out double Y, out double Z)
    {
      X = reader.ReadDouble();
      Y = reader.ReadDouble();
      Z = reader.ReadDouble();
    }

    public static void ReadXYFromStream(BinaryReader reader, out double X, out double Y)
    {
      X = reader.ReadDouble();
      Y = reader.ReadDouble();
    }

    public static int ReadColourFromStream(BinaryReader reader)
    {
      byte _ = reader.ReadByte();
      return 0; // We do not care about colours for this implementation

      // Raptor implementation
      // Result = NFFToSVOColourMap[ReadByteFromStream(Stream)];
    }

    public static double ReadFixedPoint32FromStream(BinaryReader reader)
    {
      int FixedPoint32Value = reader.ReadInt32();
      return ConvertIntegerOrdinateToRealOrdinate(FixedPoint32Value);
    }

    public static void ReadCoordFromStream(BinaryReader reader,
      out double X, out double Y,
      double OriginX, double OriginY)
    {
      X = OriginX + ConvertIntegerOrdinateToRealOrdinate(reader.ReadInt32());
      Y = OriginY + ConvertIntegerOrdinateToRealOrdinate(reader.ReadInt32());
    }

    public static void ReadRectFromStream(BinaryReader reader,
      out double MinX, out double MinY, out double MaxX, out double MaxY,
      double OriginX, double OriginY)
    {
      ReadCoordFromStream(reader, out MinX, out MinY, OriginX, OriginY);
      ReadCoordFromStream(reader, out MaxX, out MaxY, OriginX, OriginY);
    }

    public static void ReadCrossSlopeInformationFromStream(BinaryReader reader,
      out double LeftCrossSlope, out double RightCrossSlope)
    {
      float SingleLeftSlope = reader.ReadSingle();
      float SingleRightSlope = reader.ReadSingle();

      if (SingleLeftSlope == NFFConsts.kNFFNullCrossSlopeValue)
        LeftCrossSlope = Consts.NullDouble;
      else
        LeftCrossSlope = SingleLeftSlope;

      if (SingleRightSlope == NFFConsts.kNFFNullCrossSlopeValue)
        RightCrossSlope = Consts.NullDouble;
      else
        RightCrossSlope = SingleRightSlope;
    }

    public static string ReadWideStringFromStream(BinaryReader reader)
    {
      int len = reader.ReadUInt16();
      if (len > 10000)
        throw new Exception($"{len} length wide string encountered (want < 10000), bailing");

      var bytes = reader.ReadBytes(2 * len);
      return System.Text.Encoding.Unicode.GetString(bytes);
    }

    public static double NormalizeRadians(double Angle)
    {
      if (Angle != Consts.NullDouble && (Angle < 0 || Angle >= 2 * Math.PI))
      {
        double Multiples = (int) Math.Truncate(Angle / (2 * Math.PI));
        if (Angle < 0)
          Multiples = Multiples - 1;
        Angle -= Multiples * (2 * Math.PI);
      }

      return Angle;
    }

    public static double NormalizeDeflection(double Angle)
    {
      // Returns a value -Pi<=Result<Pi 

      double result = NormalizeRadians(Angle);
      if (result >= Math.PI)
        result -= (2 * Math.PI);

      return result;
    }

    public static void DistanceToNFFCurve(double CurveSegmentStartX, double CurveSegmentStartY,
      double CurveSegmentEndX, double CurveSegmentEndY,
      double PointToTestX, double PointToTestY,
      double Alpha, double Beta,
      out double Offset,
      out double T)
    {
      const double Epsylon = 0.00000001;

      try
      {
        double LineVectorX = CurveSegmentEndX - CurveSegmentStartX;
        double LineVectorY = CurveSegmentEndY - CurveSegmentStartY;

        double VectorLength2 = (LineVectorX * LineVectorX) + (LineVectorY * LineVectorY);
        double VectorLength = Math.Sqrt(VectorLength2);

        // If VectorLength2 is very small, then fail this calculation as the azimuthal
        // calculations will become degnerate and give offset values that are not useful.
        // Failure of the calculation is indication by a null calculated offset value
        // and a T value of 2 (where 0 <= T <= 1 incidates the test position is
        // perpendicular to a location on the given line segment)
        if (VectorLength2 < 0.0001)
        {
          Offset = Consts.NullDouble;
          T = 2;
          return;
        }

        // transform PointToTest into curve space (where the line vector is the +ve x axis)
        double _pX = PointToTestX - CurveSegmentStartX;
        double _pY = PointToTestY - CurveSegmentStartY;

        double Px = ((LineVectorX * _pX) + (LineVectorY * _pY)) / VectorLength2;
        double Py = ((LineVectorX * _pY) - (LineVectorY * _pX)) / VectorLength2;

        // now we solve this for d
        //
        // -3(alpha+beta)d^2 + (1/py + 4alpha + 2beta)d - (alpha+px/py) = 0
        //
        // unless py == 0, in which case
        //
        // d = px;

        double a, b, c, d, disc;

        if (Math.Abs(Py) < Epsylon)
        {
          d = Px;
//        Writeln(LogFile, 'Abs(Py) < Epsylon'); {SKIP}
        }
        else if (Math.Abs(Alpha + Beta) < Epsylon)
        {
          d = (Py * Alpha + Px) / (2.0 * Py * Alpha + 1);
//          Writeln(LogFile, 'abs(Alpha + Beta) < Epsylon'); {SKIP}
        }
        else
        {
          // general quad formula
          a = -3.0 * (Alpha + Beta);
          b = 1.0 / Py + 4.0 * Alpha + 2.0 * Beta;
          c = -(Alpha + (Px / Py));

          disc = b * b - 4 * a * c;

          if (disc < 0) // There's no solution!
          {
            Offset = Consts.NullDouble;
            T = Consts.NullDouble;
            return;
          }

          if (Py < 0)
            d = (-b - Math.Sqrt(disc)) / (2 * a);
          else
            d = (-b + Math.Sqrt(disc)) / (2 * a);
        }

        // we have distance from our point to this point on the line.
        Offset = MathUtilities.Hypot(Py, Px - d);

// evaluate the cubic at Px, to find what Py should be close to...
        double y = Alpha * d;
        double d2 = d * d; // cache this

        y = y - (2 * Alpha + Beta) * d2;
        y = y + (Alpha + Beta) * d2 * d; // see? much better.

        if (Py > 0)
          Offset = -(Offset - y);
        else
          Offset = Offset + y;

        // Convert the distance from line-segment space to world space
        Offset = Offset * VectorLength;
        T = d;

/*
  {$IFDEF SVO_EMIT_TEST_OUTPUT_TO_OUTPUTDEBUGSTRING}
    if LogNFFSmoothPolylineCalcs and InRange(t, -0.00001, 1.00001) then
      begin
        OutputDebugString(PChar(ImageAttrib.Format('f:DistanceToNFFCurve(StartX=%.6f, StartY=%.6f, EndX=%.6f, EndY=%.6f, TestX=%.6f, TestY=%.6f, Alpha=%.8f, Beta=%.8f', { SKIP}
[CurveSegmentStartX, CurveSegmentStartY,
 CurveSegmentEndX, CurveSegmentEndY,
 PointToTestX, PointToTestY,
 Alpha, Beta])));

        OutputDebugString(PChar(ImageAttrib.Format('f:DistanceToNFFCurve(LineVectorX=%.6f, LineVectorY:%.6f, VectorLength2:%.6f, _pX=%.6f, _pY=%.6f, Px=%.6f, Py=%.6f', { SKIP}
[LineVectorX, LineVectorY, VectorLength2,
 _pX, _pX, Px, Py])));
        OutputDebugString(PChar(ImageAttrib.Format('f:DistanceToNFFCurve(a=%.6f, b:%.6f, c:%.6f, d=%.6f, disc=%.6f, offset=%.6f, y=%.6f, d2=%.6f, T=%.6f', { SKIP}
[a, b, c, d, disc, offset, y, d2, T])));
      end;
  {$ENDIF}
  */
      }
      catch
      {
        Offset = Consts.NullDouble;
        T = 2;
      }
    }

    public static void CalcNFFCurvePosFromParmAndOfs(double CurveSegmentStartX, double CurveSegmentStartY,
      double CurveSegmentEndX, double CurveSegmentEndY,
      double alpha, double beta,
      double t,
      double Offset,
      out double x, out double y)
    {
/*
Here's what you need to do:

(notation convention for this email.Capitals are 2D vectors, lowercase are scalars,
and Capital_x or _y are the scalar components of a vector.Vector lengths are
represented by enclosing the vector in bars)

For a given curved segment constructed on the line segment X(X would then be the
vector from the start of the curve to the end) from a start point P:

Create a vector Y perpendicular to X in the left-hand direction.So if X is a
vector (X_x, X_y), then Y will be (-X_y, X_x).

You calculate how far along the segment your stationing value will take you.
If the station at the start is s1, and the station at the end is s2, and you're
interested in a station s, then the parameter that you'll feed into the cubic will be:

t = (s - s1) / (s2 - s1)

Evaluate the cubic at this point to find c

c = t^3 * (alpha+beta) - t^2 * (2*alpha + beta) + t* alpha

So the point on the cubic at station S and offset zero will be given by :

P + t* X + c* Y

You then need to calculate a vector perpendicular to the cubic at this point,
and move along it by your offset amount to get to where you want to be.
The gradient of the cubic at the point t is given by:

g = t ^ 2 * 3 * (alpha + beta) - t * 2 * (2 * alpha + beta) + alpha

So the vector tangent to the curve is

T = X + g * Y

And the perpendicular vector is this T vector rotation 90 degrees anti-clockwise,
which is (also we normalise in this step)

Q = (-T_y, T_x) / |T|

So for offset o, your final point is

P + t* X + (o + c) * Q

Huzzah!
=dave
*/
      //For a given curved segment constructed on the line segment X (X would then be the
      //vector from the start of the curve to the end) from a start point P:

      double X_x = CurveSegmentEndX - CurveSegmentStartX;
      double X_y = CurveSegmentEndY - CurveSegmentStartY;

      // Scale the offset to the interval length we are calculating over, and makes it's
      // sign consistent with the offset direction in the interval
      double _Offset = Offset;
      Offset = -(_Offset / MathUtilities.Hypot(X_x, X_y));

      //Create a vector Y perpendicular to X in the left-hand direction. So if X is a
      //vector (X_x, X_y), then Y will be (-X_y, X_x).

      double Y_x = -X_y;
      double Y_y = X_x;

      double CoefficientsSum = alpha + beta;

      //Evaluate the cubic at this point to find c
      //c = t^3 * (alpha+beta) - t^2 * (2*alpha + beta) + t * alpha
      double c = t * t * t * CoefficientsSum - t * t * (2 * alpha + beta) + t * alpha;

      //So the point on the cubic at station S and offset zero will be given by :
      // P + t*X + c*Y

      //You then need to calculate a vector perpendicular to the cubic at this point,
      //and move along it by your offset amount to get to where you want to be.
      //The gradient of the cubic at the point t is given by:
      //
      //g = t^2 * 3 * (alpha+beta) - t * 2 * (2 * alpha + beta) + alpha
      double g = t * t * 3 * CoefficientsSum - t * 2 * (2 * alpha + beta) + alpha;

      //So the vector tangent to the curve is
      //T = X + g * Y
      double T_x = X_x + g * Y_x;
      double T_y = X_y + g * Y_y;

      //And the perpendicular vector is this T vector rotation 90 degrees anti-clockwise,
      //which is (also we normalise in this step)
      //Q = (-T_y, T_x) / |T|
      double Mag_T = MathUtilities.Hypot(T_x, T_y) / MathUtilities.Hypot(X_x, X_y);
      double Q_x = -T_y / Mag_T;
      double Q_y = T_x / Mag_T;

      //So for offset o, your final point is
      //P + t * X + (o + c) * Q
      x = CurveSegmentStartX + t * X_x + (Offset + c) * Q_x;
      y = CurveSegmentStartY + t * X_y + (Offset + c) * Q_y;

      /*
      {$IFDEF SVO_EMIT_TEST_OUTPUT_TO_OUTPUTDEBUGSTRING      }
      if LogNFFSmoothPolylineCalcs then
      begin
      OutputDebugString(PChar(Format('f:CalcNFFCurvePosFromParmAndOfs(StartX=%.6f, StartY=%.6f, EndX=%.6f, EndY=%.6f, Alpha=%.8f, Beta=%.8f, t=%.8f, Offset = %.6f, Scaled Offset=%.6f', 
      [CurveSegmentStartX, CurveSegmentStartY,
      CurveSegmentEndX, CurveSegmentEndY,
      Alpha, Beta, T, _Offset, Offset])));

      OutputDebugString(PChar(Format('f:CalcNFFCurvePosFromParmAndOfs(X_x=%.6f, X_y=%.6f, Y_x=%.6f, Y_y=%.6f, c=%.8f, g=%.8f', 
      [X_x, X_y, Y_x, Y_y, c, g])));
      OutputDebugString(PChar(Format('f:CalcNFFCurvePosFromParmAndOfs(T_x=%.6f, T_y=%.6f, Mag_T=%.6f, Q_x=%.6f, Q_y=%.8f, x=%.8f, y=%.8f', 
      [T_x, T_y, Mag_T, Q_x, Q_y, x, y])));
      end;
      {$ENDIF}
      */
    }

    public static string MagicNumberToANSIString(byte[] MagicNumber)
    {
      return System.Text.Encoding.ASCII.GetString(MagicNumber);
    }

    public static bool SetFileVersionFromMinorMajorVersionNumbers(byte MajorVer, byte MinorVer,
      out NFFFileVersion FileVersion)
    {
      bool Result = true;

      // Convert the minor/major version numbering into the file version enum
      if (MajorVer == 1 && MinorVer == 0)
        FileVersion = NFFFileVersion.Version1_0;
      else if (MajorVer == 1 && MinorVer == 1)
        FileVersion = NFFFileVersion.Version1_1;
      else if (MajorVer == 1 && MinorVer == 2)
        FileVersion = NFFFileVersion.Version1_2;
      else if (MajorVer == 1 && MinorVer == 3)
        FileVersion = NFFFileVersion.Version1_3;
      else if (MajorVer == 1 && MinorVer == 4)
        FileVersion = NFFFileVersion.Version1_4;
      else if (MajorVer == 1 && MinorVer == 5)
        FileVersion = NFFFileVersion.Version1_5;
      else if (MajorVer == 1 && MinorVer == 6)
        FileVersion = NFFFileVersion.Version1_6;
      else
      {
        FileVersion = NFFFileVersion.Version_Undefined;
        Result = false;
      }

      return Result;
    }

    public static void DecomposeSmoothPolyLineSegmentToPolyLine(NFFLineworkSmoothedPolyLineVertexEntity startPt, 
      NFFLineworkSmoothedPolyLineVertexEntity endPt,
      double minimumLength,
      double maxSegmentLength,
      int maxNumSegments,
      Action<double, double, double, double, DecompositionVertexLocation> decompositionCallback)
    {
      // How long is this element?
      var lx = endPt.X - startPt.X;
      var ly = endPt.Y - startPt.Y;
      var Len = MathUtilities.Hypot(lx, ly);

      if (Len > minimumLength)
      {
        // Cache these, for speed.
        var Alpha = endPt.Alpha;
        var Beta = endPt.Beta;

        // Add the start point of the segment to the poly line vertices
        decompositionCallback(startPt.X, startPt.Y, Consts.NullDouble, startPt.Chainage, DecompositionVertexLocation.First);

        // zero coefficients means a straight line
        if (Alpha != 0 && Beta != 0)
        {
          var AlphaPlusBeta = Alpha + Beta;
          var TwoAlphaPlusBeta = AlphaPlusBeta + Alpha;

          // so we want to keep each segment no more than MaxSegmentLength in distance
          var SegmentCount = (int) Math.Truncate(Len / maxSegmentLength) + 1;

          // unless that means millions of segments...
          if (SegmentCount > maxNumSegments)
            SegmentCount = maxNumSegments;

          var Step = 1.0 / SegmentCount;
          var Ordinate = Step; // not starting at zero, of course.

          // Now calc each segment.
          for (var SegmentIdx = 1; SegmentIdx < SegmentCount; SegmentIdx++)
          {
            var o2 = Ordinate * Ordinate;
            var Cubic = AlphaPlusBeta * o2 * Ordinate - TwoAlphaPlusBeta * o2 + Alpha * Ordinate;

            XYZ pt;
            pt.X = startPt.X + lx * Ordinate;
            pt.Y = startPt.Y + ly * Ordinate;

            if (Cubic != 0.0)
            {
              pt.X -= ly * Cubic;
              pt.Y += lx * Cubic;
            }

            // Add the vertex to the poly line
            decompositionCallback(pt.X, pt.Y, Consts.NullDouble, Consts.NullDouble, DecompositionVertexLocation.Intermediate);

            Ordinate += Step;
          }
        }

        // Add the end point of the segment to the poly line vertices
        decompositionCallback(endPt.X, endPt.Y, Consts.NullDouble, endPt.Chainage, DecompositionVertexLocation.Last);
      }
      else
      {
        decompositionCallback(startPt.X, startPt.Y, Consts.NullDouble, startPt.Chainage, DecompositionVertexLocation.First);
        decompositionCallback(endPt.X, endPt.Y, Consts.NullDouble, endPt.Chainage, DecompositionVertexLocation.Last);
      }
    }

    public static void VectoriseSmoothPolylineTolerance(NFFLineworkSmoothedPolyLineEntity SmoothPolyline,
      double ATolerance,
      Action<double, double, DecompositionVertexLocation> DecompCallback)
    {
// ATolerance is defined as a maximum deviation from the point being inserted to a line
//  between the two points at each end of the interval being inserted into. 
      double Alpha, Beta;
      double AlphaPlusBeta;
      double TwoAlphaPlusBeta;
      double lx, ly;
      NFFLineworkSmoothedPolyLineVertexEntity StartPt, EndPt;
      XYZ StartPos, EndPos;

      XYZ GetPosition(double Ordinate)
      {
        double o2 = Ordinate * Ordinate;
        double Cubic = AlphaPlusBeta * o2 * Ordinate - TwoAlphaPlusBeta * o2 + Alpha * Ordinate;

        var Result = new XYZ(StartPt.X + lx * Ordinate, StartPt.Y + ly * Ordinate, Consts.NullDouble);

        if (Cubic != 0.0)
        {
          Result.X -= ly * Cubic;
          Result.Y += lx * Cubic;
        }

        return Result;
      }

      void InsertIntoInterval(double IntStart, double IntEnd, XYZ startPos, XYZ endPos)
      {
        double HalfInterval = (IntStart + IntEnd) / 2;
        var pos = GetPosition(HalfInterval);

        if (Math.Abs(XYZ.GetPointOffset(startPos, endPos, pos)) > ATolerance)
        {
          // Process the interval before the point to insert
          InsertIntoInterval(IntStart, HalfInterval, startPos, pos);

          // Insert the point that divides this interval
          DecompCallback(pos.X, pos.Y, DecompositionVertexLocation.Intermediate);

          // Process the interval after before the point to insert
          InsertIntoInterval(HalfInterval, IntEnd, pos, endPos);
        }
      }

      //  if SmoothPolyline.Vertices.Count < 2 then
      //    Exit;

      StartPt = SmoothPolyline.Vertices.First();
      EndPt = SmoothPolyline.Vertices.Last();

//  lx = EndPt.X - StartPt.X;
//  ly = EndPt.Y - StartPt.Y;

      // Add the start point of the smooth polyline
      DecompCallback(StartPt.X, StartPt.Y, DecompositionVertexLocation.First);

      for (int I = 0; I < SmoothPolyline.Vertices.Count - 1; I++)
      {
        EndPt = SmoothPolyline.Vertices[I + 1];

        lx = EndPt.X - StartPt.X;
        ly = EndPt.Y - StartPt.Y;

        // Cache these, for speed.
        Alpha = EndPt.Alpha;
        Beta = EndPt.Beta;

        // zero co-efficients means a straight line
        if (Alpha != 0 && Beta != 0)
        {
          AlphaPlusBeta = Alpha + Beta;
          TwoAlphaPlusBeta = AlphaPlusBeta + Alpha;

          StartPos = new XYZ(StartPt.X, StartPt.Y, StartPt.Z);
          EndPos = new XYZ(EndPt.X, EndPt.Y, EndPt.Z);

          InsertIntoInterval(0, 1, StartPos, EndPos);
        }

        if (I < SmoothPolyline.Vertices.Count - 1)
          DecompCallback(EndPt.X, EndPt.Y, DecompositionVertexLocation.Intermediate);

        StartPt = EndPt;
      }

      // Add the end point of the segment to the polyline vertices
      DecompCallback(EndPt.X, EndPt.Y, DecompositionVertexLocation.Last);
    }

    public static void DecomposeArcToPolyline(double x0, double y0, double x1, double y1, double cX, double cY,
      double angle,
      double arcChordDist,
      Action<double, double, DecompositionVertexLocation> decompCallback)
    {
      // Decompose an arc from start point[Xstart, Ystart] to end point[Xend, Yend]
      // with centre point[Xcenter, Ycentre]
      //  These are world units.
      //  Angle is in the mathematical sense - +ve for anti-clockwise, and is in RADIANS 

      var relX = x0 - cX; // Position, relative to cX, cY 
      var relY = y0 - cY;
      var radius = MathUtilities.Hypot(relX, relY);

      // Decompose arc into line segments with not more than ArcChordDist arc/chord separation
      // If you know r (radius) and h (arc/chord separation). Then
      //   d (Chord mid point to circle center distance)  = r - h,
      //   theta (central angle subtending arc) = 2 arccos(d/r),
      //   c (chord length) = 2r sin(theta/2),

      var chordToCenterDist = radius - arcChordDist;
      if (chordToCenterDist < 0)
        chordToCenterDist = radius / 2;

      var theta = 2 * Math.Acos(chordToCenterDist / radius);

      // If the included angle is too shallow, restrict it to 0.1 degrees
      if (theta * (180 / Math.PI) < 0.1)
        theta = 0.1 * (Math.PI / 180);

      var a = theta; //Temporary angle in radians
      var s = Math.Sin(a); // Sine and cosine of step angle 
      var c = Math.Cos(a);
      var a_step = a;

      if (angle >= 0)
        s = -s;

      angle = Math.Abs(angle);

      decompCallback(x0, y0, DecompositionVertexLocation.First);

      while (a < angle)
      {
        var tx = c * relX + s * relY;
        relY = -s * relX + c * relY;
        relX = tx;

        decompCallback(cX + relX, cY + relY, DecompositionVertexLocation.Intermediate);

        a += a_step;
      }

      // Include the last vertex
      decompCallback(x1, y1, DecompositionVertexLocation.Last);
    }
  }
}
