using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// A 3D spatial extents structure
  /// </summary>
  public class BoundingBox3DGrid: IMasterDataModel
  {
    /// <summary>
    /// Maximum X value, in the cartesian grid coordinate system, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "maxX", Required = Required.Always)]
    [Required]
    public double MaxX { get; set; }

    /// <summary>
    /// Maximum Y value, in the cartesian grid coordinate system, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "maxY", Required = Required.Always)]
    [Required]
    public double MaxY { get; set; }

    /// <summary>
    /// Maximum Z value, in the cartesian grid coordinate system, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "maxZ", Required = Required.Always)]
    [Required]
    public double MaxZ { get; set; }

    /// <summary>
    /// Minimum X value, in the cartesian grid coordinate system, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "minX", Required = Required.Always)]
    [Required]
    public double MinX { get; set; }

    /// <summary>
    /// Minimum Y value, in the cartesian grid coordinate system, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "minY", Required = Required.Always)]
    [Required]
    public double MinY { get; set; }

    /// <summary>
    /// Minimum Z value, in the cartesian grid coordinate system, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "minZ", Required = Required.Always)]
    [Required]
    public double MinZ { get; set; }

    /// <summary>
    /// Prevents a default instance of the <see cref="BoundingBox3DGrid"/> class from being created.
    /// </summary>
    public BoundingBox3DGrid() { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public BoundingBox3DGrid(double minx, double miny, double minz, double maxx, double maxy, double maxz)
    {
      MinX = minx;
      MinY = miny;
      MinZ = minz;
      MaxX = maxx;
      MaxY = maxy;
      MaxZ = maxz;
    }

    //Project without any tagfile data will have these values for extents from TRex
    public static readonly double MIN_RANGE = -1E100;
    public static readonly double MAX_RANGE = 1E100;
    [JsonIgnore]
    public bool ValidExtents => !(MinX == MAX_RANGE && MaxX == MIN_RANGE &&
                                  MinY == MAX_RANGE && MaxY == MIN_RANGE &&
                                  MinZ == MAX_RANGE && MaxZ == MIN_RANGE);

    public bool EmptyExtents => MinX == 0 && MaxX == 0 && MinY == 0 && MaxY == 0 && MinZ == 0 && MaxZ == 0;

    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>A string representation of the 3D extents values</returns>
    public override string ToString()
    {
      return $"minX:{MinX}, minY:{MinY}, minZ:{MinZ}, maxX:{MaxX}, maxY:{MaxY}, maxZ:{MaxZ}";
    }
    public List<string> GetIdentifiers() => new List<string>();
  }
}
