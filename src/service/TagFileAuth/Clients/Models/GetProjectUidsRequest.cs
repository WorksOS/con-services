using System;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;

namespace VSS.Productivity3D.TagFileAuth.Models
{
  /// <summary>
  /// Endpoint called by TRex to validate tagFile data and identify device and potentially project
  /// </summary>
  public class GetProjectUidsRequest : GetProjectUidsBaseRequest
  {
    /// <summary>
    /// if ProjectUid is supplied, this is a 'manual update'
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Default)]
    public string ProjectUid { get; set; }

    /// <summary>
    /// Grid position NEE.
    /// </summary>
    [JsonProperty(PropertyName = "northing", Required = Required.Default)]
    public double? Northing { get; set; }

    /// <summary>
    /// Grid position NEE.
    /// </summary>    
    [JsonProperty(PropertyName = "easting", Required = Required.Default)]
    public double? Easting { get; set; }

    [JsonIgnore]
    public bool HasNE => Northing.HasValue && Easting.HasValue;

    public GetProjectUidsRequest() { }


    public GetProjectUidsRequest
      (string projectUid, string platformSerial,
        double latitude, double longitude, 
        double? northing = null, double? easting = null) 
      : base(platformSerial, latitude, longitude)
    {
      ProjectUid = projectUid;
      Northing = northing;
      Easting = easting;
    }

    public new void Validate()
    {
      base.Validate();

      // if it has a projectUid, then it's a manual import and must have either assetUid or radio/dt  
      if (!string.IsNullOrEmpty(ProjectUid) && !Guid.TryParse(ProjectUid, out var projectUid))
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsResult.FormatResult(ProjectUid, uniqueCode: 36));
     
      if (!HasLatLong && !HasNE)
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 54));
      
      // NE can be negative and zero
    }
  }
}
