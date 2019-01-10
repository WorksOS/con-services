using System.Linq;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Validation;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// Request representation for requesting project statistics
  /// </summary>
  public class ProjectStatisticsRequest : ProjectID, IValidatable
  {

    /// <summary>
    /// The set of surveyed surfaces that should be excluded from the calculation of the spatial and temporal extents of the project.
    /// </summary>
    [JsonProperty(PropertyName = "excludedSurveyedSurfaceIds", Required = Required.Default)]
    public long[] ExcludedSurveyedSurfaceIds { get; private set; }

    public override void Validate()
    {
      base.Validate();
      // Validation rules might be placed in here...
      // throw new NotImplementedException();
      var validator = new DataAnnotationsValidator();
      validator.TryValidate(this, out var results);
      if (results.Any())
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, results.FirstOrDefault().ErrorMessage));
      }
    }

    public static ProjectStatisticsRequest CreateStatisticsParameters(long ProjectId, long[] ExcludedSurveyedSurfaceIds)
    {
      return new ProjectStatisticsRequest
      {
        ProjectId = ProjectId,
        ExcludedSurveyedSurfaceIds = ExcludedSurveyedSurfaceIds
      };
    }

    //Private constructor to hide the request builder
    private ProjectStatisticsRequest()
    { }
  }
}
