using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// TFA v2 endpoint to retrieve ProjectUid for a tagfile
  ///      this is used by CTCT device
  /// </summary>
  public class GetProjectUidRequest 
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
    public int Validate()
    {
      // what VSS deviceTypes are these: torch=SNM940/941, aqua=?, R2=? 
      var allowedDeviceTypes = new List<int>() { (int)DeviceTypeEnum.MANUALDEVICE, (int)DeviceTypeEnum.SNM940, (int)DeviceTypeEnum.SNM941, (int)DeviceTypeEnum.EC520 };
      var isDeviceTypeValid = allowedDeviceTypes.Contains(deviceType);

      if (!isDeviceTypeValid)
      {
        return 30;
      }

      if (string.IsNullOrEmpty(radioSerial))
      {
       return 10;
      }

      if (latitude < -90 || latitude > 90)
      {
        return 21;
      }

      if (longitude < -180 || longitude > 180)
      {
        return 22;
      }

      if (!(timeOfPosition > DateTime.UtcNow.AddYears(-50) && timeOfPosition <= DateTime.UtcNow.AddDays(30)))
      {
        return 23;
      }

      return 0;
    }
  }
}