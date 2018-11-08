using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
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
  }
}