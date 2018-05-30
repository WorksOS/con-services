using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.Utilities;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  /// <summary>
  /// The representation of a Patch request. A patch defines a series of subgrids of cell data returned to the caller. patchNumber and patchSize control which patch of
  /// subgrid and cell data need to be returned within the overall set of patches that comprise the overall data set identified by the thematic dataset, filtering and
  /// analytics parameters within the request.
  /// Requesting patch number 0 will additionally return a summation of the total number of patches of the requested size that need to be requested in order to assemble the
  /// complete data set.
  /// </summary>
  public class PatchRequest : RaptorHelper
  {
    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    [JsonProperty(PropertyName = "callId", Required = Required.Default)]
    public Guid? callId { get; protected set; }

    /// <summary>
    /// The thematic mode to be rendered; elevation, compaction, temperature etc
    /// </summary>
    [JsonProperty(PropertyName = "mode", Required = Required.Always)]
    [Required]
    public DisplayMode mode { get; private set; }

    /// <summary>
    /// The set of colours to be used to map the datum values in the thematic data to colours to be rendered in the tile.
    /// </summary>
    [JsonProperty(PropertyName = "palettes", Required = Required.Default)]
    public List<ColorPalette> palettes { get; private set; }

    /// <summary>
    /// The settings to be used when considering compaction information being processed and analysed in preparation for rendering.
    /// </summary>
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
    public LiftBuildSettings liftBuildSettings { get; private set; }

    /// <summary>
    /// Render the thematic data into colours using the supplied color palettes.
    /// </summary>
    [JsonProperty(PropertyName = "renderColorValues", Required = Required.Always)]
    [Required]
    public bool renderColorValues { get; private set; }

    /// <summary>
    /// The volume computation type to use for summary volume thematic rendering
    /// </summary>
    [JsonProperty(PropertyName = "computeVolType", Required = Required.Default)]
    public RaptorConverters.VolumesType computeVolType { get; private set; }

    /// <summary>
    /// The tolerance to be used to indicate no change in volume for a cell. Used for summary volume thematic rendering. Value is expressed in meters.
    /// </summary>
    [Range(ValidationConstants.MIN_NO_CHANGE_TOLERANCE, ValidationConstants.MAX_NO_CHANGE_TOLERANCE)]
    [JsonProperty(PropertyName = "computeVolNoChangeTolerance", Required = Required.Default)]
    public double computeVolNoChangeTolerance { get; private set; }

    /// <summary>
    /// The descriptor for the design to be used for volume or cut/fill based thematic renderings.
    /// </summary>
    [JsonProperty(PropertyName = "designDescriptor", Required = Required.Default)]
    public DesignDescriptor designDescriptor { get; private set; }

    /// <summary>
    /// The base or earliest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter1", Required = Required.Default)]
    public FilterResult filter1 { get; private set; }

    /// <summary>
    /// The ID of the base or earliest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filterId1", Required = Required.Default)]
    public long filterId1 { get; private set; }

    /// <summary>
    /// The top or latest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter2", Required = Required.Default)]
    public FilterResult filter2 { get; private set; }

    /// <summary>
    /// The ID of the top or latest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filterId2", Required = Required.Default)]
    public long filterId2 { get; private set; }

    /// <summary>
    /// The method of filtering cell passes into layers to be used for thematic renderings that require layer analysis as an input into the rendered data.
    /// If this value is provided any layer method provided in a filter is ignored.
    /// </summary>
    [JsonProperty(PropertyName = "filterLayerMethod", Required = Required.Default)]
    public FilterLayerMethod filterLayerMethod { get; private set; }

    /// <summary>
    /// The number of the patch of data to be requested in the overall series of patches covering the required dataset.
    /// </summary>
    [Range(MIN_PATCH_NUM, MAX_PATCH_NUM)]
    [JsonProperty(PropertyName = "patchNumber", Required = Required.Always)]
    [Required]
    public int patchNumber { get; private set; }

    /// <summary>
    /// The number of subgrids to return in the patch
    /// </summary>
    [Range(MIN_PATCH_SIZE, MAX_PATCH_SIZE)]
    [JsonProperty(PropertyName = "patchSize", Required = Required.Always)]
    [Required]
    public int patchSize { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private PatchRequest()
    { }

    /// <summary>
    /// Create instance of PatchRequest
    /// </summary>
    public static PatchRequest CreatePatchRequest(
      long projectId,
      Guid? callId,
      DisplayMode mode,
      List<ColorPalette> palettes,
      LiftBuildSettings liftBuildSettings,
       bool renderColorValues,
      RaptorConverters.VolumesType computeVolType,
      double computeVolNoChangeTolerance,
      DesignDescriptor designDescriptor,
      FilterResult filter1,
      long filterId1,
      FilterResult filter2,
      long filterId2,
      FilterLayerMethod filterLayerMethod,
      int patchNumber,
      int patchSize
      )
    {
      return new PatchRequest
      {
        ProjectId = projectId,
        callId = callId,
        mode = mode,
        palettes = palettes,
        liftBuildSettings = liftBuildSettings,
        renderColorValues = renderColorValues,
        computeVolType = computeVolType,
        computeVolNoChangeTolerance = computeVolNoChangeTolerance,
        designDescriptor = designDescriptor,
        filter1 = filter1,
        filterId1 = filterId1,
        filter2 = filter2,
        filterId2 = filterId2,
        filterLayerMethod = filterLayerMethod,
        patchNumber = patchNumber,
        patchSize = patchSize
      };
    }

    public override void Validate()
    {
      base.Validate();
      ValidatePalettes(palettes, mode);
      liftBuildSettings?.Validate();

      ValidateDesign(designDescriptor, mode, computeVolType);

      if (mode == DisplayMode.VolumeCoverage)
      {
        ValidateVolumesFilters(computeVolType, filter1, filterId1, filter2, filterId2);
      }
    }

    private const int MIN_PATCH_SIZE = 1;
    private const int MAX_PATCH_SIZE = 1000;
    private const int MIN_PATCH_NUM = 0;
    private const int MAX_PATCH_NUM = 1000;
  }
}
