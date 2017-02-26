using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon
{
  /// <summary>
  /// The request representation used to request the project Id that a specified asset is inside at a given location and date time.
  /// </summary>
  public class GetProjectIdRequest // : IValidatable, IServiceDomainObject, IHelpSample
  {
    /// <summary>
    /// The id of the asset whose tagfile is to be processed. A value of -1 indicates 'none' so all assets are considered. 
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "assetId", Required = Required.Always)]
    public long assetId { get; private set; }

    /// <summary>
    /// WGS84 latitude in decimal degrees. 
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "latitude", Required = Required.Always)]
    public double latitude { get; private set; }

    /// <summary>
    /// WGS84 longitude in decimal degrees. 
    /// </summary>    
    [Required]
    [JsonProperty(PropertyName = "longitude", Required = Required.Always)]
    public double longitude { get; private set; }

    /// <summary>
    /// Elevation in meters. 
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "height", Required = Required.Always)]
    public double height { get; private set; }

    /// <summary>
    /// Date and time the asset was at the given location. 
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "timeOfPosition", Required = Required.Always)]
    public DateTime timeOfPosition { get; private set; }

    ///// <summary>
    ///// Private constructor
    ///// </summary>
    //private GetProjectIdRequest()
    //{ }

    /// <summary>
    /// Create instance of GetProjectIdRequest
    /// </summary>
    public static GetProjectIdRequest CreateGetProjectIdRequest(
      long assetId,
      double latitude,
      double longitude,
      double height,
      DateTime timeOfPosition,
      string TCCOrgUID
      )
    {
      return new GetProjectIdRequest
      {
        assetId = assetId,
        latitude = latitude,
        longitude = longitude,
        height = height,
        timeOfPosition = timeOfPosition
      };
    }

    /// <summary>
    /// Example for Help
    /// </summary>
    public static GetProjectIdRequest HelpSample
    {
      get
      {
        return CreateGetProjectIdRequest(1892337661625085, -43.544566584363544, 172.59246826171878, 1.2, DateTime.UtcNow.AddMinutes(-1), "");
      }
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      const double EPSILON = 10e-8;

      if (latitude < -Math.PI / 2 + EPSILON || latitude > Math.PI / 2 - EPSILON)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            String.Format("Latitude value of {0} should be between -90 degrees and 90 degrees", latitude)));
      }

      if (longitude < -Math.PI + EPSILON || longitude > Math.PI - EPSILON)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            String.Format("Longitude value of {0} should be between -180 degrees and 180 degrees", longitude)));
      }
    }
  }
}