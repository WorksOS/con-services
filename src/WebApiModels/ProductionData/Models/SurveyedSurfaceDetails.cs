using System;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  /// <summary>
  /// Representation of Surveyed Surface in a Raptor project.
  /// </summary>
  public class SurveyedSurfaceDetails
  {
    /// <summary>
    /// The ID of the Surveyed Surface file.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Description to identify a surveyed surface file either by id or by its location in TCC.
    /// </summary>
    public DesignDescriptor SurveyedSurface { get; set; }

    /// <summary>
    /// Surveyed UTC date/time.
    /// </summary>
    public DateTime AsAtDate { get; set; }

    /// <summary>
    /// T3DBoundingWorldExtent describes a plan extent (X and Y) covering a
    /// rectangular area of the world in world coordinates, and a height range
    /// within that extent.
    /// </summary>
    public BoundingBox3DGrid Extents { get; set; }

    /// <summary>
    /// Private constructor.
    /// </summary>
    private SurveyedSurfaceDetails()
    { }

    /// <summary>
    /// Creates an instance of the SurveyedSurfaceDetails class.
    /// </summary>
    public static SurveyedSurfaceDetails CreateSurveyedSurfaceDetails(
      long id,
      DesignDescriptor surveyedSurface,
      DateTime asAtDate,
      BoundingBox3DGrid extents
      )
    {
      return new SurveyedSurfaceDetails
      {
        Id = id,
        SurveyedSurface = surveyedSurface,
        AsAtDate = asAtDate,
        Extents = extents
      };
    }
  }
}
