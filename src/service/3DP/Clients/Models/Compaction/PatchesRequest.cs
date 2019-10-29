using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Productivity3D.Models.Compaction
{
  /// <summary>
  /// Request for cut/fill data from earthworks.
  /// </summary>
  public class PatchesRequest
  {
    /// <summary>  EC520 serial number  </summary> 
    [JsonProperty(PropertyName = "ecSerial", Required = Required.Always)]
    public string ECSerial { get; private set; }

    /// <summary>  EC520 serial number  </summary> 
    [JsonProperty(PropertyName = "radioSerial", Required = Required.Default)]
    public string RadioSerial { get; private set; }

    /// <summary>  TCC OrgId  </summary> 
    [JsonProperty(PropertyName = "tccOrgUid", Required = Required.Default)]
    public string TccOrgUid { get; set; }
    
    /// <summary>  WGS84 latitude in decimal degrees.  </summary>
    [JsonProperty(PropertyName = "machineLatitude", Required = Required.Always)]
    public double MachineLatitude { get; private set; }

    /// <summary>  WGS84 longitude in decimal degrees.  </summary>
    [JsonProperty(PropertyName = "machineLongitude", Required = Required.Always)]
    public double MachineLongitude { get; private set; }

    /// <summary>  Bounding box in NE  </summary> 
    [JsonProperty(PropertyName = "boundingBox", Required = Required.Always)]
    public BoundingBox2DGrid BoundingBox { get; private set; }

    /// <summary>  Default private constructor.  </summary>
    private PatchesRequest()
    { }

    private const double MAX_PATCHES_SM_BOUNDARY = 2000;

    public PatchesRequest(string ecSerial, string radioSerial,
      string tccOrgUid,
      double machineLatitude, double machineLongitude,
      BoundingBox2DGrid boundingBox)
    {
      ECSerial = ecSerial;
      RadioSerial = radioSerial;
      TccOrgUid = tccOrgUid;
      MachineLatitude = machineLatitude;
      MachineLongitude = machineLongitude;
      BoundingBox = boundingBox;
    }

    public void Validate()
    {
      if (string.IsNullOrEmpty(ECSerial))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "ECSerial required"));
      }

      if (Math.Abs(MachineLatitude) > 90.0 || Math.Abs(MachineLongitude) > 180.0 ||
          (Math.Abs(MachineLatitude) < 2 && Math.Abs(MachineLongitude) < 2))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Invalid Machine Location"));
      }

      BoundingBox.Validate();

      // NE are in meters  
      if (((BoundingBox.TopRightX - BoundingBox.BottomLeftX) * (BoundingBox.TopRightY - BoundingBox.BottomleftY)) > MAX_PATCHES_SM_BOUNDARY)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Invalid bounding box: Must be {MAX_PATCHES_SM_BOUNDARY}m2 or less."));
      }
    }
  }
}
