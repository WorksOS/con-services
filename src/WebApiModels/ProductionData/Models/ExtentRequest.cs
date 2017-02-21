
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.Models
{
    /// <summary>
    /// A representation of a project extents request
    /// </summary>
  public class ExtentRequest : ProjectID, IValidatable
    {


      /// <summary>
      /// The set of surveyed surface IDs to be excluded from the calculation of the project extents
      /// </summary>
      [JsonProperty(PropertyName = "excludedSurveyedSurfaceIds", Required = Required.Default)]
      public long[] excludedSurveyedSurfaceIds { get; set; }


      public static ExtentRequest CreateExtentRequest(long ProjectId,
          long[] ExcludedSurveyedSurfaceIds)
      {
        return new ExtentRequest()
        {
          projectId = ProjectId,
          excludedSurveyedSurfaceIds = ExcludedSurveyedSurfaceIds
        };
      }

      public override void Validate()
      {
        base.Validate();
      }

      /// <summary>
      /// Create example instance of ExtentRequest to display in Help documentation.
      /// </summary>
      public new static ExtentRequest HelpSample
      {
        get
        {
          return new ExtentRequest()
          {
              projectId = 100,
              excludedSurveyedSurfaceIds = new long[]{100,101,201},
          };
        }
      }
    }
}