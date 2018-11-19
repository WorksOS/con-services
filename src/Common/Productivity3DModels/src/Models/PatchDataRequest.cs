using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Utilities;

namespace VSS.Productivity3D.Models.Models
{
  public class PatchDataRequest : ProjectID
  {
    /// <summary>
    /// The base or earliest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter1", Required = Required.Default)]
    public FilterResult Filter1 { get; private set; }

    /// <summary>
    /// The top or latest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter2", Required = Required.Default)]
    public FilterResult Filter2 { get; private set; }

    /// <summary>
    /// The thematic mode to be rendered; elevation, compaction, temperature etc
    /// </summary>
    [JsonProperty(PropertyName = "mode", Required = Required.Always)]
    [Required]
    public DisplayMode Mode { get; private set; }

    /// <summary>
    /// The number of the patch of data to be requested in the overall series of patches covering the required dataset.
    /// </summary>
    [Range(ValidationConstants3D.MIN_PATCH_NUM, ValidationConstants3D.MAX_PATCH_NUM)]
    [JsonProperty(PropertyName = "patchNumber", Required = Required.Always)]
    [Required]
    public int PatchNumber { get; private set; }

    /// <summary>
    /// The number of subgrids to return in the patch
    /// </summary>
    [Range(ValidationConstants3D.MIN_PATCH_SIZE, ValidationConstants3D.MAX_PATCH_SIZE)]
    [JsonProperty(PropertyName = "patchSize", Required = Required.Always)]
    [Required]
    public int PatchSize { get; private set; }


  }
}
