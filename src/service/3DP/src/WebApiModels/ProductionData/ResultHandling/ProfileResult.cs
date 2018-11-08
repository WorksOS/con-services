using System.Collections.Generic;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// The representation of a profile computed as a straight line between two points in the cartesian grid coordinate system of the project or
  /// by following a section of an alignment centerline.
  /// </summary>
  /// 
  public class ProfileResult : BaseProfile
  {
    /// <summary>
    /// The collection of cells produced by the query. Cells are ordered by increasing station value along the line or alignment.
    /// </summary>
    /// 
    public List<ProfileCell> cells;

    /// <summary>
    /// A geometrical representation of the profile which defines the actual portion of the line or alignment used for the profile.
    /// </summary>
    /// 
    public List<StationLLPoint> alignmentPoints;

    /// <summary>
    /// Private constructor.
    /// </summary>
    /// 
    public ProfileResult()
    {
      // ...
    }
  }
}