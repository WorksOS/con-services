using System;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Utilities;

namespace VSS.Productivity3D.Models.Models
{
  public class PatchDataRequest : TRexBaseRequest
  {
    /// <summary>
    /// The top or latest filter to be used.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public FilterResult Filter2 { get; private set; }

    /// <summary>
    /// The thematic mode to be rendered; elevation, compaction, temperature etc
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public DisplayMode Mode { get; private set; }

    /// <summary>
    /// The number of the patch of data to be requested in the overall series of patches covering the required data set.
    /// </summary>
    [Range(ValidationConstants3D.MIN_PATCH_NUM, ValidationConstants3D.MAX_PATCH_NUM)]
    [JsonProperty(Required = Required.Always)]
    [Required]
    public int PatchNumber { get; private set; }

    /// <summary>
    /// The number of sub grids to return in the patch
    /// </summary>
    [Range(ValidationConstants3D.MIN_PATCH_SIZE, ValidationConstants3D.MAX_PATCH_SIZE)]
    [JsonProperty(Required = Required.Always)]
    [Required]
    public int PatchSize { get; private set; }

    /// <summary>
    /// Default private constructor
    /// </summary>
    private PatchDataRequest()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public PatchDataRequest(
      Guid projectUid,
      FilterResult filter1,
      FilterResult filter2,
      DisplayMode mode,
      int patchNumber,
      int patchSize,
      OverridingTargets overrides,
      LiftSettings liftSettings
    )
    {
      ProjectUid = projectUid;
      Filter = filter1;
      Filter2 = filter2;
      Mode = mode;
      PatchNumber = patchNumber;
      PatchSize = patchSize;
      Overrides = overrides;
      LiftSettings = liftSettings;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      Filter2?.Validate();
    }
  }
}
