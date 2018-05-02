using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;

namespace VSS.MasterData.Project.WebAPI.Common.Utilities
{
  public class ProjectBoundaryValidator
  {
    public const string POLYGON_WKT = "POLYGON((";
    private static List<string> _replacements = new List<string> {"POLYGON", "(", ")"};
    protected static ContractExecutionStatesEnum contractExecutionStatesEnum = new ContractExecutionStatesEnum();

    private static IEnumerable<Point> ParseBoundaryData(string s, char pointSeparator, char coordSeparator)
    {
      string[] pointsArray = s. /*Remove(s.Length - 1).*/Split(pointSeparator);

      foreach (var t in pointsArray)
      {
        //gets x and y coordinates split by comma, trims whitespace at pos 0, converts to double array
        var coordinates = t.Trim().Split(coordSeparator).Select(double.Parse).ToArray();
        yield return (new Point(coordinates[1], coordinates[0]));
      }
    }

    private static IEnumerable<Point> ParseBoundaryDataPoint(string s, char pointSeparator, char coordSeparator)
    {
      string[] pointsArray = s.Split(pointSeparator);

      foreach (var t in pointsArray)
      {
        //gets x and y coordinates split by comma, trims whitespace at pos 0, converts to double array
        var coordinates = t.Trim().Split(coordSeparator).Select(double.Parse).ToArray();
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
                logger.Info("Removed the Last Point in  GeometryWKT as it was duplicated");
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
                  logger.Info("Removed the All the Consecutive Point in  GeometryWKT");
                  return adjustedWktText;
                }
              }
            }
          }
          logger.Info("Not a valid GeometryWKT");
          return null;
        }
        catch
        {
          logger.Info("Not a valid GeometryWKT");
          return null;
        }
            */

      //For now, just validate the number of points and the format
      ValidatePoints(wkt, false);

      return wkt;
    }

    public static IEnumerable<Point> ParseGeometryData(string s)
    {
      foreach (string to_replace in _replacements)
      {
        s = s.Replace(to_replace, string.Empty);
      }

      return ParseBoundaryData(s, ',', ' ');
    }

    public static IEnumerable<Point> ParseGeometryDataPointLL(string s)
    {
      foreach (string to_replace in _replacements)
      {
        s = s.Replace(to_replace, string.Empty);
      }

      return ParseBoundaryDataPoint(s, ',', ' ');
    }

    public static string GetWicketFromPoints(List<Point> points)
    {
      if (points.Count == 0)
        return string.Empty;

      var polygonWkt = new StringBuilder("POLYGON((");
      foreach (var point in points)
      {
        polygonWkt.Append(String.Format("{0} {1},", point.x, point.y));
      }

      return polygonWkt.ToString().TrimEnd(',') + ("))");
    }

    public static List<Point> MakingValidPoints(List<Point> points)
    {
      List<Point> adjustedPoints = new List<Point>();
      if (!points[0].Equals(points[points.Count - 1]))
        points.Add(points[0]);
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

    public static double CalculateAreaSqMeters(string projectBoundaryWKT)
    {
      List<Point> points = ParseGeometryData(projectBoundaryWKT).ToList();

      //from http://stackoverflow.com/questions/1340223/calculating-area-enclosed-by-arbitrary-polygon-on-earths-surface
      //This is like MATLAB areaint function.
      //Calculate the surface per unit area using lat/lng then multiply by Earth's surface area
      const double EARTH_SURFACE_AREA = 5.10072E14; //sq m
      const double DEGREES_TO_RADIANS = Math.PI / 180;
      double sum = 0.0;
      double prevcolat = 0.0;
      double prevaz = 0.0;
      double colat0 = 0.0;
      double az0 = 0.0;
      for (int i = 0; i < points.Count; i++)
      {
        double latRad = points[i].Latitude * DEGREES_TO_RADIANS;
        double lngRad = points[i].Longitude * DEGREES_TO_RADIANS;
        double sinLatDiv2 = Math.Sin(latRad / 2);
        double sinLngDiv2 = Math.Sin(lngRad / 2);
        double cosLat = Math.Cos(latRad);
        double powLng = Math.Pow(sinLngDiv2, 2);
        double powLat = Math.Pow(sinLatDiv2, 2);
        double colat = 2 * Math.Atan2(Math.Sqrt(powLat + cosLat * powLng), Math.Sqrt(1 - powLat - cosLat * powLng));
        double az = 0.0;
        if (points[i].Latitude >= 90)
        {
          az = 0;
        }
        else if (points[i].Latitude <= -90)
        {
          az = Math.PI;
        }
        else
        {
          az = Math.Atan2(cosLat * Math.Sin(lngRad), Math.Sin(latRad)) % (2 * Math.PI);
        }

        if (i == 0)
        {
          colat0 = colat;
          az0 = az;
        }

        if (i > 0 && i < points.Count)
        {
          double deltaaz = az - prevaz;
          double signum = deltaaz > 0 ? 1 : (deltaaz < 0 ? -1 : 0);
          deltaaz = Math.Abs(deltaaz);
          sum = sum + (1 - Math.Cos(prevcolat + (colat - prevcolat) / 2)) * Math.PI *
                ((deltaaz / Math.PI) - 2 * Math.Ceiling(((deltaaz / Math.PI) - 1) / 2)) * signum;
        }

        prevcolat = colat;
        prevaz = az;
      }

      sum = sum + (1 - Math.Cos(prevcolat + (colat0 - prevcolat) / 2)) * (az0 - prevaz);
      double tempSum = Math.Abs(sum) / 4 / Math.PI;
      double areaSquareMeters = EARTH_SURFACE_AREA * Math.Min(tempSum, 1 - tempSum);
      return areaSquareMeters;
    }
  }
}
