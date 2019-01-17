using System.Collections.Generic;
using Newtonsoft.Json;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class GeoJson : ResponseBase
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
