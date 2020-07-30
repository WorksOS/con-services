using System;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;

namespace VSS.Productivity3D.TagFileAuth.Models
{
  /// <summary>
  /// Endpoint called by 3dp GetSubGridPatches service to identify device and potentially project
  /// </summary>
  public class GetProjectUidsBaseRequest
  {
    /// <summary>
    /// The serial number of platform operating on the device.
    ///     Should == the HardwareId or Serial from a tag file.
    ///     Actually this could be an EC520; EC520W; CBnnn or Marine platform
    ///        this is why the above 'DeviceType' is now obsolete
    /// </summary>
    [JsonProperty(PropertyName = "platformSerial", Required = Required.Default)]
    public string PlatformSerial { get; set; }
    
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

    [JsonIgnore]
    public bool HasLatLong => Math.Abs(Latitude) > 0.0 && Math.Abs(Longitude) > 0.0;

    public GetProjectUidsBaseRequest() { }


    /// <summary>
    /// Create instance of platformSerial
    /// </summary>
    public GetProjectUidsBaseRequest
    (string platformSerial, double latitude, double longitude)
    {
      PlatformSerial = platformSerial;
      Latitude = latitude;
      Longitude = longitude;
    }

    public void Validate()
    {
      if (string.IsNullOrEmpty(PlatformSerial))
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 51));

      // todoJeannie remove messages 23; 30; 37 from Contractexe plus the TRexTagFileResultCode in common 
      
      if (Latitude < -90 || Latitude > 90)
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 21));

      if (Longitude < -180 || Longitude > 180)
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 22));
    }
  }
}
