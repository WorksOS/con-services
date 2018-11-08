using System.Collections.Generic;

namespace VSS.Tile.Service.Common.Models
{
  /// <summary>
  /// Classes that are the model for the GeoJSON for design boundaries.
  /// </summary>
  /// 
  public class Geometry
  {
    public string type { get; set; }
    public List<List<List<double>>> coordinates { get; set; }
  }

  public class Properties
  {
    public string name { get; set; }
  }

  public class Feature
  {
    public string type { get; set; }
    public Geometry geometry { get; set; }
    public Properties properties { get; set; }
  }

  public class RootObject
  {
    public string type { get; set; }
    public List<Feature> features { get; set; }
  }

}
