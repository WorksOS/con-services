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
    public double BottomLeftX { get; set; }
    /// <summary>
    /// The bottom left corner of the bounding box, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "bottomleftY", Required = Required.Always)]
    [Required]
    public double BottomleftY { get; set; }
    /// <summary>
    /// The top right corner of the bounding box, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "topRightX", Required = Required.Always)]
    [Required]
    public double TopRightX { get; set; }
    /// <summary>
    /// The top right corner of the bounding box, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "topRightY", Required = Required.Always)]
    [Required]
    public double TopRightY { get; set; }

    /// <summary>
    /// Default private constructor
    /// </summary>
    private BoundingBox2DGrid()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="botLeftX"></param>
    /// <param name="botLeftY"></param>
    /// <param name="topRightX"></param>
    /// <param name="topRightY"></param>
    public BoundingBox2DGrid
    (
      double botLeftX,
      double botLeftY,
      double topRightX,
      double topRightY
    )
    {
      BottomLeftX = botLeftX;
      BottomleftY = botLeftY;
      TopRightX = topRightX;
      TopRightY = topRightY;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (BottomLeftX > TopRightX || BottomleftY > TopRightY)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid bounding box: corners are not bottom left and top right."));
      }
    }
  }
}