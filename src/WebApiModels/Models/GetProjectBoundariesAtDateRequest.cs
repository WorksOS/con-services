using System;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models
{
  /// <summary>
  /// The request representation used to request the boundaries of projects that are active at a specified date time and belong to the owner
  /// of the specified asset.
  /// </summary>
  public class GetProjectBoundariesAtDateRequest : ContractRequest
  {
    /// <summary>
    /// The id of the asset owned by the customer whose active project boundaries are returned. 
    /// </summary>
    [JsonProperty(PropertyName = "assetId", Required = Required.Always)]
    public long assetId { get; set; }

    /// <summary>
    /// The date time from the tag file which must be within the active project date range. 
    /// </summary>
    [JsonProperty(PropertyName = "tagFileUTC", Required = Required.Always)]
    public DateTime tagFileUTC { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private GetProjectBoundariesAtDateRequest()
    {
    }

    /// <summary>
    /// Create instance of GetProjectBoundariesAtDateRequest
    /// </summary>
    public static GetProjectBoundariesAtDateRequest CreateGetProjectBoundariesAtDateRequest(long assetId,
      DateTime tagFileUTC)
    {
      return new GetProjectBoundariesAtDateRequest
      {
        assetId = assetId,
        tagFileUTC = tagFileUTC
      };
    }


    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (assetId <= 0)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectBoundariesAtDateResult.CreateGetProjectBoundariesAtDateResult(false, new ProjectBoundaryPackage[0],
            ContractExecutionStatesEnum.ValidationError, 9));
      }

      if (!(tagFileUTC > DateTime.UtcNow.AddYears(-50) && tagFileUTC <= DateTime.UtcNow.AddDays(30)))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectBoundariesAtDateResult.CreateGetProjectBoundariesAtDateResult(false, new ProjectBoundaryPackage[0],
            ContractExecutionStatesEnum.ValidationError, 17));
      }
    }
  }
}