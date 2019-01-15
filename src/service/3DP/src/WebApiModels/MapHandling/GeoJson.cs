using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSS.Productivity3D.WebApi.Models.MapHandling
{
  /// <summary>
  /// Classes that are the model for the GeoJSON for design boundaries.
  /// </summary>
  public class GeoJson
  {
    internal class FeatureType
    {
      public const string FEATURE = "Feature";
      public const string FEATURE_COLLECTION = "FeatureCollection";
    }

    [JsonProperty(PropertyName = "type")]
    public string Type { get; set; }
    
    [JsonProperty(PropertyName = "features")]
    public List<Feature> Features { get; set; }
  }

  public class Feature
  {
    [JsonProperty(PropertyName = "type")]
    public string Type { get; set; }

    [JsonProperty(PropertyName = "geometry")]
    public Geometry Geometry { get; set; }

    [JsonProperty(PropertyName = "properties")]
    public Properties Properties { get; set; }
  }

  public class Geometry
  {
    internal class Types
    {
      public const string MULTI_LINE_STRING = "MultiLineString";
    }

    [JsonProperty(PropertyName = "type")]
    public string Type { get; set; }

    [JsonProperty(PropertyName = "coordinates")]
    public List<List<double[]>> Coordinates { get; set; }
  }

  public class Properties
  {
    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }
  }
}
