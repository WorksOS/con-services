using System;

namespace VSS.Map3D.Common
{
  /// <summary>
  /// WGS84 Bounding box
  /// </summary>
  public class LLBoundingBox
  {
    public Boolean InRadians = false;

    private MapPoint minPoint;
    private MapPoint maxPoint;
    public MapPoint MinPoint { get => minPoint; set=>minPoint = value; }
    public MapPoint MaxPoint { get => maxPoint; set=>maxPoint = value; }

    public Double West {get => minPoint.Longitude; set => minPoint.Longitude = value;}
    public Double East {get => maxPoint.Longitude; set => maxPoint.Longitude = value;}
    public Double South {get => minPoint.Latitude; set => minPoint.Latitude = value;}
    public Double North {get => maxPoint.Latitude; set => maxPoint.Latitude = value;}

    public LLBoundingBox(MapPoint min, MapPoint max)
    {
      MinPoint = min;
      MaxPoint = max;
    }
    public LLBoundingBox(Double west, Double south, Double east, Double north, Boolean inRadians = true)
    {
      minPoint.Longitude = west;
      minPoint.Latitude  = south;
      maxPoint.Longitude = east;
      maxPoint.Latitude  = north;
      InRadians = inRadians;
    }

    public MapPoint GetCenter()
    {
      if (InRadians)
        {
           var pt = MapUtil.MidPointLL(MapUtil.Rad2Deg(minPoint.Latitude), MapUtil.Rad2Deg(minPoint.Longitude),
                MapUtil.Rad2Deg(maxPoint.Latitude), MapUtil.Rad2Deg(maxPoint.Longitude));
           pt.Latitude = MapUtil.Deg2Rad(pt.Latitude);
           pt.Longitude = MapUtil.Deg2Rad(pt.Longitude);
           return pt;
        }
      else
        return MapUtil.MidPointLL(minPoint.Latitude,minPoint.Longitude,maxPoint.Latitude,maxPoint.Longitude);
    }

    public Double ToDistance(double lonPt1, double latPt1, double lonPt2, double latPt2)
    {
      if (InRadians)
        return MapUtil.GetDistance( MapUtil.Rad2Deg(lonPt1), MapUtil.Rad2Deg(latPt1), MapUtil.Rad2Deg(lonPt2), MapUtil.Rad2Deg(latPt2));
      else
        return MapUtil.GetDistance(lonPt1, latPt1, lonPt2, latPt2);
    }

    public String ToDisplay()
    {
      return $"({minPoint.Longitude} , {minPoint.Latitude}) , ({maxPoint.Longitude} , {maxPoint.Latitude})";
    }

  }
}
