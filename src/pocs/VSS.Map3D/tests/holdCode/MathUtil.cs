using System;
using System.Collections.Generic;
using System.Text;
/*
            if (args.Length != 4) {
                Console.WriteLine("Usage: example.exe <latitude1> <longitude1> <latitude2> <longitude2>");
                return;
            }

            double lat1 = Double.Parse(args[0]);
            double lon1 = Double.Parse(args[1]);
            double lat2 = Double.Parse(args[2]);
            double lon2 = Double.Parse(args[3]);

            Console.WriteLine("Distance: " + haversine(lat1, lon1, lat2, lon2) + " km");
            Console.WriteLine("Initial bearing: " + bearingTo(lat1, lon1, lat2, lon2) + "°");
            Console.WriteLine("Final bearing: " + bearingFrom(lat2, lon2, lat1, lon1) + "°"); 
 */
namespace VSS.Map3D.Quantize
{
  class MathUtil
  {
    private const double RADIUS = 6371;

    private static double toRad(double angle)
    {
      return angle * Math.PI / 180;
    }

    private static double toDeg(double angle)
    {
      return angle * 180 / Math.PI;
    }

    private static double bearingTo(double lat1, double lon1, double lat2, double lon2)
    {
      double dLon = toRad(lon2 - lon1);

      lat1 = toRad(lat1);
      lat2 = toRad(lat2);

      double y = Math.Sin(dLon) * Math.Cos(lat2);
      double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);

      double bearing = toDeg(Math.Atan2(y, x));

      if (bearing < 0)
      {
        bearing += 360;
      }

      return bearing;
    }

    private static double bearingFrom(double lat1, double lon1, double lat2, double lon2)
    {
      double bearing = bearingTo(lat1, lon1, lat2, lon2);

      bearing += 180;
      if (bearing > 360)
      {
        bearing -= 360;
      }

      return bearing;
    }

    private static double haversine(double lat1, double lon1, double lat2, double lon2)
    {
      double dLat = toRad(lat2 - lat1);
      double dLon = toRad(lon2 - lon1);

      double a, c, distance;

      lat1 = toRad(lat1);
      lat2 = toRad(lat2);

      a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
      c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
      distance = RADIUS * c;

      return distance;
    }
  }
}
