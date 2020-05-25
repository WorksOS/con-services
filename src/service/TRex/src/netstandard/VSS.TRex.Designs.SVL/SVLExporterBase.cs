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
