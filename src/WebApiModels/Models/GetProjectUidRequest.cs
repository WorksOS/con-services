using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models
{
  /// <summary>
  /// The request representation used to request the project Uid that a specified asset is inside at a given location and date time.
  /// </summary>
  public class GetProjectUidRequest : ContractRequest
  {
    /// <summary>
    /// The device type of the machine. Valid values any, but normally 6=SNM940 (torch machines).
    /// </summary>
    [JsonProperty(PropertyName = "deviceType", Required = Required.Always)]
    public int deviceType { get; set; }

    /// <summary>
    /// The radio serial number of the machine from the tagfile.
    /// </summary>
    [JsonProperty(PropertyName = "radioSerial", Required = Required.Default)]
    public string radioSerial { get; set; }

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
    /// Date and time the asset was at the given location. 
    /// </summary>
    [JsonProperty(PropertyName = "timeOfPosition", Required = Required.Always)]
    public DateTime timeOfPosition { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private GetProjectUidRequest()
    {
    }

    /// <summary>
    /// Create instance of GetProjectUidRequest
    /// </summary>
    public static GetProjectUidRequest CreateGetProjectUidRequest(int deviceType, string radioSerial, 
      double latitude, double longitude, DateTime timeOfPosition)
    {
      return new GetProjectUidRequest
      {
        deviceType = deviceType,
        radioSerial = radioSerial,
        latitude = latitude,
        longitude = longitude,
        timeOfPosition = timeOfPosition
      };
    }


    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      // todo what VSS deviceTypes are these: torch, aqua, R2; 
      var allowedDeviceTypes = new List<int>() {(int)DeviceTypeEnum.SNM940, (int)DeviceTypeEnum.SNM941};
      var isDeviceTypeValid = allowedDeviceTypes.Contains(deviceType);

      if (!isDeviceTypeValid)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectUidResult.CreateGetProjectUidResult("", 30));
      }

      if (string.IsNullOrEmpty(radioSerial) )
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectUidResult.CreateGetProjectUidResult("", 10));
      }

      if (latitude < -90 || latitude > 90)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectUidResult.CreateGetProjectUidResult("", 21));
      }

      if (longitude < -180 || longitude > 180)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectUidResult.CreateGetProjectUidResult("", 22));
      }

      if (!(timeOfPosition > DateTime.UtcNow.AddYears(-50) && timeOfPosition <= DateTime.UtcNow.AddDays(30)))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectUidResult.CreateGetProjectUidResult("", 23));
      }

    }
  }
}