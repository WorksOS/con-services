using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace VSS.Hosted.VLCommon
{
  public class Polygon
  {

    public double? MinLat = null;
    public double? MaxLat = null;
    public double? MinLon = null;
    public double? MaxLon = null;

    static String pointFormat = "<Point><x>{0}</x><y>{1}</y></Point>";

    // Private members
    private List<Point> points = null;

    public Polygon()
    {
    }

    public Polygon(string polygonXml)
    {
      Xml = polygonXml;
    }

    // Public accessors
    public string Xml
    {
      get
      {
        StringBuilder xmlBuilder = new StringBuilder("<Polygon>", 35 + 75 * points.Count);

        foreach (Point polyPt in points)
        {
          xmlBuilder.AppendFormat(pointFormat, polyPt.x, polyPt.y);
        }
        xmlBuilder.Append("</Polygon>");
        return xmlBuilder.ToString();
      }
      set
      {
        XmlToPoints(value);
        CalculateBBox();
      }
    }

    private void CalculateBBox()
    {
      if (points == null || points.Count == 0)
      {
        MinLat = null;
        MaxLat = null;
        MinLon = null;
        MaxLon = null;
        return;
      }

      MaxLat = points[0].y;
      MinLat = MaxLat;
      MaxLon = points[0].x;
      MinLon = MaxLon;
      for (int i = 1; i < points.Count; i++)
      {
        double lon = points[i].x;
        double lat = points[i].y;
        if (lon > MaxLon)
        {
          MaxLon = lon;
        }
        else if (lon < MinLon)
        {
          MinLon = lon;
        }

        if (lat > MaxLat)
        {
          MaxLat = lat;
        }
        else if (lat < MinLat)
        {
          MinLat = lat;
        }
      }
    }

    public IEnumerable<Point> PolygonPoints
    {
      get
      {
        return points;
      }
    }

    private void XmlToPoints(string polygonXml)
    {
      if (!string.IsNullOrEmpty(polygonXml))
      {
        XElement doc = XElement.Load(new StringReader(polygonXml));
        points = (from xml in doc.Elements("Point")
                  select new Point()
                  {
                    x = double.Parse(xml.Element("x").Value),
                    y = double.Parse(xml.Element("y").Value)
                  }).ToList<Point>();
      }
    }

    // Point management.  Add, remove, clear...
    public void AddPoint(double latitude, double longitude)
    {
      if (points == null)
      {
        points = new List<Point>();
      }
      points.Add(new Point(latitude, longitude));
      CalculateBBox();
    }

    public void RemovePoint(int atIndex)
    {
      if (atIndex >= 0 && atIndex < points.Count)
      {
        points.RemoveAt(atIndex);
      }
      CalculateBBox();
    }

    public void InsertPoint(double latitude, double longitude, int atIndex)
    {
      points.Insert(atIndex, new Point(latitude, longitude));
      CalculateBBox();
    }

    public void ClearPoints()
    {
      points.Clear();
      // Unset bounding box
      MinLat = null;
      MinLon = null;
      MaxLat = null;
      MaxLon = null;
    }

    // Useful polygon functions. Area, isInside...
    public bool Inside(double latitude, double longitude)
    {
      //Legacy TCM sites don't have polygon points.  Use bounding box instead.
      if (points.Count < 3)
      {
        return MinLat <= latitude
             && latitude <= MaxLat
             && MinLon <= longitude
             && longitude <= MaxLon;
      }

      // Bounding box check.  We can't ever ensure we are inside here, but we can be sure if we are outside.
      if (longitude < MinLon || longitude > MaxLon || latitude < MinLat || latitude > MaxLat)
      {
        return false;
      }

      // from http://alienryderflex.com/polygon/
      bool oddNodes = false;

      int j = points.Count - 1;
      for (int i = 0; i < points.Count; i++)
      {
        // Get the points only once.  Even though the accessor is fast, multiple gets are a measured culprit of poor performance
        double pointILatitue = points[i].Latitude;
        double pointJLatitue = points[j].Latitude;
        if (pointILatitue < latitude && pointJLatitue >= latitude
         || pointJLatitue < latitude && pointILatitue >= latitude)
        {
          double pointILongitue = points[i].Longitude;
          double pointJLongitue = points[j].Longitude;
          if (pointILongitue + (latitude - pointILatitue) /
             (pointJLatitue - pointILatitue) * (pointJLongitue - pointILongitue) < longitude)
          {
            oddNodes = !oddNodes;
          }
        }
        j = i;
      }

      return oddNodes;
    }

    public bool PolygonAproximatesRectangle
    {
      get
      {
        //Legacy TCM sites don't have polygon points.
        //Also if less than three points then can't calculate polygon area
        if (points.Count < 3)
          return true;

        //Otherwise polygon is approximated by bounding rectangle 
        //if the difference in areas is less than 10%
        double rectArea = RectangleArea;
        return Math.Abs(rectArea - Area) / rectArea < 0.1;
      }
    }

    private double RectangleArea
    {
      get
      {
        if (MinLat == null || MinLon == null || MaxLat == null || MaxLon == null)
          return 0.0;

        return (MaxLon.Value - MinLon.Value) * (MaxLat.Value - MinLat.Value);
      }
    }

    private double Area
    {
      get
      {
        //For legacy TCM sites, there are no polygon points.
        //Also can't calculate area if less than 3 points.
        if (points.Count < 3)
          return 0.0;

        double area = 0.0;
        int j = 0;

        for (int i = 0; i < points.Count; i++)
        {
          j = j++ % points.Count;
          area += (points[i].x + points[j].x) * (points[i].y - points[j].y);
        }

        return area * 0.5;
      }
    }
  }
}
