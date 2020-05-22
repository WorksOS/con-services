using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Designs.SVL.Utilities;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.SVL
{
  public class SVLExporterBase
  {
    public DistanceUnitsType Units { get; set; } = DistanceUnitsType.Meters;
    public double AlignmentLabelingInterval { get; set; } = 10; // Default to 10 meters

    protected const int kAlignmentCenterLineColor = 1; // Red
    protected const int kAlignmentCenterLineThickness = 2;

    protected double AzimuthAt(NFFGuidableAlignmentEntity alignment, double stn)
    {
      double TestStn1, TestStn2;

      if (stn < alignment.StartStation + 0.001)
        TestStn1 = alignment.StartStation;
      else
        TestStn1 = stn - 0.001;

      if (stn > (alignment.EndStation - 0.001))
        TestStn2 = alignment.EndStation;
      else
        TestStn2 = stn + 0.001;

      alignment.ComputeXY(TestStn1, 0, out var X1, out var Y1);
      alignment.ComputeXY(TestStn2, 0, out var X2, out var Y2);

      if (X1 != Consts.NullDouble && Y1 != Consts.NullDouble && X2 != Consts.NullDouble && Y2 != Consts.NullDouble)
      {
        GeometryUtils.RectToPolar(Y1, X1, Y2, X2, out var result, out _);
        return result;
      }

      return Consts.NullDouble;
    }

    protected DesignProfilerRequestResult Validate(NFFGuidableAlignmentEntity alignment)
    {
      if (alignment.Entities.Count == 0)
      {
        return DesignProfilerRequestResult.AlignmentContainsNoElements;
      }

      if (alignment.StartStation == Consts.NullDouble || alignment.EndStation == Consts.NullDouble)
      {
        return DesignProfilerRequestResult.AlignmentContainsNoStationing;
      }

      if (alignment.StartStation >= alignment.EndStation)
      {
        return DesignProfilerRequestResult.AlignmentContainsInvalidStationing;
      }

      return DesignProfilerRequestResult.OK;
    }
  }
}
