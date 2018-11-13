using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Defines a bounding box representing a 3D area
  /// </summary>
  public class BoundingExtents3D 
  {
    /// <summary>
    /// The minimum X coord
    /// </summary>
    [JsonProperty(PropertyName = "minX", Required = Required.Always)]
    [Required]
    public double MinX { get; set; }

    /// <summary>
    /// The minimum Y coord
    /// </summary>
    [JsonProperty(PropertyName = "minY", Required = Required.Always)]
    [Required]
    public double MinY { get; set; }

    /// <summary>
    /// The minimum Z coord
    /// </summary>
    [JsonProperty(PropertyName = "minZ", Required = Required.Always)]
    [Required]
    public double MinZ { get; set; }

    /// <summary>
    /// The maximum X coord
    /// </summary>
    [JsonProperty(PropertyName = "maxX", Required = Required.Always)]
    [Required]
    public double MaxX { get; set; }

    /// <summary>
    /// The maximum Y coord
    /// </summary>
    [JsonProperty(PropertyName = "maxY", Required = Required.Always)]
    [Required]
    public double MaxY { get; set; }

    /// <summary>
    /// The maximum Z coord
    /// </summary>
    [JsonProperty(PropertyName = "maxZ", Required = Required.Always)]
    [Required]
    public double MaxZ { get; set; }

    /// <summary>
    /// Default private constructor
    /// </summary>
    private BoundingExtents3D()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="minX"></param>
    /// <param name="minY"></param>
    /// <param name="minZ"></param>
    /// <param name="maxX"></param>
    /// <param name="maxY"></param>
    /// <param name="maxZ"></param>
    public BoundingExtents3D
    (
      double minX,
      double minY,
      double minZ,
      double maxX,
      double maxY,
      double maxZ
    )
    {
      MinX = minX;
      MinY = minY;
      MinZ = minZ;
      MaxX = maxX;
      MaxY = maxY;
      MaxZ = maxZ;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (MinX > MaxX || MinY > MaxY || MinZ > MaxZ)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid bounding box: corners are not bottom left and top right."));
      }
    }
  }
}