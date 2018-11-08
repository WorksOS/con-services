using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Utilities;

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
    private const int MIN_PATCH_SIZE = 1;
    private const int MAX_PATCH_SIZE = 1000;
    private const int MIN_PATCH_NUM = 0;
    private const int MAX_PATCH_NUM = 1000;

    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    [JsonProperty(PropertyName = "callId", Required = Required.Default)]
    public Guid? CallId { get; protected set; }

    /// <summary>
    /// The thematic mode to be rendered; elevation, compaction, temperature etc
    /// </summary>
    [JsonProperty(PropertyName = "mode", Required = Required.Always)]
    [Required]
    public DisplayMode Mode { get; private set; }

    /// <summary>
    /// The set of colours to be used to map the datum values in the thematic data to colours to be rendered in the tile.
    /// </summary>
    [JsonProperty(PropertyName = "palettes", Required = Required.Default)]
    public List<ColorPalette> Palettes { get; private set; }

    /// <summary>
    /// The settings to be used when considering compaction information being processed and analysed in preparation for rendering.
    /// </summary>
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
    public LiftBuildSettings LiftBuildSettings { get; private set; }

    /// <summary>
    /// Render the thematic data into colours using the supplied color palettes.
    /// </summary>
    [JsonProperty(PropertyName = "renderColorValues", Required = Required.Always)]
    [Required]
    public bool RenderColorValues { get; private set; }

    /// <summary>
    /// The volume computation type to use for summary volume thematic rendering
    /// </summary>
    [JsonProperty(PropertyName = "computeVolType", Required = Required.Default)]
    public VolumesType ComputeVolType { get; private set; }

    /// <summary>
    /// The tolerance to be used to indicate no change in volume for a cell. Used for summary volume thematic rendering. Value is expressed in meters.
    /// </summary>
    [Range(ValidationConstants3D.MIN_NO_CHANGE_TOLERANCE, ValidationConstants3D.MAX_NO_CHANGE_TOLERANCE)]
    [JsonProperty(PropertyName = "computeVolNoChangeTolerance", Required = Required.Default)]
    public double ComputeVolNoChangeTolerance { get; private set; }

    /// <summary>
    /// The descriptor for the design to be used for volume or cut/fill based thematic renderings.
    /// </summary>
    [JsonProperty(PropertyName = "designDescriptor", Required = Required.Default)]
    public DesignDescriptor DesignDescriptor { get; private set; }

    /// <summary>
    /// The base or earliest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter1", Required = Required.Default)]
    public FilterResult Filter1 { get; private set; }

    /// <summary>
    /// The ID of the base or earliest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filterId1", Required = Required.Default)]
    public long FilterId1 { get; private set; }

    /// <summary>
    /// The top or latest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter2", Required = Required.Default)]
    public FilterResult Filter2 { get; private set; }

    /// <summary>
    /// The ID of the top or latest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filterId2", Required = Required.Default)]
    public long FilterId2 { get; private set; }

    /// <summary>
    /// The method of filtering cell passes into layers to be used for thematic renderings that require layer analysis as an input into the rendered data.
    /// If this value is provided any layer method provided in a filter is ignored.
    /// </summary>
    [JsonProperty(PropertyName = "filterLayerMethod", Required = Required.Default)]
    public FilterLayerMethod FilterLayerMethod { get; private set; }

    /// <summary>
    /// The number of the patch of data to be requested in the overall series of patches covering the required dataset.
    /// </summary>
    [Range(MIN_PATCH_NUM, MAX_PATCH_NUM)]
    [JsonProperty(PropertyName = "patchNumber", Required = Required.Always)]
    [Required]
    public int PatchNumber { get; private set; }

    /// <summary>
    /// The number of subgrids to return in the patch
    /// </summary>
    [Range(MIN_PATCH_SIZE, MAX_PATCH_SIZE)]
    [JsonProperty(PropertyName = "patchSize", Required = Required.Always)]
    [Required]
    public int PatchSize { get; private set; }

    [JsonProperty(PropertyName = "includeTimeOffsets")]
    public bool IncludeTimeOffsets { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private PatchRequest()
    { }

    /// <summary>
    /// Create instance of PatchRequest
    /// </summary>
    public static PatchRequest Create(
      long projectId,
      Guid? callId,
      DisplayMode mode,
      List<ColorPalette> palettes,
      LiftBuildSettings liftBuildSettings,
       bool renderColorValues,
      VolumesType computeVolType,
      double computeVolNoChangeTolerance,
      DesignDescriptor designDescriptor,
      FilterResult filter1,
      long filterId1,
      FilterResult filter2,
      long filterId2,
      FilterLayerMethod filterLayerMethod,
      int patchNumber,
      int patchSize,
      bool includeTimeOffsets = false
      )
    {
      return new PatchRequest
      {
        ProjectId = projectId,
        CallId = callId,
        Mode = mode,
        Palettes = palettes,
        LiftBuildSettings = liftBuildSettings,
        RenderColorValues = renderColorValues,
        ComputeVolType = computeVolType,
        ComputeVolNoChangeTolerance = computeVolNoChangeTolerance,
        DesignDescriptor = designDescriptor,
        Filter1 = filter1,
        FilterId1 = filterId1,
        Filter2 = filter2,
        FilterId2 = filterId2,
        FilterLayerMethod = filterLayerMethod,
        PatchNumber = patchNumber,
        PatchSize = patchSize,
        IncludeTimeOffsets = includeTimeOffsets
      };
    }

    public override void Validate()
    {
      base.Validate();
      ValidatePalettes(Palettes, Mode);
      LiftBuildSettings?.Validate();

      ValidateDesign(DesignDescriptor, Mode, ComputeVolType);

      if (Mode == DisplayMode.VolumeCoverage)
      {
        ValidateVolumesFilters(ComputeVolType, Filter1, FilterId1, Filter2, FilterId2);
      }
    }
  }
}
