using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;

namespace VSS.Productivity3D.TagFileAuth.Models
{
  /// <summary>
  /// The request representation used to request the boundary of a project that is active at a specified date time.
  /// </summary>
  public class GetProjectBoundaryAtDateRequest 
  {
    /// <summary>
    /// The id of the project to get the boundary of. 
    /// </summary>
    [JsonProperty(PropertyName = "projectId", Required = Required.Always)]
    public long shortRaptorProjectId { get; set; }

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
    public static GetProjectBoundaryAtDateRequest CreateGetProjectBoundaryAtDateRequest(long shortRaptorProjectId,
      DateTime tagFileUTC)
    {
      return new GetProjectBoundaryAtDateRequest
      {
        shortRaptorProjectId = shortRaptorProjectId,
        tagFileUTC = tagFileUTC
      };
    }


    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (shortRaptorProjectId <= 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          GetProjectBoundaryAtDateResult.CreateGetProjectBoundaryAtDateResult(false, new TWGS84FenceContainer(),
            ContractExecutionStatesEnum.ValidationError, 18));
      }

      if (!(tagFileUTC > DateTime.UtcNow.AddYears(-50) && tagFileUTC <= DateTime.UtcNow.AddDays(2)))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectBoundaryAtDateResult.CreateGetProjectBoundaryAtDateResult(false, new TWGS84FenceContainer(),
            ContractExecutionStatesEnum.ValidationError, 23));
      }
    }
  }
}
