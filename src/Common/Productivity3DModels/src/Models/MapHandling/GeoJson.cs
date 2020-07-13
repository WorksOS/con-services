using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models.MapHandling
{
  /// <summary>
  /// Classes that are the model for the GeoJSON for design boundaries.
  /// </summary>
  public class GeoJson
  {
    public class FeatureType
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
    public BaseGeometry Geometry { get; set; }

    [JsonProperty(PropertyName = "properties")]
    public Properties Properties { get; set; }
  }

  public class BaseGeometry
  {
    [JsonProperty(PropertyName = "type")]
    protected string Type { get; set; }
  }

  public class CenterlineGeometry : BaseGeometry
  {
    [JsonProperty(PropertyName = "coordinates")]
    public List<double[]> CenterlineCoordinates { get; set; } = new List<double[]>();

    public CenterlineGeometry()
    {
      Type = GeometryTypes.LINESTRING;
    }
  }

  public class FenceGeometry: BaseGeometry
  {
    [JsonProperty(PropertyName = "coordinates")]
    public List<List<double[]>> FenceCoordinates { get; set; } = new List<List<double[]>>();

    public FenceGeometry()
    {
      Type = GeometryTypes.POLYGON;
    }
  }

  public class GeometryTypes
  {
    public const string LINESTRING = "LineString";
    public const string POLYGON = "Polygon";
    public const string MULTI_LINE_STRING = "MultiLineString";
  }

  public class Properties
  {
    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }
  }
}
