using System;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;

namespace VSS.Productivity3D.TagFileAuth.Models
{
  /// <summary>
  /// Endpoint called by 3dp GetSubGridPatches service to identify device and potentially project
  /// </summary>
  public class GetProjectAndAssetUidsBaseRequest 
  {
    /// <summary>
    /// The serial number of platform operating on the device.
    ///     Should == the HardwareId or Serial from a tag file.
    ///     Actually this could be an EC520; EC520W; CBnnn or Marine platform
    ///        this is why the above 'DeviceType' is now obsolete
    /// </summary>
    [JsonProperty(PropertyName = "ec520Serial", Required = Required.Default)]
    public string Ec520Serial { get; set; }

    /// <summary>
    /// The SNM94n radio serial number of the radio on the device.
    ///      This is optional and probably never required 
    ///      May be a SNM or non-trimble, we don't really care.
    /// </summary>
    [JsonProperty(PropertyName = "radioSerial", Required = Required.Default)]
    public string ObsoleteRadioSerial { get; set; }

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
    /// Date and time the asset was at the given location. 
    /// </summary>
    [JsonProperty(PropertyName = "timeOfPosition", Required = Required.Always)]
    public DateTime ObsoleteTimeOfPosition { get; set; }

    [JsonIgnore]
    public bool HasLatLong => Math.Abs(Latitude) > 0.0 && Math.Abs(Longitude) > 0.0;

    public GetProjectAndAssetUidsBaseRequest() { }


    /// <summary>
    /// Create instance of GetProjectAndAssetUidsBaseRequest
    /// </summary>
    public GetProjectAndAssetUidsBaseRequest
    (string ec520Serial, string radioSerial, double latitude, double longitude, DateTime timeOfPosition)
    {
      Ec520Serial = ec520Serial;
      ObsoleteRadioSerial = radioSerial;
      Latitude = latitude;
      Longitude = longitude;
      ObsoleteTimeOfPosition = timeOfPosition;
    }

    public void Validate()
    {
      if (string.IsNullOrEmpty(Ec520Serial))
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsEarthWorksResult.FormatResult(uniqueCode: 51));
      
      if (Latitude < -90 || Latitude > 90)
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsEarthWorksResult.FormatResult(uniqueCode: 21));

      if (Longitude < -180 || Longitude > 180)
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsEarthWorksResult.FormatResult(uniqueCode: 22));

      if (!(ObsoleteTimeOfPosition > DateTime.UtcNow.AddYears(-50) && ObsoleteTimeOfPosition <= DateTime.UtcNow.AddDays(2)))
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsEarthWorksResult.FormatResult(uniqueCode: 23));
    }
  }
}
