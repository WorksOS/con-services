using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.Designs;
using VSS.TRex.Geometry;
using VSS.TRex.Rendering.Palettes;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Models
{
  /// <summary>
  /// The request representation for rendering a tile of thematic information such as elevation, compaction, temperature etc.
  /// The bounding box of the area to be rendered may be specified in either WGS84 lat/lon or cartesian grid coordinates in the project coordinate system.
  /// </summary>
  public class TileRequest : ProjectUID
  {
    private const int MIN_PIXELS = 64;
    private const int MAX_PIXELS = 4096;

    /// <summary>
    /// Default private constructor.
    /// </summary>
    protected TileRequest()
    {
      // ...
    }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static TileRequest CreateTileRequest(string projectUID)
    {
      return new TileRequest()
      {
        projectUid = projectUID
      };
    }

    /// <summary>
    /// The thematic mode to be rendered; elevation, compaction, temperature etc
    /// </summary>
    [JsonProperty(PropertyName = "mode", Required = Required.Always)]
    [Required]
    public DisplayMode Mode { get; protected set; }

    /* TODO
    /// <summary>
    /// The set of colours to be used to map the datum values in the thematic data to colours to be rendered in the tile.
    /// In case of cut/fill data rendering the transition order should be datum value descendent.
    /// </summary>
    [JsonProperty(PropertyName = "palettes", Required = Required.Default)]
    //Use default palette
    public List<ColorPalette> Palettes { get; protected set; }
    */

    /// <summary>
    /// The design to be used in cases of cut/fill subgrid requests
    /// </summary>
    [JsonProperty(PropertyName = "cutFillDesignID", Required = Required.Default)]
    public Guid CutFillDesignID { get; set; }

    /// <summary>
    /// The base or earliest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter1", Required = Required.Default)]
    public FilterResult Filter1 { get; protected set; }

    /// <summary>
    /// The top or latest filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter2", Required = Required.Default)]
    public FilterResult Filter2 { get; protected set; }

    /// <summary>
    /// The bounding box enclosing the area to be rendered.
    /// </summary>
    [JsonProperty(PropertyName = "extents", Required = Required.Always)]
    [Required]
    public BoundingWorldExtent3D Extents { get; protected set; }

    /// <summary>
    /// CoordsAreGrid controls whether the plan (XY/NE) coordinates in the spatial filters are to 
    /// be interpreted as rectangular cartesian coordinates or as WGS84 latitude/longitude coordinates
    /// </summary>
    public bool CoordsAreGrid { get; set; }
   
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
    public ushort Height { get; protected set; }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      //ValidatePalettes(Palettes, mode);

      Filter1.Validate();
      Filter2.Validate();

      if (!Guid.TryParse(projectUid, out var _siteModelID))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"Invalid projectUid={projectUid}, siteModelID={_siteModelID}"));
      }

      // TODO...
      //ValidatePalette(Palettes, mode);

      if (Extents == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Extents are required"));
      }
    }
  }
}
