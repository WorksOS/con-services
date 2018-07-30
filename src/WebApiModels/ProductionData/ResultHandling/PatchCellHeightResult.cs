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
    private PatchCellHeightResult()
    { }

    /// <summary>
    /// Elevation at the cell center point
    /// </summary>
    [JsonProperty(PropertyName = "elevation")]
    [ProtoMember(1)]
    public ushort Elevation { get; private set; }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static PatchCellHeightResult Create(ushort elevation)
    {
      return new PatchCellHeightResult
      {
        Elevation = elevation
      };
    }
  }
}
