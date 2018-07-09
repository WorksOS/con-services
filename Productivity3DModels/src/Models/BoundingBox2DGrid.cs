using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Defines a bounding box representing a 2D grid coorindate area
  /// </summary>
  public class BoundingBox2DGrid 
  {
    /// <summary>
    /// The bottom left corner of the bounding box, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "bottomLeftX", Required = Required.Always)]
    [Required]
    public double bottomLeftX { get; set; }
    /// <summary>
    /// The bottom left corner of the bounding box, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "bottomleftY", Required = Required.Always)]
    [Required]
    public double bottomleftY { get; set; }
    /// <summary>
    /// The top right corner of the bounding box, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "topRightX", Required = Required.Always)]
    [Required]
    public double topRightX { get; set; }
    /// <summary>
    /// The top right corner of the bounding box, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "topRightY", Required = Required.Always)]
    [Required]
    public double topRightY { get; set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private BoundingBox2DGrid()
    { }

    /// <summary>
    /// Create instance of BoundingBox2DGrid
    /// </summary>
    public static BoundingBox2DGrid CreateBoundingBox2DGrid(
    double botLeftX,
    double botLeftY,
    double topRightX,
    double topRightY
    )
    {
      return new BoundingBox2DGrid
      {
        bottomLeftX = botLeftX,
        bottomleftY = botLeftY,
        topRightX = topRightX,
        topRightY = topRightY
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (bottomLeftX > topRightX || bottomleftY > topRightY)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid bounding box: corners are not bottom left and top right."));
      }
    }
  }
}