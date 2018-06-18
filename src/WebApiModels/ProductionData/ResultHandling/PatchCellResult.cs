using Newtonsoft.Json;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  /// <summary>
  /// A single cell of information within a subgrid in a patch
  /// </summary>
  public class PatchCellResult
  {
    private PatchCellResult()
    { }

    /// <summary>
    /// Elevation at the cell center point
    /// </summary>
    [JsonProperty(PropertyName = "elevation")]
    public float Elevation { get; private set; }

    /// <summary>
    /// Requested thematic datum. Intepretation and parsing depends on the thematic domain
    /// </summary>
    [JsonProperty(PropertyName = "datum")]
    public ushort Datum { get; private set; }

    /// <summary>
    /// The color the cell is rendered in. Only present if renderColorValues is true.
    /// </summary>
    [JsonProperty(PropertyName = "color")]
    public uint Color { get; private set; }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static PatchCellResult CreatePatchCellResult(float elevation, ushort datum, uint color)
    {
      return new PatchCellResult
      {
        Elevation = elevation,
        Datum = datum,
        Color = color
      };
    }
  }
}
