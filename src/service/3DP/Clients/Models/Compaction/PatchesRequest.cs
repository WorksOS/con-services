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
    [JsonProperty(PropertyName = "obsoleteRadioSerial", Required = Required.Default)]
    public string ObsoleteRadioSerial { get; private set; }

    /// <summary>  TCC OrgId  </summary> 
    [JsonProperty(PropertyName = "obsoleteTccOrgUid", Required = Required.Default)]
    public string ObsoleteTccOrgUid { get; set; }
    
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

    private const double MAX_BOUNDARY_SQUARE_METERS = 2000;

    public PatchesRequest(string ecSerial, string obsoleteRadioSerial,
      string obsoleteTccOrgUid,
      double machineLatitude, double machineLongitude,
      BoundingBox2DGrid boundingBox)
    {
      ECSerial = ecSerial;
      ObsoleteRadioSerial = obsoleteRadioSerial;
      ObsoleteTccOrgUid = obsoleteTccOrgUid;
      MachineLatitude = machineLatitude;
      MachineLongitude = machineLongitude;
      BoundingBox = boundingBox;
    }

    public void Validate()
    {
      // the response codes match those in TFA.
      if (string.IsNullOrEmpty(ECSerial))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(3037, "Platform serial number must be provided"));
      }

      if (Math.Abs(MachineLatitude) > 90.0 || Math.Abs(MachineLongitude) > 180.0 ||
          (Math.Abs(MachineLatitude) < 2 && Math.Abs(MachineLongitude) < 2))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(3021, "Invalid Machine Location"));
      }

      try
      {
        BoundingBox.Validate();
      }
      catch (Exception)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(3020, "Invalid bounding box: corners are not bottom left and top right."));
      }

      // NE are in meters  
      var areaSqMeters = (BoundingBox.TopRightX - BoundingBox.BottomLeftX) * (BoundingBox.TopRightY - BoundingBox.BottomleftY);
      if (areaSqMeters > MAX_BOUNDARY_SQUARE_METERS)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(3019, $"Invalid bounding box sqM: {areaSqMeters}. Must be {MAX_BOUNDARY_SQUARE_METERS}m2 or less."));
      }
    }
  }
}
