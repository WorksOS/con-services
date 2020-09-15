using System;
using Newtonsoft.Json;
using VSS.Productivity3D.Productivity3D.Models.ProductionData.ResultHandling;

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

    public PatchesRequest(string ecSerial, 
      double machineLatitude, double machineLongitude,
      BoundingBox2DGrid boundingBox)
    {
      ECSerial = ecSerial;
      MachineLatitude = machineLatitude;
      MachineLongitude = machineLongitude;
      BoundingBox = boundingBox;
    }

    public PatchSubgridsProtobufResult Validate()
    {
      // the response codes match those in TFA.
      if (string.IsNullOrEmpty(ECSerial))
      {
        return new PatchSubgridsProtobufResult(3037, "Platform serial number must be provided");
      }

      if (Math.Abs(MachineLatitude) > 90.0 || Math.Abs(MachineLongitude) > 180.0 ||
          (Math.Abs(MachineLatitude) < 2 && Math.Abs(MachineLongitude) < 2))
      {
        return new PatchSubgridsProtobufResult(3021, "Invalid Machine Location");
      }

      try
      {
        BoundingBox.Validate();
      }
      catch (Exception)
      {
        return new PatchSubgridsProtobufResult(3020, "Invalid bounding box: corners are not bottom left and top right.");
      }

      // NE are in meters  
      var areaSqMeters = (BoundingBox.TopRightX - BoundingBox.BottomLeftX) * (BoundingBox.TopRightY - BoundingBox.BottomleftY);
      if (areaSqMeters > MAX_BOUNDARY_SQUARE_METERS)
      {
        return new PatchSubgridsProtobufResult(3019, $"Invalid bounding box sqM: {Math.Round(areaSqMeters, 4)}. Must be {MAX_BOUNDARY_SQUARE_METERS}m2 or less.");
      }
      return new PatchSubgridsProtobufResult(0, "success");

    }
  }
}
