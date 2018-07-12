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
  public class GetProjectAndAssetUidsRequest : ContractRequest
  {

    // TFHarvester workflows:
    // workflow #1 Manual import
    //    projectUid is supplied
    //       RadioSerial must also be supplied - else error
    //             Validate BOTH exist
    //             Validate correct subscription/s  

    // workflow #2 Auto import
    //    projectUid is NOT supplied
    //    There must be a way to identify a customer
    //     a) radioSerial and DeviceType or b) tccOrgId
    //             Validate BOTH exist
    //             Validate correct subscription/s 

    // workflow #3 Overide (test scenario only, will NOT call TFA)
    //    projectUid and AssetUid are used to apply a tag file directly to a Project

    /// <summary>
    /// if ProjectUid is supplied, this is a 'manual update'
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Default)]
    public string ProjectUid { get; set; }

    ///// <summary>
    ///// if assetUid is supplied, this is a 'manual update'
    ///// </summary>
    //[JsonProperty(PropertyName = "assetUid", Required = Required.Default)]
    //public string AssetUid { get; set; }


    /// <summary>
    /// The device type of the machine. Valid values any, but normally 6=SNM940 (torch machines).
    /// </summary>
    [JsonProperty(PropertyName = "deviceType", Required = Required.Always)]
    public int DeviceType { get; set; }

    /// <summary>
    /// The radio serial number of the machine from the tagfile.
    /// </summary>
    [JsonProperty(PropertyName = "radioSerial", Required = Required.Default)]
    public string RadioSerial { get; set; }

    // workflow #3 tccOrgUid 
    //             Validate correct subscription/s : ????

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
    private GetProjectAndAssetUidsRequest()
    {
    }

    /// <summary>
    /// Create instance of GetProjectAndAssetUidsRequest
    /// </summary>
    public static GetProjectAndAssetUidsRequest CreateGetProjectAndAssetUidsRequest
    (string projectUid, int deviceType, string radioSerial, string tccOrgUid,
      double latitude, double longitude, DateTime timeOfPosition)
    {
      return new GetProjectAndAssetUidsRequest
      {
        ProjectUid = projectUid,
        DeviceType = deviceType,
        RadioSerial = radioSerial,
        TccOrgUid = tccOrgUid,
        Latitude = latitude,
        Longitude = longitude,
        TimeOfPosition = timeOfPosition
      };
    }


    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      // if it has a projectUid, then it's a manual import and must have either assetUid or radio/dt
      if (!string.IsNullOrEmpty(ProjectUid) && !Guid.TryParseExact(ProjectUid, "D", out var projectUid))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult("", "", 36));
      }

      var isDeviceTypeValid = (((DeviceTypeEnum) DeviceType).ToString() != DeviceType.ToString());

      if (!isDeviceTypeValid)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult("", "", 30));
      }

      if (!string.IsNullOrEmpty(projectUid.ToString()) && string.IsNullOrEmpty(RadioSerial) && string.IsNullOrEmpty(TccOrgUid))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult("", "", 37));
      }


      if (Latitude < -90 || Latitude > 90)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult("", "", 21));
      }

      if (Longitude < -180 || Longitude > 180)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult("", "", 22));
      }

      if (!(TimeOfPosition > DateTime.UtcNow.AddYears(-50) && TimeOfPosition <= DateTime.UtcNow.AddDays(30)))
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          GetProjectAndAssetUidsResult.CreateGetProjectAndAssetUidsResult("", "", 23));
      }

    }
  }
}