using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.ResultHandling;
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
    public long[] excludedSurveyedSurfaceIds { get; private set; }

    public override void Validate()
    {
      base.Validate();
      // Validation rules might be placed in here...
      // throw new NotImplementedException();
      var validator = new DataAnnotationsValidator();
      ICollection<ValidationResult> results;
      validator.TryValidate(this, out results);
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
                 excludedSurveyedSurfaceIds = ExcludedSurveyedSurfaceIds
             };
    }

    //Private constructor to hide the request builder
    private ProjectStatisticsRequest()
    { }
  }
}
