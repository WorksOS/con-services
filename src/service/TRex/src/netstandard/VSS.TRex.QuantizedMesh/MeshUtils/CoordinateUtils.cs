/*
ECEF is a right-handed Cartesian coordinate system with the origin at the Earth’s center, and that is fixed with respect
to the Earth (i.e., rotates along with the Earth). Units are in meters. The three axis are defined as follows:
X:	Passes through the equator at the Prime Meridian (latitude = 0, longitude = 0).
Y:	Passes through the equator 90 degrees east of the Prime Meridian (latitude = 0, longitude = 90 degrees).
Z:	Passes through the North Pole (latitude = 90 degrees, longitude = any value). 
http://danceswithcode.net/engineeringnotes/geodetic_to_ecef/geodetic_to_ecef.html
 */
using System;
using VSS.TRex.QuantizedMesh.Models;

namespace VSS.TRex.QuantizedMesh.MeshUtils
{
  public class CoordinateUtils
  {
    private static double a = 6378137.0;              //WGS-84 semi-major axis
    private static double e2 = 6.6943799901377997e-3;  //WGS-84 first eccentricity squared
    private static double a1 = 4.2697672707157535e+4;  //a1 = a*e2
    private static double a2 = 1.8230912546075455e+9;  //a2 = a1*a1
    private static double a3 = 1.4291722289812413e+2;  //a3 = a1*e2/2
    private static double a4 = 4.5577281365188637e+9;  //a4 = 2.5*a2
    private static double a5 = 4.2840589930055659e+4;  //a5 = a1+a3
    private static double a6 = 9.9330562000986220e-1;  //a6 = 1-e2
    private static double zp, w2, w, r2, r, s2, c2, s, c, ss;
    private static double g, rg, rf, u, v, m, f, p, x, y, z;
    private static double n, lat, lon, alt;

    //Convert Earth-Centered-Earth-Fixed (ECEF) to lat, Lon, Altitude
    //Input is a three element array containing x, y, z in meters
    //Returned array contains lat and lon in radians, and altitude in meters
    public static Vector3 ecef_to_geo(Vector3 ecef)
    {
      double[] geo = new double[3];   //Results go here (Lat, Lon, Altitude)
      x = ecef.X;
      y = ecef.Y;
      z = ecef.Z;
      zp = Math.Abs(z);
      w2 = x * x + y * y;
      w = Math.Sqrt(w2);
      r2 = w2 + z * z;
      r = Math.Sqrt(r2);
      geo[1] = Math.Atan2(y, x);       //Lon (final)
      s2 = z * z / r2;
      c2 = w2 / r2;
      u = a2 / r;
      v = a3 - a4 / r;
      if (c2 > 0.3)
      {
        s = (zp / r) * (1.0 + c2 * (a1 + u + s2 * v) / r);
        geo[0] = Math.Asin(s);      //Lat
        ss = s * s;
        c = Math.Sqrt(1.0 - ss);
      }
      else
      {
        c = (w / r) * (1.0 - s2 * (a5 - u - c2 * v) / r);
        geo[0] = Math.Acos(c);      //Lat
        ss = 1.0 - c * c;
        s = Math.Sqrt(ss);
      }
      g = 1.0 - e2 * ss;
      rg = a / Math.Sqrt(g);
      rf = a6 * rg;
      u = w - rg * c;
      v = zp - rf * s;
      f = c * u + s * v;
      m = c * v - s * u;
      p = m / (rf / g + f);
      geo[0] = geo[0] + p;      //Lat
      geo[2] = f + m * p / 2.0;     //Altitude
      if (z < 0.0)
      {
        geo[0] *= -1.0;     //Lat
      }
      //      return new Vector3(geo[0], geo[1], geo[2]);    //Return Lat, Lon, Altitude in that order
      return new Vector3(geo[1], geo[0], geo[2]);    //Return Lat, Lon, Altitude in that order
    }

    //Convert Lat, Lon, Altitude to Earth-Centered-Earth-Fixed (ECEF)
    //Input is a Vector3 containing lon, lat (rads) and alt (m)
    //Returned Vector3 contains x, y, z in meters
    public static Vector3 geo_to_ecef(Vector3 geo)
    {
      //      double[] ecef = new double[3];  //Results go here (x, y, z)
      Vector3 ecef = new Vector3(0, 0, 0);  //Results go here (x, y, z)

      lat = geo.Y;
      lon = geo.X;
      alt = geo.Z;

      n = a / Math.Sqrt(1 - e2 * Math.Sin(lat) * Math.Sin(lat));

      ecef.X = (n + alt) * Math.Cos(lat) * Math.Cos(lon);    //ECEF x
      ecef.Y = (n + alt) * Math.Cos(lat) * Math.Sin(lon);    //ECEF y
      ecef.Z = (n * (1 - e2) + alt) * Math.Sin(lat);          //ECEF z
                                                              //      ecef[0] = (n + alt) * Math.Cos(lat) * Math.Cos(lon);    //ECEF x
                                                              //      ecef[1] = (n + alt) * Math.Cos(lat) * Math.Sin(lon);    //ECEF y
                                                              //     ecef[2] = (n * (1 - e2) + alt) * Math.Sin(lat);          //ECEF z

      return ecef;     //Return x, y, z in ECEF
    }
  }
}
