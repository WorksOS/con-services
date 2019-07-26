using System;
using System.Linq;
using VSS.Common.Abstractions.Configuration;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Designs.Models;
using VSS.TRex.Designs.SVL.Interfaces;
using VSS.TRex.Designs.SVL.Utilities;
using VSS.TRex.DI;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.SVL
{
  /// <summary>
  /// Constructs a polygonal boundary around a given station and offset range for a master guidable alignment
  /// </summary>
  public class SVLAlignmentBoundaryDeterminator : ISVLAlignmentBoundaryDeterminator
  {
    private const double DEFAULT_MINIMUM_STATION_INTERVAL_FOR_SVL_FILTER_BOUNDARY_GENERATION = 1.0;
    private const double DEFAULT_DESIGN_FILTER_BOUNDARY_POLYLINE_COMPRESSION_TOERANCE = 0.05;

    private const double MinimumRoadFenceAngle = 89; //Degrees
    private const double Epsilon = 1; // Shortest allowable segment is 1m
    private const double SmallEpsilon = 0.001; // Silly small segment length we will discard out of hand

    private double MinimumStationIntervalForSVLFilterBoundaryGeneration =
      DIContext.Obtain<IConfigurationStore>().GetValueDouble("MINIMUM_STATION_INTERVAL_FOR_SVL_FILTER_BOUNDARY_GENERATION", DEFAULT_MINIMUM_STATION_INTERVAL_FOR_SVL_FILTER_BOUNDARY_GENERATION);

    private double DesignFilterBoundaryPolylineCompressionTolerance = 
      DIContext.Obtain<IConfigurationStore>().GetValueDouble("DESIGN_FILTER_BOUNDARY_POLYLINE_COMPRESSION_TOERANCE", DEFAULT_DESIGN_FILTER_BOUNDARY_POLYLINE_COMPRESSION_TOERANCE);

    public NFFGuidableAlignmentEntity Alignment { get; set; }
    public double StartStation { get; set; }
    public double EndStation { get; set; }
    public double OffsetLeft { get; set; }
    public double OffsetRight { get; set; }

    private SVLAlignmentBoundaryDeterminator()
    {
    }

    public SVLAlignmentBoundaryDeterminator(NFFGuidableAlignmentEntity alignment,
      double startStation, double endStation, double offsetLeft, double offsetRight)
    {
      if (alignment == null)
        throw new ArgumentException($"Alignment cannot be null in constructor for {nameof(SVLAlignmentBoundaryDeterminator)}");

      if (!alignment.IsMasterAlignment() || !alignment.IsStationed)
        throw new ArgumentException($"Alignment must be a stationed master alignment in constructor for {nameof(SVLAlignmentBoundaryDeterminator)}");

      if (offsetLeft > offsetRight)
        MinMax.Swap(ref offsetLeft, ref offsetRight);

      Alignment = alignment;

      StartStation = startStation;
      EndStation = endStation;
      OffsetLeft = offsetLeft;
      OffsetRight = offsetRight;
    }

    public bool DetermineBoundary(out DesignProfilerRequestResult calcResult, out Fence fence)
    {
      // Todo InterlockedIncrement64(DesignProfilerRequestStats.NumAlignmentFilterBoundariesComputed);

      // Walk the alignment and construct a boundary as per the SVO algorithm for road design boundary construction
      CalculateRoadSubsetFence(out calcResult, out fence);

      return calcResult == DesignProfilerRequestResult.OK && fence.NumVertices > 2;
    }

    private double SubtendedAngle(double bearing1, double bearing2)
    {
      var result = Math.Abs(bearing1 - bearing2);
      if (result > Math.PI)
        result = 2 * Math.PI - result;
      return result;
    }

    private double BearingOf(double x1, double y1, double x2, double y2)
    {
      GeometryUtils.rect_to_polar(y1, x1, y2, x2, out double result, out _);
      return result;
    }

    private double AzimuthAt(double Stn)
    {
      double TestStn1, TestStn2;
      double result = Consts.NullDouble;

      if (Stn < (Alignment.StartStation + 0.001))
        TestStn1 = Alignment.StartStation;
      else
        TestStn1 = Stn - 0.001;

      if (Stn > (Alignment.EndStation - 0.001))
        TestStn2 = Alignment.EndStation;
      else
        TestStn2 = Stn + 0.001;

      Alignment.ComputeXY(TestStn1, 0, out double X1, out double Y1);
      Alignment.ComputeXY(TestStn2, 0, out double X2, out double Y2);

      if (X1 != Consts.NullDouble && Y1 != Consts.NullDouble && X2 != Consts.NullDouble && Y2 != Consts.NullDouble)
        GeometryUtils.rect_to_polar(Y1, X1, Y2, X2, out result, out _);

      return result;
    }

    private void AddOffsetPoints(Fence fence, Fence RHSProfile, double currentPos, bool force)
    {
      double ptX, ptY;

      // Find the LHS/RHS coords
      double PtBrng1 = 0;
      double PtBrng2 = 0;

      Alignment.LocateEntityAtStation(currentPos, out NFFStationedLineworkEntity Element);

      if (Element == null)
        return;

      Element.ComputeXY(currentPos, OffsetLeft, out ptX, out ptY);

      if (fence.NumVertices > 0)
        PtBrng1 = BearingOf(fence.Points.Last().X, fence.Points.Last().Y, ptX, ptY);
      fence.Points.Add(new FencePoint(ptX, ptY));

      Element.ComputeXY(currentPos, OffsetRight, out ptX, out ptY);

      if (RHSProfile.NumVertices > 0)
        PtBrng2 = BearingOf(RHSProfile.Points[RHSProfile.NumVertices - 1].X, RHSProfile.Points[RHSProfile.NumVertices - 1].Y, ptX, ptY);
      RHSProfile.Points.Add(new FencePoint(ptX, ptY));

      // Prune points on the inside on curves where the offset is greater than the radius and
      // hence cause the points to walk the wrong way down the road!
      if (!force && fence.NumVertices > 1 && SubtendedAngle(PtBrng1, PtBrng2) > Math.PI / 4)
      {
        // Check arc radii 
        if (Element is NffLineworkArcEntity element)
          if (element.Radius() < (OffsetLeft + 0.001))
            fence.Points.RemoveAt(fence.NumVertices - 1);
          else if (element.Radius() > (OffsetRight - 0.001))
            RHSProfile.Points.RemoveAt(RHSProfile.NumVertices - 1);

        //  Check smooth curve radii ???  - tricky!!!
        //        if SubtendedAngle(PtBrng1, brng1) > SubTendedAngle(ptBrng2, brng2) then // we need to prune the negative offset point
        //           Fence.Remove(Fence.Last)
        //        else  // We need to prune the positive offset point
        //           RHSProfile.Remove(RHSProfile.Last);
      }
    }

    private void SmoothFenceLine(Fence fence, int StartIdx, int Increment, int EndIdx)
    {
      // Smooth each a fence line, removing all points forming the apex of corners
      // smaller than MinimumRoadFenceAngle degrees. Preserve the start and end points.
      double brng;
      double DistA, DistB, DistC;

      // First, remove any vertices that are too close to each other and corners that are too sharp.
      int I = StartIdx;
      do
      {
        GeometryUtils.rect_to_polar(fence[I - 1].X, fence[I - 1].Y, fence[I + 1].X, fence[I + 1].Y, out brng, out DistA);
        GeometryUtils.rect_to_polar(fence[I].X, fence[I].Y, fence[I - 1].X, fence[I - 1].Y, out brng, out DistB);
        GeometryUtils.rect_to_polar(fence[I].X, fence[I].Y, fence[I + 1].X, fence[I + 1].Y, out brng, out DistC);

        if (DistA < Epsilon || DistB < SmallEpsilon || DistC < SmallEpsilon ||
            // Cosine rule
            (DistB * DistB + DistC * DistC - DistA + DistA) / (2 * DistB * DistC) > Math.Cos(MinimumRoadFenceAngle * (Math.PI / 180)))
        {
          fence.Points.RemoveAt(I);
          if (Increment == 1)
            EndIdx--;
          if ((Increment == 1 && I > StartIdx) || (Increment == -1 && I > EndIdx))
            I--;
        }
        else
        I += Increment;
      } while (I != EndIdx);

      // Next: Apply polyline compression to the polyline to remove redundant vertices.
      fence.Compress(0.01); // Todo: DesignFilterBoundaryPolylineCompressionTolerance);
    }

    private void CalculateRoadSubsetFence(out DesignProfilerRequestResult calcResult, out Fence fence)
      // Run down the road from start to end chainage in steps the size of which is
      // determined by the default cross section interval, computing the
      // left and right offset coordinates OffsetLeft & OffsetRight meters from the centerline and add these to
      // the fence point list
    {
      fence = new Fence();

      double SubsetStartStation = Math.Max(Alignment.StartStation, StartStation);
      double SubsetEndStation = Math.Min(Alignment.EndStation, EndStation);

      if (SubsetEndStation <= SubsetStartStation)
      {
        calcResult = DesignProfilerRequestResult.InvalidStationValues;
        return;
      }

      double CurrentPos = SubsetStartStation;
      double Interval = Math.Max(MinimumStationIntervalForSVLFilterBoundaryGeneration,
        Math.Min(Math.Abs(OffsetLeft), Math.Abs(OffsetRight)) / 4);

      var RHSProfile = new Fence();

      // Calculate all the offset points
      while (true)
      {
        AddOffsetPoints(fence, RHSProfile, CurrentPos, false);
        CurrentPos = CurrentPos + Interval;
        if (CurrentPos >= SubsetEndStation)
        {
          AddOffsetPoints(fence, RHSProfile, SubsetEndStation, true);
          break;
        }
      }

      // Now lets smooth each size if the fence, removing all points forming the apex of corners
      // smaller than MinimumRoadFenceAngle degrees;

      if (fence.NumVertices > 2)
        SmoothFenceLine(fence, 1, 1, fence.NumVertices - 1);
      if (RHSProfile.NumVertices > 2)
        SmoothFenceLine(RHSProfile, RHSProfile.NumVertices - 2, -1, 0);

      // Now copy the RHS profile fence points over to the fence
      for (int I = RHSProfile.NumVertices - 1; I >= 0; I--)
        fence.Points.Add(RHSProfile[I]);

      calcResult = DesignProfilerRequestResult.OK;
    }
  }
}
