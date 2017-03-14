using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon
{
  /// <summary>
  /// The request representation used to request the project Id that a specified asset is inside at a given location and date time.
  /// </summary>
  public class GetProjectIdRequest 
  {

    private long _assetId;
    private double _latitude;
    private double _longitude;
    private DateTime _timeOfPosition;
    private string _tccOrgUid;

    /// <summary>
    /// The id of the asset whose tagfile is to be processed. A value of -1 indicates 'none' so all assets are considered (depending on tccOrgId). 
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "assetId", Required = Required.Always)]
    public long assetId { get { return _assetId; } private set { _assetId = value; } }

    /// <summary>
    /// WGS84 latitude in decimal degrees. 
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "latitude", Required = Required.Always)]
    public double latitude { get { return _latitude; } private set { _latitude = value; } }

    /// <summary>
    /// WGS84 longitude in decimal degrees. 
    /// </summary>    
    [Required]
    [JsonProperty(PropertyName = "longitude", Required = Required.Always)]
    public double longitude { get { return _longitude; } private set { _longitude = value; } }

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
    public DateTime timeOfPosition { get { return _timeOfPosition; } private set { _timeOfPosition = value; } }

    /// <summary>
    /// Date and time the asset was at the given location. 
    /// </summary>
    [Required]
    [JsonProperty(PropertyName = "tccOrgUid", Required = Required.Always)]
    public string tccOrgUid { get { return _tccOrgUid; } private set { _tccOrgUid = value; } }

    
    /// <summary>
    /// Private constructor
    /// </summary>
    private GetProjectIdRequest()
    { }

    /// <summary>
    /// Create instance of GetProjectIdRequest
    /// </summary>
    public static GetProjectIdRequest CreateGetProjectIdRequest( long assetId, double latitude, double longitude, double height, DateTime timeOfPosition, string tccOrgUid)
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
    /// Example for Help
    /// </summary>
    public static GetProjectIdRequest HelpSample
    {
      get
      {
        return CreateGetProjectIdRequest(1892337661625085, -43.544566584363544, 172.59246826171878, 1.2, DateTime.UtcNow.AddMinutes(-1), "476434f7-a87a-4c8a-b5cc-ab98afa3964a");
      }
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (assetId <= 0 && string.IsNullOrEmpty(tccOrgUid))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            String.Format("Must contain one or more of assetId {0} or tccOrgId {1}", assetId, tccOrgUid)));
      }

      if (latitude < -90 || latitude > 90)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            String.Format("Latitude value of {0} should be between -90 degrees and 90 degrees", latitude)));
      }

      if (longitude < -180 || longitude > 180)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            String.Format("Longitude value of {0} should be between -180 degrees and 180 degrees", longitude)));
      }

      if (!(timeOfPosition > DateTime.UtcNow.AddYears(-5) && timeOfPosition <= DateTime.UtcNow))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            String.Format("timeOfPosition must have occured within last 5 years {0}", timeOfPosition)));
      }

    }
  }
}