using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.SVL
{
  public class SVLExporterBase
  {
    public DistanceUnitsType Units { get; set; } = DistanceUnitsType.Meters;

    /// <summary>
    /// The interval along the alignment that labels should be created.
    /// This value is expressed in meters and defaults to 10 meters
    /// </summary>
    public double AlignmentLabelingInterval { get; set; } = 10; // Default to 10 meters

    /// <summary>
    /// Notes whether arcs elements should be expressed as poly lines (chorded arcs), or as geometric arcs.
    /// </summary>
    public bool ConvertArcsToPolyLines { get; set; }

    /// <summary>
    /// The maximum error between the arc a chorded poly line an arc should be converted into.
    /// This value is expressed in meters and defaults to 1 meter
    /// </summary>
    public double ArcChordTolerance { get; set; } = 1.0;

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
