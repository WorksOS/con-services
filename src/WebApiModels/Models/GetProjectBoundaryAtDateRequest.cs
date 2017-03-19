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
  public class GetProjectBoundaryAtDateRequest 
  {

    private long _projectId;
    private DateTime _tagFileUTC;

    /// <summary>
    /// The id of the project to get the boundary of. 
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "projectId", Required = Required.Always)]
    public long projectId { get { return _projectId; } private set { _projectId = value; } }

    /// <summary>
    /// The date time from the tag file which must be within the active project date range. 
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "tagFileUTC", Required = Required.Always)]
    public DateTime tagFileUTC { get { return _tagFileUTC; } private set { _tagFileUTC = value; } }

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
    /// Example for Help
    /// </summary>
    public static GetProjectBoundaryAtDateRequest HelpSample
    {
      get
      {
        return CreateGetProjectBoundaryAtDateRequest(3912, DateTime.UtcNow.AddMinutes(-1));
      }
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