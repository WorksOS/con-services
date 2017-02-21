using System;
using Newtonsoft.Json;

namespace VSS.Raptor.Service.Common.Models
{
  /// <summary>
  /// A 3D spatial extents structure
  /// </summary>
  public class BoundingBox3DGrid
  {
    /// <summary>
    /// Maximum X value, in the cartesian grid coordinate system, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "maxX", Required = Required.Always)]
    public double maxX { get; set; }

    /// <summary>
    /// Maximum Y value, in the cartesian grid coordinate system, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "maxY", Required = Required.Always)]
    public double maxY { get; set; }

    /// <summary>
    /// Maximum Z value, in the cartesian grid coordinate system, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "maxZ", Required = Required.Always)]
    public double maxZ { get; set; }

    /// <summary>
    /// Minimum X value, in the cartesian grid coordinate system, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "minX", Required = Required.Always)]
    public double minX { get; set; }

    /// <summary>
    /// Minimum Y value, in the cartesian grid coordinate system, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "minY", Required = Required.Always)]
    public double minY { get; set; }

    /// <summary>
    /// Minimum Z value, in the cartesian grid coordinate system, expressed in meters
    /// </summary>
    [JsonProperty(PropertyName = "minZ", Required = Required.Always)]
    public double minZ { get; set; }

    /// <summary>
    /// Get Help sample for the object
    /// </summary>
    public static BoundingBox3DGrid HelpSample
    {
      get
      {
        return new BoundingBox3DGrid() {maxX = 100, maxY = 200, maxZ = 300, minX = 50, minY = 45, minZ = 10};
      }
    }

    /// <summary>
    /// Prevents a default instance of the <see cref="BoundingBox3DGrid"/> class from being created.
    /// </summary>
    private BoundingBox3DGrid()
    { }

    public static BoundingBox3DGrid CreatBoundingBox3DGrid(double minx, double miny, double minz, double maxx, double maxy, double maxz)
    {
      return new BoundingBox3DGrid()
             {
                 minX = minx,
                 minY = miny,
                 minZ = minz,
                 maxX = maxx,
                 maxY = maxy,
                 maxZ = maxz,
             };
    }

    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>A string representation of the 3D extents values</returns>
    public override string ToString()
    {
      return String.Format("minX:{0}, minY:{1}, minZ:{2}, maxX:{3}, maxY:{4}, maxZ:{5}", minX, minY, minZ, maxX, maxY, maxZ);
    }
  }
}