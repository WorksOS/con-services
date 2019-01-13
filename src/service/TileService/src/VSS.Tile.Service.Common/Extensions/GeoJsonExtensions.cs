using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.Tile.Service.Common.Models;

namespace VSS.Tile.Service.Common.Extensions
{
  public static class GeoJsonExtensions
  {
    public static IEnumerable<IEnumerable<WGSPoint>> GeoJsonToPoints(this string geoJson)
    {
      var polygons = new List<List<WGSPoint>>();
      if (string.IsNullOrEmpty(geoJson)) return polygons;
      var root = JsonConvert.DeserializeObject<RootObject>(geoJson);
      return root.GeoJsonToPoints();
    }

    public static IEnumerable<IEnumerable<WGSPoint>> GeoJsonToPoints(this RootObject geoJson)
    {
      var polygons = new List<List<WGSPoint>>();
      if (geoJson.features == null) return polygons;
      foreach (var feature in geoJson.features)
      {
        if (feature.geometry != null)
        {
          var points = new List<WGSPoint>();
          foreach (var coordList in feature.geometry.coordinates)
          {
            foreach (var coordPair in coordList)
            {
              points.Add(new WGSPoint(coordPair[1].LatDegreesToRadians(),
                coordPair[0].LonDegreesToRadians())); //GeoJSON is lng/lat
            }
          }
          polygons.Add(points);
        }
      }
      return polygons;
    }
  }
}
