using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.TagFileAuth.Service.Models.RaptorServicesCommon
{
  /// <summary>
  /// The request representation used to request the boundary of a project that is active at a specified date time.
  /// </summary>
  public class GetProjectBoundaryAtDateRequest //: ProjectID //, IValidatable//, IServiceDomainObject, IHelpSample
  {
    /// <summary>
    /// The id of the project to get the boundary of. 
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "projectId", Required = Required.Always)]
    public long projectId { get; private set; }

    /// <summary>
    /// The date time from the tag file which must be within the active project date range. 
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "tagFileUTC", Required = Required.Always)]
    public DateTime tagFileUTC { get; private set; }

    ///// <summary>
    ///// Private constructor
    ///// </summary>
    //private GetProjectBoundaryAtDateRequest()
    //{ }

    /// <summary>
    /// Create instance of GetProjectBoundaryAtDateRequest
    /// </summary>
    public static GetProjectBoundaryAtDateRequest CreateGetProjectBoundaryAtDateRequest(
      long projectId,
      DateTime tagFileUTC
      )
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
    public static new GetProjectBoundaryAtDateRequest HelpSample
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
    }
  }
}