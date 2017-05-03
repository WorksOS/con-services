using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using WebApiModels.ResultHandling;

namespace WebApiModels.Models
{
  /// <summary>
  /// The request representation used to request the boundary of a project that is active at a specified date time.
  /// </summary>
  public class GetProjectBoundaryAtDateRequest: ContractRequest 
  {
    /// <summary>
    /// The id of the project to get the boundary of. 
    /// </summary>
    [JsonProperty(PropertyName = "projectId", Required = Required.Always)]
    public long projectId { get; set; }

    /// <summary>
    /// The date time from the tag file which must be within the active project date range. 
    /// </summary>
    [JsonProperty(PropertyName = "tagFileUTC", Required = Required.Always)]
    public DateTime tagFileUTC { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private GetProjectBoundaryAtDateRequest()
    { }

    /// <summary>
    /// Create instance of GetProjectBoundaryAtDateRequest
    /// </summary>
    public static GetProjectBoundaryAtDateRequest CreateGetProjectBoundaryAtDateRequest(long projectId, DateTime tagFileUTC)
    {
      return new GetProjectBoundaryAtDateRequest
      {
        projectId = projectId,
        tagFileUTC = tagFileUTC
      };
    }
    

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (projectId <= 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            String.Format("Must have projectID {0}", projectId)));
      }

      if (!(tagFileUTC > DateTime.UtcNow.AddYears(-5) && tagFileUTC <= DateTime.UtcNow))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            String.Format("tagFileUTC must have occured within last 5 years {0}", tagFileUTC)));
      }
    }
  }
}