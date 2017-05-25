using ProjectWebApiCommon.ResultsHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ProjectWebApiCommon.Models
{
  public class ProjectBoundaryValidator
  {
    public const string POLYGON_WKT = "POLYGON((";
    private static List<string> _replacements = new List<string> {"POLYGON", "(", ")"};
    protected static ContractExecutionStatesEnum contractExecutionStatesEnum = new ContractExecutionStatesEnum();

    private static IEnumerable<Point> ParseBoundaryData(string s, char pointSeparator, char coordSeparator)
    {

      string[] pointsArray = s. /*Remove(s.Length - 1).*/Split(pointSeparator);

      for (int i = 0; i < pointsArray.Length; i++)
      {
        //gets x and y coordinates split by comma, trims whitespace at pos 0, converts to double array
        var coordinates = pointsArray[i].Trim().Split(coordSeparator).Select(c => double.Parse(c)).ToArray();
        yield return (new Point(coordinates[1], coordinates[0]));
      }
    }

    public static void ValidateV1(string boundary)
    {
      ValidatePoints(boundary, true);
    }

    public static string ValidateWKT(string wkt)
    {
      //Comment out until System.Data.Spatial available in .netcore (Microsoft.EntityFrameworkCore)

      /*
    try
    {
      if (wkt != null)
      {
        var dbGeometry = DbGeometry.FromText(wkt);
        if (dbGeometry.IsValid)
          return wkt;
        var points = ParseGeometryData(wkt);
            if (points.Count > 1 && points[points.Count - 1].Equals(points[points.Count - 2]))
            {
              points.RemoveAt(points.Count - 1);
              var wktText = GetWicketFromPoints(points);
              var fixedGeometry = DbGeometry.FromText(wktText);
              if (fixedGeometry.IsValid)
              {
                Log.Info("Removed the Last Point in  GeometryWKT as it was duplicated");
                return wktText;
              }
              else
              {
                //Trying One Last Time
                // Removing all consecutive duplicate points
                List<Point> adjustedPoints = MakingValidPoints(points);
                var adjustedWktText = GetWicketFromPoints(adjustedPoints);
                var adjustedFixedGeometry = DbGeometry.FromText(adjustedWktText);
                if (adjustedFixedGeometry.IsValid)
                {
                  Log.Info("Removed the All the Consecutive Point in  GeometryWKT");
                  return adjustedWktText;
                }
              }
            }
          }
          Log.Info("Not a valid GeometryWKT");
          return null;
        }
        catch
        {
          Log.Info("Not a valid GeometryWKT");
          return null;
        }
            */

      //For now, just validate the number of points and the format
      ValidatePoints(wkt, false);

      return wkt;
    }

    private static IEnumerable<Point> ParseGeometryData(string s)
    {
      foreach (string to_replace in _replacements)
      {
        s = s.Replace(to_replace, string.Empty);
      }
      return ParseBoundaryData(s, ',', ' ');
    }

    private static string GetWicketFromPoints(List<Point> points)
    {
      if (points.Count == 0)
        return "";

      var polygonWkt = new StringBuilder("POLYGON((");
      foreach (var point in points)
      {
        polygonWkt.Append(String.Format("{0} {1},", point.x, point.y));
      }
      return polygonWkt.ToString().TrimEnd(',') + ("))");
    }

    private static List<Point> MakingValidPoints(List<Point> points)
    {
      List<Point> adjustedPoints = new List<Point>();
      points.Add(new Point(Double.MaxValue, Double.MaxValue));
      for (int i = 0; i < points.Count - 1; i++)
      {
        var firstPoint = points[i];
        var secondPoint = points[i + 1];
        if (!firstPoint.Equals(secondPoint))
        {
          adjustedPoints.Add(firstPoint);
        }
      }
      return adjustedPoints;
    }

    private static void ValidatePoints(string boundary, bool oldFormat)
    {
      if (string.IsNullOrEmpty(boundary))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(23),
            contractExecutionStatesEnum.FirstNameWithOffset(23)));
      }
      try
      {
        var points = (oldFormat ? ParseBoundaryData(boundary, ';', ',') : ParseGeometryData(boundary)).ToList();

        if (points.Count < 3)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(24),
              contractExecutionStatesEnum.FirstNameWithOffset(24)));
        }
      }
      catch
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(contractExecutionStatesEnum.GetErrorNumberwithOffset(25),
            contractExecutionStatesEnum.FirstNameWithOffset(25)));
      }
    }

  }

  public class Point : IEquatable<Point>
  {
    public double x;
    public double y;

    public Point(double latitude, double longitude)
    {
      this.x = longitude;
      this.y = latitude;
    }

    public double Latitude
    {
      get { return y; }
    }

    public double Longitude
    {
      get { return x; }
    }

    public bool Equals(Point other)
    {
      return other != null ? (this.Latitude == other.Latitude && this.Longitude == other.Longitude) : false;
    }
  }
}
