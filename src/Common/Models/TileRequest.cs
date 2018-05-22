using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.Utilities;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// The request representation for rendering a tile of thematic information such as elevation, compaction, temperature etc
  /// The bounding box of the area to be rendered may be specified in either WGS84 lat/lon or cartesian grid coordinates in the project coordinate system.
  /// </summary>
  public class TileRequest : RaptorHelper
  {
    private const int MIN_PIXELS = 64;
    private const int MAX_PIXELS = 4096;

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
    public DisplayMode mode { get; protected set; }

    /// <summary>
    /// The set of colours to be used to map the datum values in the thematic data to colours to be rendered in the tile.
    /// In case of cut/fill data rendering the transition order should be datum value descendent.
    /// </summary>
    [JsonProperty(PropertyName = "palettes", Required = Required.Default)]
    //Use default palette
    public List<ColorPalette> Palettes { get; protected set; }

    /// <summary>
    /// Color to be used to render subgrids representationaly when the production data is zoomed too far away.
    /// </summary>
    /// <value>
    /// The display color of the representational.
    /// </value>
    [JsonProperty(PropertyName = "representationalDisplayColor", Required = Required.Default)]
    public uint RepresentationalDisplayColor { get; protected set; }

    /// <summary>
    /// The settings to be used when considering compaction information being processed and analysed in preparation for rendering.
    /// </summary>
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
    public LiftBuildSettings LiftBuildSettings { get; protected set; }

    /// <summary>
    /// The volume computation type to use for summary volume thematic rendering
    /// </summary>
    [JsonProperty(PropertyName = "computeVolType", Required = Required.Default)]
    public RaptorConverters.VolumesType ComputeVolumesType { get; protected set; }

    /// <summary>
    /// The tolerance to be used to indicate no change in volume for a cell. Used for summary volume thematic rendering. Value is expressed in meters.
    /// </summary>
    [Range(ValidationConstants.MIN_NO_CHANGE_TOLERANCE, ValidationConstants.MAX_NO_CHANGE_TOLERANCE)]
    [JsonProperty(PropertyName = "computeVolNoChangeTolerance", Required = Required.Default)]
    public double ComputeVolNoChangeTolerance { get; protected set; }

    /// <summary>
    /// The descriptor for the design to be used for volume or cut/fill based thematic renderings.
    /// </summary>
    [JsonProperty(PropertyName = "designDescriptor", Required = Required.Default)]
    public DesignDescriptor DesignDescriptor { get; protected set; }

    /// <summary>
    /// The base or earliest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter1", Required = Required.Default)]
    public FilterResult Filter1 { get; protected set; }

    /// <summary>
    /// The ID of the base or earliest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filterId1", Required = Required.Default)]
    public long FilterId1 { get; protected set; }

    /// <summary>
    /// The top or latest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter2", Required = Required.Default)]
    public FilterResult Filter2 { get; protected set; }

    /// <summary>
    /// The ID of the top or latest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filterId2", Required = Required.Default)]
    public long FilterId2 { get; protected set; }

    /// <summary>
    /// The method of filtering cell passes into layers to be used for thematic renderings that require layer analysis as an input into the rendered data.
    /// If this value is provided any layer method provided in a filter is ignored.
    /// </summary>
    [JsonProperty(PropertyName = "filterLayerMethod", Required = Required.Default)]
    public FilterLayerMethod FilterLayerMethod { get; protected set; }

    /// <summary>
    /// The bounding box enclosing the area to be rendered. The bounding box is expressed in terms of WGS84 latitude and longitude positions, expressed in radians.
    /// Value may be null but either this or the bounding box in grid coordinates must be provided.
    /// </summary>
    [JsonProperty(PropertyName = "boundBoxLL", Required = Required.Default)]
    public BoundingBox2DLatLon BoundBoxLatLon { get; protected set; }

    /// <summary>
    /// The bounding box enclosing the area to be rendered. The bounding box is expressed in terms of cartesian grid coordinates in the project coordinate system, expressed in meters.
    /// Value may be null but either this or the bounding box in lat/lng coordinates must be provided.
    /// </summary>
    [JsonProperty(PropertyName = "boundBoxGrid", Required = Required.Default)]
    public BoundingBox2DGrid BoundBoxGrid { get; protected set; }

    /// <summary>
    /// The width, in pixels, of the image tile to be rendered
    /// </summary>
    [Range(MIN_PIXELS, MAX_PIXELS)]
    [JsonProperty(PropertyName = "width", Required = Required.Always)]
    [Required]
    public ushort Width { get; protected set; }

    /// <summary>
    /// The height, in pixels, of the image tile to be rendered
    /// </summary>
    [Range(MIN_PIXELS, MAX_PIXELS)]
    [JsonProperty(PropertyName = "height", Required = Required.Always)]
    [Required]
    public ushort height { get; protected set; }

    [JsonIgnore]
    public bool IsSummaryVolumeCutFillRequest { get; set; }

    /// <summary>
    /// Default private constructor.
    /// </summary>
    protected TileRequest()
    { }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static TileRequest CreateTileRequest(
        long projectId,
        Guid? callId,
        DisplayMode mode,
        List<ColorPalette> palettes,
        LiftBuildSettings liftBuildSettings,
        RaptorConverters.VolumesType computeVolType,
        double computeVolNoChangeTolerance,
        DesignDescriptor designDescriptor,
        FilterResult filter1,
        long filterId1,
        FilterResult filter2,
        long filterId2,
        FilterLayerMethod filterLayerMethod,
        BoundingBox2DLatLon boundingBoxLatLon,
        BoundingBox2DGrid boundingBoxGrid,
        ushort width,
        ushort height,
        uint representationalDisplayColor = 0,
        uint cmvDetailsColorNumber = 5,
        uint cmvPercentChangeColorNumber = 6,
        bool setSummaryDataLayersVisibility = true)
    {
      return new TileRequest
      {
        ProjectId = projectId,
        CallId = callId,
        mode = mode,
        Palettes = palettes,
        LiftBuildSettings = liftBuildSettings,
        ComputeVolumesType = computeVolType,
        ComputeVolNoChangeTolerance = computeVolNoChangeTolerance,
        DesignDescriptor = designDescriptor,
        Filter1 = filter1,
        FilterId1 = filterId1,
        Filter2 = filter2,
        FilterId2 = filterId2,
        FilterLayerMethod = filterLayerMethod,
        BoundBoxLatLon = boundingBoxLatLon,
        BoundBoxGrid = boundingBoxGrid,
        Width = width,
        height = height,
        RepresentationalDisplayColor = representationalDisplayColor,
        cmvDetailsColorNumber = cmvDetailsColorNumber,
        cmvPercentChangeColorNumber = cmvPercentChangeColorNumber,
        setSummaryDataLayersVisibility = setSummaryDataLayersVisibility
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();
      ValidatePalettes(Palettes, mode);

      //Compaction settings
      LiftBuildSettings?.Validate();

      //Volumes
      //mode == DisplayMode.VolumeCoverage
      //computeVolNoChangeTolerance and computeVolType must be provided but since not nullable types they always will have a value anyway
      ValidateDesign(DesignDescriptor, mode, ComputeVolumesType);

      //Summary volumes: v1 has mode VolumeCoverage, v2 has mode CutFill but computeVolType is set
      if (mode == DisplayMode.VolumeCoverage ||
         (mode == DisplayMode.CutFill &&
         (ComputeVolumesType == RaptorConverters.VolumesType.Between2Filters ||
         ComputeVolumesType == RaptorConverters.VolumesType.BetweenDesignAndFilter ||
         ComputeVolumesType == RaptorConverters.VolumesType.BetweenFilterAndDesign)))
      {
        ValidateVolumesFilters(ComputeVolumesType, this.Filter1, this.FilterId1, this.Filter2, this.FilterId2);
      }

      if (BoundBoxLatLon == null && BoundBoxGrid == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Bounding box required either in lat/lng or grid coordinates"));

      }

      if (BoundBoxLatLon != null && BoundBoxGrid != null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Only one bounding box is allowed"));
      }

      if (mode == DisplayMode.TargetThicknessSummary && LiftBuildSettings.liftThicknessTarget == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "For this mode LiftThickness Target in LIftBuildSettings must be specified."));
      }

      if (mode == DisplayMode.TargetSpeedSummary && LiftBuildSettings.machineSpeedTarget == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "For this mode SpeedSummary Target in LiftBuildSettings must be specified."));
      }
    }
  }
}
