using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;

namespace VSS.Productivity3D.TagFileAuth.Models
{
  /// <summary>
  /// The request representation used to request the boundaries of projects that are active at a specified date time and belong to the owner
  /// of the specified asset.
  /// </summary>
  public class GetProjectBoundariesAtDateRequest 
  {
    /// <summary>
    /// The id of the asset owned by the customer whose active project boundaries are returned. 
    /// </summary>
    [JsonProperty(PropertyName = "assetId", Required = Required.Always)]
    public long shortRaptorAssetId { get; set; }

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
    public static GetProjectBoundariesAtDateRequest CreateGetProjectBoundariesAtDateRequest(long shortRaptorAssetId,
      DateTime tagFileUTC)
    {
      return new GetProjectBoundariesAtDateRequest
      {
        shortRaptorAssetId = shortRaptorAssetId,
        tagFileUTC = tagFileUTC
      };
    }


    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (shortRaptorAssetId <= 0)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectBoundariesAtDateResult.CreateGetProjectBoundariesAtDateResult(false, new List<ProjectBoundaryPackage>(0),
            ContractExecutionStatesEnum.ValidationError, 9));
      }

      if (!(tagFileUTC > DateTime.UtcNow.AddYears(-50) && tagFileUTC <= DateTime.UtcNow.AddDays(2)))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectBoundariesAtDateResult.CreateGetProjectBoundariesAtDateResult(false, new List<ProjectBoundaryPackage>(0),
            ContractExecutionStatesEnum.ValidationError, 23));
      }
    }
  }
}
