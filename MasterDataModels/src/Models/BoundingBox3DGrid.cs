using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Newtonsoft.Json;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// A 3D spatial extents structure
  /// TODO this is a copy from VSS.Productivity3D.Common.Models maybe we should only have one.....
  /// </summary>
  public class BoundingBox3DGrid
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
    private BoundingBox3DGrid()
    { }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static BoundingBox3DGrid CreatBoundingBox3DGrid(double minx, double miny, double minz, double maxx, double maxy, double maxz)
    {
      return new BoundingBox3DGrid
      {
        MinX = minx,
        MinY = miny,
        MinZ = minz,
        MaxX = maxx,
        MaxY = maxy,
        MaxZ = maxz,
      };
    }

    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>A string representation of the 3D extents values</returns>
    public override string ToString()
    {
      return $"minX:{MinX}, minY:{MinY}, minZ:{MinZ}, maxX:{MaxX}, maxY:{MaxY}, maxZ:{MaxZ}";
    }
  }
}

