using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Models.Validation;

namespace VSS.Productivity3D.Models.Models
{
  public class TRexTileRequest
  {
    private const int MIN_PIXELS = 64;
    private const int MAX_PIXELS = 4096;

    /// <summary>
    /// A project unique identifier.
    /// </summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Always)]
    [ValidProjectUID]

    public Guid? ProjectUid { get; private set; }
    /// <summary>
    /// The thematic mode to be rendered; elevation, compaction, temperature etc
    /// </summary>
    [JsonProperty(PropertyName = "mode", Required = Required.Always)]
    [Required]
    public DisplayMode Mode { get; private set; }

    /// <summary>
    /// The set of colours to be used to map the datum values in the thematic data to colours to be rendered in the tile.
    /// In case of cut/fill data rendering the transition order should be datum value descendent.
    /// </summary>
    [JsonProperty(PropertyName = "palettes", Required = Required.Default)]
    //Use default palette
    public List<ColorPalette> Palettes { get; private set; }

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
    /// The top or latest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter2", Required = Required.Default)]
    public FilterResult Filter2 { get; private set; }

    /// <summary>
    /// The bounding box enclosing the area to be rendered. The bounding box is expressed in terms of WGS84 latitude and longitude positions, expressed in radians.
    /// Value may be null but either this or the bounding box in grid coordinates must be provided.
    /// </summary>
    [JsonProperty(PropertyName = "boundBoxLL", Required = Required.Default)]
    public BoundingBox2DLatLon BoundBoxLatLon { get; private set; }

    /// <summary>
    /// The bounding box enclosing the area to be rendered. The bounding box is expressed in terms of cartesian grid coordinates in the project coordinate system, expressed in meters.
    /// Value may be null but either this or the bounding box in lat/lng coordinates must be provided.
    /// </summary>
    [JsonProperty(PropertyName = "boundBoxGrid", Required = Required.Default)]
    public BoundingBox2DGrid BoundBoxGrid { get; private set; }

    /// <summary>
    /// The width, in pixels, of the image tile to be rendered
    /// </summary>
    [Range(MIN_PIXELS, MAX_PIXELS)]
    [JsonProperty(Required = Required.Always)]
    [Required]
    public ushort Width { get; private set; }

    /// <summary>
    /// The height, in pixels, of the image tile to be rendered
    /// </summary>
    [Range(MIN_PIXELS, MAX_PIXELS)]
    [JsonProperty(Required = Required.Always)]
    [Required]
    public ushort Height { get; private set; }

    /// <summary>
    /// Any overriding machine targets
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    public OverridingTargets Overrides { get; private set; }

    /// <summary>
    /// Default public constructor.
    /// </summary>
    public TRexTileRequest()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public TRexTileRequest(
      Guid? projectUid,
      DisplayMode mode,
      List<ColorPalette> palettes,
      DesignDescriptor designDescriptor,
      FilterResult filter1,
      FilterResult filter2,
      BoundingBox2DLatLon boundingBoxLatLon,
      BoundingBox2DGrid boundingBoxGrid,
      ushort width,
      ushort height,
      OverridingTargets overrides)
    {
      ProjectUid = projectUid;
      Mode = mode;
      Palettes = palettes;
      DesignDescriptor = designDescriptor;
      Filter1 = filter1;
      Filter2 = filter2;
      BoundBoxLatLon = boundingBoxLatLon;
      BoundBoxGrid = boundingBoxGrid;
      Width = width;
      Height = height;
      Overrides = overrides;
    }

    /// <summary>
    /// Validates all properties.
    /// </summary>
    public void Validate()
    {
      Overrides?.Validate();
      DesignDescriptor?.Validate();

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

      if (Mode == DisplayMode.TargetSpeedSummary && Overrides?.MachineSpeedTarget == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "For this mode SpeedSummary Target in Overrides must be specified."));
      }
    }
  }
}
