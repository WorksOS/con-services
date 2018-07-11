using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  /// <summary>
  /// The request representation for managing of the Raptor’s list of ground/surveyed surfaces.
  /// </summary>
  /// 
  public class SurveyedSurfaceRequest : ProjectID, IValidatable
  {
    /// <summary>
    /// Description to identify a surveyed surface file either by id or by its location in TCC.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "SurveyedSurface", Required = Required.Always)]
    [Required]
    public DesignDescriptor SurveyedSurface { get; private set; }

    /// <summary>
    /// Surveyed UTC date/time.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "SurveyedUtc", Required = Required.Always)]
    [Required]
    public DateTime SurveyedUtc { get; private set; }

    /// <summary>
    /// Private constructor.
    /// </summary>
    /// 
    private SurveyedSurfaceRequest()
    {
      // ...
    }

    /// <summary>
    /// Creates a instance of the SurveyedSurfaceRequest class.
    /// </summary>
    /// <param name="projectId">Project ID.</param>
    /// <param name="surveyedSurface">Descripotion of Surveyed surface data.</param>
    /// <param name="surveyedUtc">Surveyed UTC.</param>
    /// <returns></returns>
    /// 
    public static SurveyedSurfaceRequest CreateSurveyedSurfaceRequest(
      long projectId,
      DesignDescriptor surveyedSurface,
      DateTime surveyedUtc
      )
    {
      return new SurveyedSurfaceRequest
      { 
                ProjectId = projectId,
                SurveyedSurface = surveyedSurface,
                SurveyedUtc = surveyedUtc
              };
    }
    
    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();
      SurveyedSurface.Validate();
    }
  }
}
