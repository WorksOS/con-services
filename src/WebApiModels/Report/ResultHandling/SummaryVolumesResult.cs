using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.ResultHandling
{
  /// <summary>
  /// Represents result returned by Summary Volumes request
  /// </summary>
  public class SummaryVolumesResult : ContractExecutionResult
  {
    /// <summary>
    /// Default private constructor.
    /// </summary>
    private SummaryVolumesResult()
    { }

    /// <summary>
    /// Zone boundaries
    /// </summary>
    public BoundingBox3DGrid BoundingExtents { get; private set; }

    /// <summary>
    /// Cut volume in m3
    /// </summary>
    public double Cut { get; private set; }

    /// <summary>
    /// Fill volume in m3
    /// </summary>
    public double Fill { get; private set; }

    /// <summary>
    /// Cut area in m2
    /// </summary>
    public double CutArea { get; private set; }

    /// <summary>
    /// Fill area in m2
    /// </summary>
    public double FillArea { get; private set; }

    /// <summary>
    /// Total coverage area (cut + fill + no change) in m2. 
    /// </summary>
    public double TotalCoverageArea { get; private set; }

    public bool HasData() => true;

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static SummaryVolumesResult Create(BoundingBox3DGrid convertExtents, double cut,
            double fill, double totalCoverageArea, double cutArea, double fillArea)
    {
      return new SummaryVolumesResult
      {
        BoundingExtents = convertExtents,
        Cut = cut,
        Fill = fill,
        TotalCoverageArea = totalCoverageArea,
        CutArea = cutArea,
        FillArea = fillArea
      };
    }

    public static SummaryVolumesResult CreateEmptySummaryVolumesResult()
    {
      var boundingBox = BoundingBox3DGrid.CreatBoundingBox3DGrid(0, 0, 0, 0, 0, 0);

      return Create(boundingBox, 0.0, 0.0, 0.0, 0.0, 0.0);
    }
  }
}