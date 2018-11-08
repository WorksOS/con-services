using Newtonsoft.Json;
using ProtoBuf;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// A single cell of information within a subgrid in a patch
  /// </summary>
  [ProtoContract (SkipConstructor = true)]
  public class PatchCellHeightResult
  {
    /// <summary>
    /// Elevation at the cell center point
    /// </summary>
    [JsonProperty(PropertyName = "elevationOffset")]
    [ProtoMember(1, IsRequired = true)]
    public ushort ElevationOffset { get; private set; }

    /// <summary>
    /// Elevation at the cell center point
    /// </summary>
    [JsonProperty(PropertyName = "timeOffset")]
    [ProtoMember(2, IsRequired = false)]
    public uint TimeOffset { get; private set; }

    public bool ShouldSerializeTimeOffset()
    {
      return TimeOffset != uint.MaxValue;
    }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static PatchCellHeightResult Create(ushort elevation, uint time)
    {
      return new PatchCellHeightResult
      {
        ElevationOffset = elevation,
        TimeOffset = time
      };
    }
  }
}
