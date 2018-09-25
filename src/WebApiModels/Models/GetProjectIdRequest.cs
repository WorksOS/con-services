using System;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models
{
  /// <summary>
  /// The request representation used to request the project Id that a specified asset is inside at a given location and date time.
  /// </summary>
  public class GetProjectIdRequest
  {
    /// <summary>
    /// The id of the asset whose tagfile is to be processed. A value of -1 indicates 'none' so all assets are considered (depending on tccOrgId). 
    /// </summary>
    [JsonProperty(PropertyName = "assetId", Required = Required.Always)]
    public long assetId { get; set; }

    /// <summary>
    /// WGS84 latitude in decimal degrees. 
    /// </summary>
    [JsonProperty(PropertyName = "latitude", Required = Required.Always)]
    public double latitude { get; set; }

    /// <summary>
    /// WGS84 longitude in decimal degrees. 
    /// </summary>    
    [JsonProperty(PropertyName = "longitude", Required = Required.Always)]
    public double longitude { get; set; }

    /// <summary>
    /// Elevation in meters. 
    /// </summary>
    [JsonProperty(PropertyName = "height", Required = Required.Always)]
    public double height { get; set; }

    /// <summary>
    /// Date and time the asset was at the given location. 
    /// </summary>
    [JsonProperty(PropertyName = "timeOfPosition", Required = Required.Always)]
    public DateTime timeOfPosition { get; set; }

    /// <summary>
    /// Date and time the asset was at the given location. 
    /// </summary>
    [JsonProperty(PropertyName = "tccOrgUid", Required = Required.Default)]
    public string tccOrgUid { get; set; }


    /// <summary>
    /// Private constructor
    /// </summary>
    private GetProjectIdRequest()
    {
    }

    /// <summary>
    /// Create instance of GetProjectIdRequest
    /// </summary>
    public static GetProjectIdRequest CreateGetProjectIdRequest(long assetId, double latitude, double longitude,
      double height, DateTime timeOfPosition, string tccOrgUid)
    {
      return new GetProjectIdRequest
      {
        assetId = assetId,
        latitude = latitude,
        longitude = longitude,
        height = height,
        timeOfPosition = timeOfPosition,
        tccOrgUid = tccOrgUid
      };
    }


    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (assetId <= 0 && string.IsNullOrEmpty(tccOrgUid))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectIdResult.CreateGetProjectIdResult(false, -1, 
            ContractExecutionStatesEnum.ValidationError, 20));
      }

      if (latitude < -90 || latitude > 90)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectIdResult.CreateGetProjectIdResult(false, -1, 
            ContractExecutionStatesEnum.ValidationError, 21));
      }

      if (longitude < -180 || longitude > 180)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectIdResult.CreateGetProjectIdResult(false, -1, 
            ContractExecutionStatesEnum.ValidationError, 22));
      }

      if (!(timeOfPosition > DateTime.UtcNow.AddYears(-50) && timeOfPosition <= DateTime.UtcNow.AddDays(30)))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectIdResult.CreateGetProjectIdResult(false, -1, 
            ContractExecutionStatesEnum.ValidationError, 23));
      }

    }
  }
}