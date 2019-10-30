using System;
using Newtonsoft.Json;
using VSS.Common.Exceptions;

namespace VSS.Productivity3D.TagFileAuth.Models
{
  /// <summary>
  /// TFA v2 endpoint to retrieve ProjectUid and/or AssetUid and subscription indicator for a tagfile.
  ///      this is used by the 3dp GetSubGridPatches endpoint used by EarthWorks for cut-fill maps.
  /// </summary>
  public class GetProjectAndAssetUidsEarthWorksRequest 
  {
    /// <summary>
    /// The EC520 serial number of the machine from the tagfile.
    /// </summary>
    [JsonProperty(PropertyName = "ec520Serial", Required = Required.Default)]
    public string Ec520Serial { get; set; }

    /// <summary>
    /// The SNM94n radio serial number of the machine from the tagfile.
    /// </summary>
    [JsonProperty(PropertyName = "radioSerial", Required = Required.Default)]
    public string RadioSerial { get; set; }

    /// <summary>
    /// Date and time the asset was at the given location. 
    /// </summary>
    [JsonProperty(PropertyName = "tccOrgUid", Required = Required.Default)]
    public string TccOrgUid { get; set; }

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
    public DateTime TimeOfPosition { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private GetProjectAndAssetUidsEarthWorksRequest()
    { }

    /// <summary>
    /// Create instance of GetProjectAndAssetUidsEarthWorksRequest
    /// </summary>
    public GetProjectAndAssetUidsEarthWorksRequest
    (string ec520Serial, string radioSerial, string tccOrgUid,
      double latitude, double longitude, DateTime timeOfPosition)
    {
      Ec520Serial = ec520Serial;
      RadioSerial = radioSerial;
      TccOrgUid = tccOrgUid;
      Latitude = latitude;
      Longitude = longitude;
      TimeOfPosition = timeOfPosition;
    }

    public int Validate()
    {
      if (string.IsNullOrEmpty(Ec520Serial))
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsEarthWorksResult.FormatResult(uniqueCode: 51));

      if (Latitude < -90 || Latitude > 90)
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsEarthWorksResult.FormatResult(uniqueCode: 21));

      if (Longitude < -180 || Longitude > 180)
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsEarthWorksResult.FormatResult(uniqueCode: 22));

      if (!(TimeOfPosition > DateTime.UtcNow.AddYears(-50) && TimeOfPosition <= DateTime.UtcNow.AddDays(30)))
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsEarthWorksResult.FormatResult(uniqueCode: 23));

      return 0;
    }
  }
}
