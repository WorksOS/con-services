using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;

namespace VSS.Productivity3D.TagFileAuth.Models
{
  /// <summary>
  /// Endpoint called by TRex to validate tagFile data and identify device and potentially project
  /// todoJeannie investigate these obsolete members as I don't think we need them
  /// </summary>
  public class GetProjectAndAssetUidsRequest : GetProjectAndAssetUidsBaseRequest
  {
    /// <summary>
    /// if ProjectUid is supplied, this is a 'manual update'
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Default)]
    public string ProjectUid { get; set; }

    /// <summary>
    /// The device type of the machine. Valid values any, but normally 6=SNM940 (torch machines).
    ///     For the 
    ///     For the 3d earthworks patches endpoint we don't use this atm.  
    /// </summary>
    [JsonProperty(PropertyName = "deviceType", Required = Required.Always)]
    public int ObsoleteDeviceType { get; set; }

    /// <summary>
    /// Grid position NEE.
    ///     For the 3d earthworks patched endpoint we don't use this atm. 
    /// </summary>
    [JsonProperty(PropertyName = "northing", Required = Required.Default)]
    public double? Northing { get; set; }

    /// <summary>
    /// Grid position NEE.
    ///     For the 3d earthworks patched endpoint we don't use this atm.  
    /// </summary>    
    [JsonProperty(PropertyName = "easting", Required = Required.Default)]
    public double? Easting { get; set; }

    [JsonIgnore]
    public bool HasNE => Northing.HasValue && Easting.HasValue;

    public GetProjectAndAssetUidsRequest() { }


    public GetProjectAndAssetUidsRequest
      (string projectUid, int deviceType, string radioSerial, string ec520Serial,
        double latitude, double longitude, DateTime timeOfPosition,
        double? northing = null, double? easting = null) 
      : base(ec520Serial, radioSerial, latitude, longitude, timeOfPosition)
    {
      ProjectUid = projectUid;
      ObsoleteDeviceType = deviceType;
      Northing = northing;
      Easting = easting;
    }

    public new void Validate()
    {
      base.Validate();

      // if it has a projectUid, then it's a manual import and must have either assetUid or radio/dt  
      if (!string.IsNullOrEmpty(ProjectUid) && !Guid.TryParseExact(ProjectUid, "D", out var projectUid))
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsResult.FormatResult(ProjectUid, uniqueCode: 36));

      var allowedDeviceTypes = new List<int>() { (int)TagFileDeviceTypeEnum.ManualImport, (int)TagFileDeviceTypeEnum.SNM940, (int)TagFileDeviceTypeEnum.SNM941, (int)TagFileDeviceTypeEnum.EC520 };
      var isDeviceTypeValid = allowedDeviceTypes.Contains(ObsoleteDeviceType);

      if (!isDeviceTypeValid)
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 30));

      if (string.IsNullOrEmpty(ProjectUid) && string.IsNullOrEmpty(ObsoleteRadioSerial) && string.IsNullOrEmpty(Ec520Serial))
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 37));

      if (!HasLatLong && !HasNE)
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 54));
      
      // NE can be negative and zero

      if (!(ObsoleteTimeOfPosition > DateTime.UtcNow.AddYears(-50) && ObsoleteTimeOfPosition <= DateTime.UtcNow.AddDays(2)))
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest, GetProjectAndAssetUidsResult.FormatResult(uniqueCode: 23));
    }
  }
}
