using System;
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

    /// <summary>
    /// Creates a sample instance of ProfileResult class to be displayed in the Help documentation.
    /// </summary>
    /// 
    public static ProfileResult HelpSample => new ProfileResult
    {
      callId = new Guid(),
      success = true,
      minStation = 0,
      maxStation = 100,
      minHeight = 0,
      maxHeight = 212,
      gridDistanceBetweenProfilePoints = 12,
      cells = new List<ProfileCell> { ProfileCell.HelpSample, ProfileCell.HelpSample },
      alignmentPoints = new List<StationLLPoint> { StationLLPoint.HelpSample, StationLLPoint.HelpSample}
    };
  }
}