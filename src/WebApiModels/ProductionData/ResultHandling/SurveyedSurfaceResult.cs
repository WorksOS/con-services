using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;

namespace VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling
{
  /// <summary>
  /// Surveyed Surface result class.
  /// </summary>
  /// 
  public class SurveyedSurfaceResult : ContractExecutionResult
  {
    /// <summary>
    /// Array of Surveyed Surface details.
    /// </summary>
    /// 
    public SurveyedSurfaceDetails[] SurveyedSurfaces { get; private set; }

    /// <summary>
    /// Private constructor.
    /// </summary>
    /// 
    private SurveyedSurfaceResult()
    {
      // ...
    }

    /// <summary>
    /// Creates an instance of the SurveyedSurfaceResult class.
    /// </summary>
    /// <param name="surveyedSurfaces">Array of Surveyed Surface details.</param>
    /// <returns>A created instance of the SurveyedSurfaceResult class.</returns>
    /// 
    public static SurveyedSurfaceResult CreateSurveyedSurfaceResult(SurveyedSurfaceDetails[] surveyedSurfaces)
    {
      return new SurveyedSurfaceResult { SurveyedSurfaces = surveyedSurfaces };
    }

    /// <summary>
    /// Creates a sample instance of the SurveyedSurfaceResult class to be displayed in Help documentation.
    /// </summary>
    /// 
    public static SurveyedSurfaceResult HelpSample
    {
      get { return new SurveyedSurfaceResult { SurveyedSurfaces = new SurveyedSurfaceDetails[] { SurveyedSurfaceDetails.HelpSample } }; }
    }
  }
}