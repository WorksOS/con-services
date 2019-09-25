using Newtonsoft.Json;
using ProtoBuf;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// A single cell of information within a subgrid in a patch
  /// </summary>
  [ProtoContract (SkipConstructor = true)]
  public class PatchCellSimpleResult : PatchCellSimpleResultBase
  {
    /// <summary>
    /// Gets the northing patch origin in meters, as a delta.
    /// </summary>
    [ProtoMember(1, IsRequired = true)]
    [JsonProperty(PropertyName = "worldOriginX")]
    public double WorldOriginX { get; private set; }

    /// <summary>
    /// Gets the easting patch origin in meters, as a delta.
    /// </summary>
    [ProtoMember(2, IsRequired = true)]
    [JsonProperty(PropertyName = "worldOriginY")]
    public double WorldOriginY { get; private set; }


    /// <summary>
    /// Elevation at the cell center point
    /// </summary>
    [ProtoMember(3, IsRequired = true)]
    [JsonProperty(PropertyName = "elevation")]
    public double Elevation { get; private set; }

    /// <summary>
    /// Time machine recorded this
    /// </summary>
    [ProtoMember(4, IsRequired = true)]
    [JsonProperty(PropertyName = "eventTime")]
    public uint EventTime { get; private set; }

    
    /// <summary>
    /// Static constructor.
    /// </summary>
    public static PatchCellSimpleResult Create(double worldOriginX, double worldOriginY, double elevation, uint eventTime)
    {
      return new PatchCellSimpleResult
      {
        WorldOriginX = worldOriginX,
        WorldOriginY = worldOriginY,
        Elevation = elevation,
        EventTime = eventTime
      };
    }
  }
}
