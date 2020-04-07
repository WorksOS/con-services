using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;

namespace VSS.Productivity3D.TagFileAuth.Models
{
  /// <summary>
  /// TFA v4 endpoint to retrieve ProjectUid and/or DeviceUid for a tagfile
  /// </summary>
  public class GetProjectAndAssetUidsRequest 
  {
    /// <summary>
    /// if ProjectUid is supplied, this is a 'manual update'
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Default)]
    public string ProjectUid { get; set; }

    /// <summary>
    /// The device type of the machine. Valid values any, but normally 6=SNM940 (torch machines).
    /// </summary>
    [JsonProperty(PropertyName = "deviceType", Required = Required.Always)]
    public int DeviceType { get; set; }

    /// <summary>
    /// The SNM94n radio serial number of the machine from the tagfile.
    ///  todoFromThis we can determine CB type
    /// </summary>
    [JsonProperty(PropertyName = "radioSerial", Required = Required.Default)]
    public string RadioSerial { get; set; }

    /// <summary>
    /// The EC520 serial number of the machine from the tagfile.
    /// </summary>
    [JsonProperty(PropertyName = "ec520Serial", Required = Required.Default)]
    public string Ec520Serial { get; set; }

    /// <summary>
    /// WGS84 latitude in decimal degrees. 
    /// </summary>
    [JsonProperty(PropertyName = "latitude", Required = Required.Always)]
    public double Latitude { get; set; }

    /// <summary>
    /// WGS84 longitude in decimal degrees. 
    /// </summary>    
    [JsonProperty(PropertyName = "longitude", Required = Required.Always)]
    public double Longitude { get; set; }

    /// <summary>
    /// Date and time the device was at the given location. 
    /// </summary>
    [JsonProperty(PropertyName = "timeOfPosition", Required = Required.Always)]
    public DateTime TimeOfPosition { get; set; }

    private GetProjectAndAssetUidsRequest()
    { }

    public GetProjectAndAssetUidsRequest
    ( string projectUid, int deviceType, string radioSerial, string ec520Serial, 
      double latitude, double longitude, DateTime timeOfPosition)
    {
      ProjectUid = projectUid;
      DeviceType = deviceType;
      RadioSerial = radioSerial;
      Ec520Serial = ec520Serial;
      Latitude = latitude;
      Longitude = longitude;
      TimeOfPosition = timeOfPosition;
    }

    public void Validate()
    {
      // if it has a projectUid, then it's a manual import and must have either assetUid or radio/dt  
      if (!string.IsNullOrEmpty(ProjectUid) && !Guid.TryParseExact(ProjectUid, "D", out var projectUid))
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsResult.FormatResult(ProjectUid, uniqueCode: 36));

      var allowedDeviceTypes = new List<int>() { (int)TagFileDeviceTypeEnum.ManualImport, (int)TagFileDeviceTypeEnum.SNM940, (int)TagFileDeviceTypeEnum.SNM941, (int)TagFileDeviceTypeEnum.EC520 };
      var isDeviceTypeValid = allowedDeviceTypes.Contains(DeviceType);

      if (!isDeviceTypeValid)
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 30));

      if (string.IsNullOrEmpty(ProjectUid) && string.IsNullOrEmpty(RadioSerial) && string.IsNullOrEmpty(Ec520Serial))
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 37));
      
      if (Latitude < -90 || Latitude > 90)
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 21));
      
      if (Longitude < -180 || Longitude > 180)
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 22));
      
      if (!(TimeOfPosition > DateTime.UtcNow.AddYears(-50) && TimeOfPosition <= DateTime.UtcNow.AddDays(2)))
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 23));
    }
  }
}
