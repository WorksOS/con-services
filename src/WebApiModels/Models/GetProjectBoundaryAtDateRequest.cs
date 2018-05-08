using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models
{
  /// <summary>
  /// The request representation used to request the boundary of a project that is active at a specified date time.
  /// </summary>
  public class GetProjectBoundaryAtDateRequest : ContractRequest
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
    {
    }

    /// <summary>
    /// Create instance of GetProjectBoundaryAtDateRequest
    /// </summary>
    public static GetProjectBoundaryAtDateRequest CreateGetProjectBoundaryAtDateRequest(long projectId,
      DateTime tagFileUTC)
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
          GetProjectBoundaryAtDateResult.CreateGetProjectBoundaryAtDateResult(false, new TWGS84FenceContainer(),
            ContractExecutionStatesEnum.ValidationError, 18));
      }

      if (!(tagFileUTC > DateTime.UtcNow.AddYears(-50) && tagFileUTC <= DateTime.UtcNow.AddDays(30)))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectBoundaryAtDateResult.CreateGetProjectBoundaryAtDateResult(false, new TWGS84FenceContainer(),
            ContractExecutionStatesEnum.ValidationError, 17));
      }
    }
  }
}