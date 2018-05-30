using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;

namespace VSS.Productivity3D.WebApiModels.Compaction.Models
{

  /// <summary>
  /// The request representation for rendering a tile of thematic information such as elevation, compaction, temperature etc.
  /// The bounding box of the area to be rendered may be specified in WGS84 lat/lon.
  /// This is a simplified contract for compaction. The full contract is TileRequest.
  /// </summary>
  public class CompactionTileRequest : ProjectID, IValidatable
  {
    /// <summary>
    /// The thematic mode to be rendered; elevation, compaction, temperature etc
    /// </summary>
    [JsonProperty(PropertyName = "mode", Required = Required.Always)]
    [Required]
    public DisplayMode mode { get; private set; }

    /// <summary>
    /// The filter to be used.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public CompactionFilter filter { get; private set; }

    /// <summary>
    /// The bounding box enclosing the area to be rendered. The bounding box is expressed in terms of WGS84 latitude and longitude positions, expressed in radians.
    /// </summary>
    [JsonProperty(PropertyName = "boundBoxLL", Required = Required.Default)]
    public BoundingBox2DLatLon boundBoxLL { get; private set; }

    /// <summary>
    /// The width, in pixels, of the image tile to be rendered
    /// </summary>
    [Range(MIN_PIXELS, MAX_PIXELS)]
    [JsonProperty(PropertyName = "width", Required = Required.Always)]
    [Required]
    public ushort width { get; private set; }

    /// <summary>
    /// The height, in pixels, of the image tile to be rendered
    /// </summary>
    [Range(MIN_PIXELS, MAX_PIXELS)]
    [JsonProperty(PropertyName = "height", Required = Required.Always)]
    [Required]
    public ushort height { get; private set; }


    /// <summary>
    /// Private constructor
    /// </summary>
    private CompactionTileRequest()
    { }

    /// <summary>
    /// Create instance of CompactionTileRequest
    /// </summary>
    public static CompactionTileRequest CreateTileRequest(
        long projectId,
        DisplayMode mode,
        CompactionFilter filter,
        BoundingBox2DLatLon boundingBoxLatLon,
        ushort width,
        ushort height
      )
    {
      return new CompactionTileRequest
      {
        ProjectId = projectId,
        mode = mode,
        filter = filter,
        boundBoxLL = boundingBoxLatLon,
        width = width,
        height = height
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();
      if (filter != null)
      {
        filter.Validate();
      }

      if (boundBoxLL == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
             new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                 "Bounding box required in lat/lng"));

      }
    }

    private const int MIN_PIXELS = 64;
    private const int MAX_PIXELS = 4096;


  }
}
