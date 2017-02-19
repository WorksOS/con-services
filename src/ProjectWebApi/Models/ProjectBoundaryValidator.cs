using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ProjectWebApi.Models
{
  public class ProjectBoundaryValidator
  {
    private static List<Point> ParseBoundaryData(string s)
    {
      var points = new List<Point>();

      string[] pointsArray = s.Remove(s.Length - 1).Split(';');

      for (int i = 0; i < pointsArray.Length; i++)
      {
        double[] coordinates = new double[2];

        //gets x and y coordinates split by comma, trims whitespace at pos 0, converts to double array
        coordinates = pointsArray[i].Trim().Split(',').Select(c => double.Parse(c)).ToArray();

        points.Add(new Point(coordinates[1], coordinates[0]));
      }
      return points;
    }

    public static void Validate(string boundary)
    {
      try
      {
        var points = ParseBoundaryData(boundary);

        if (points.Count < 3)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              "Invalid project's boundary as it should contain at least 3 points");
        }
      }
      catch
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            "Invalid project's boundary");
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
