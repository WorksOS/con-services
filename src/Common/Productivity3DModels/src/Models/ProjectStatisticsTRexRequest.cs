using System;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Productivity3D.Models.Validation;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Request representation for requesting project statistics
  /// </summary>
  public class ProjectStatisticsTRexRequest
  {
    /// <summary>
    /// A project unique identifier.
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Default)]
    [ValidProjectUID]
    public Guid ProjectUid { get; set; }

    /// <summary>
    /// The set of surveyed surfaces that should be excluded from the calculation of the spatial and temporal extents of the project.
    /// </summary>
    [JsonProperty(PropertyName = "excludedSurveyedSurfaceUids", Required = Required.Default)]
    public Guid[] ExcludedSurveyedSurfaceUids { get; private set; }

    /// <summary>
    /// Prevents a default instance of the <see cref="ProjectStatisticsTRexRequest"/> class from being created.
    /// </summary>
    private ProjectStatisticsTRexRequest()
    { }

    public ProjectStatisticsTRexRequest(Guid projectUid, Guid[] excludedSurveyedSurfaceUids)
    {
      ProjectUid = projectUid;
      ExcludedSurveyedSurfaceUids = excludedSurveyedSurfaceUids;
    }

    public void Validate()
    {
      var validator = new DataAnnotationsValidator();
      validator.TryValidate(this, out var results);
      if (results.Any())
      {
        throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, results.FirstOrDefault().ErrorMessage));
      }

      if (ExcludedSurveyedSurfaceUids != null && ExcludedSurveyedSurfaceUids.Length > 0)
      {
        foreach (var uid in ExcludedSurveyedSurfaceUids)
        {
          if (uid == Guid.Empty)
          {
            throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                string.Format(
                  "Excluded Surface Uid is invalid")));
          }
        }
      }
    }
  }
}
