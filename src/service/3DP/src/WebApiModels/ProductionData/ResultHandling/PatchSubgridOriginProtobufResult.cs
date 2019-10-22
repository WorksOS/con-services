using Newtonsoft.Json;
using ProtoBuf;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// A subgrid of information within a patch result
  /// </summary>
  [ProtoContract(SkipConstructor = true)]
  public class PatchSubgridOriginProtobufResult
  {
    /// <summary>
    /// Gets the northing patch origin in meters, as a delta.
    /// </summary>
    [ProtoMember(1, IsRequired = true)]
    [JsonProperty(PropertyName = "subgridOriginX")]
    public double SubgridOriginX { get; private set; }

    /// <summary>
    /// Gets the easting patch origin in meters, as a delta.
    /// </summary>
    [ProtoMember(2, IsRequired = true)]
    [JsonProperty(PropertyName = "subgridOriginY")]
    public double SubgridOriginY { get; private set; }

    /// <summary>
    /// The elevation origin referenced by all cell elevations in the binary representation of the patch subgrids.
    /// UTC expressed as Unix time in seconds.
    /// </summary>
    [ProtoMember(3, IsRequired = true)]
    [JsonProperty(PropertyName = "timeOrigin")]
    public uint TimeOrigin { get; set; }

    /// <summary>
    /// The elevation origin referenced by all cell elevations in the binary representation of the patch subgrids. Values are expressed in meters.
    /// </summary>
    [ProtoMember(4, IsRequired = true)]
    [JsonProperty(PropertyName = "elevationOrigin")]
    public float ElevationOrigin { get; set; }

    /// <summary>
    /// Elevation at the cell center point
    /// </summary>
    [JsonProperty(PropertyName = "elevationOffsets")]
    [ProtoMember(6, IsRequired = true, IsPacked = true)]
    public ushort[] ElevationOffsets { get; private set; }

    /// <summary>
    /// Elevation at the cell center point
    /// </summary>
    [JsonProperty(PropertyName = "timeOffsets")]
    [ProtoMember(7, IsRequired = true, IsPacked = true)]
    public uint[] TimeOffsets { get; private set; }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static PatchSubgridOriginProtobufResult Create(double subgridOriginX, double subgridOriginY, float elevationOrigin, uint timeOrigin, ushort[] elevationOffsets, uint[] timeOffsets)
    {
      return new PatchSubgridOriginProtobufResult
      {
        SubgridOriginX = subgridOriginX,
        SubgridOriginY = subgridOriginY,
        ElevationOrigin = elevationOrigin,
        TimeOrigin = timeOrigin,
        ElevationOffsets = elevationOffsets,
        TimeOffsets = timeOffsets
      };
    }
  }
}
