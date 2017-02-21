using System;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.Models
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
    public DesignDescriptor SurveyedSurface { get; private set; }

    /// <summary>
    /// Surveyed UTC date/time.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "SurveyedUtc", Required = Required.Always)]
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
      return new SurveyedSurfaceRequest() 
              { 
                projectId = projectId,
                SurveyedSurface = surveyedSurface,
                SurveyedUtc = surveyedUtc
              };
    }

    /// <summary>
    /// Creates a sample instance of the SurveyedSurfaceRequest class to be displayed in Help documentation.
    /// </summary>
    /// 
    public new static SurveyedSurfaceRequest HelpSample
    {
      get { return new SurveyedSurfaceRequest() { projectId = 1, SurveyedSurface = DesignDescriptor.HelpSample, SurveyedUtc = DateTime.UtcNow }; }
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