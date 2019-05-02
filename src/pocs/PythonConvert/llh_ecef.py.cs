
using @division = @@__future__.division;

using old_div = past.utils.old_div;

using math;

using System;

using System.Collections.Generic;

public static class llh_ecef {
    
    public static double radiusX = 6378137.0;
    
    public static double radiusY = 6378137.0;
    
    public static double radiusZ = 6356752.31424518;
    
    public static double wgs84_a = radiusX;
    
    public static double wgs84_b = radiusZ;
    
    public static double wgs84_e2 = 0.00669437999019758;
    
    public static double wgs84_a2 = Math.Pow(wgs84_a, 2);
    
    public static double wgs84_b2 = Math.Pow(wgs84_b, 2);
    
    public static object LLH2ECEF(object lon, object lat, object alt) {
        lat *= old_div(math.pi, 180.0);
        lon *= old_div(math.pi, 180.0);
        Func<object, object> n = x => {
            return old_div(wgs84_a, math.sqrt(1 - wgs84_e2 * Math.Pow(math.sin(x), 2)));
        };
        var x = (n(lat) + alt) * math.cos(lat) * math.cos(lon);
        var y = (n(lat) + alt) * math.cos(lat) * math.sin(lon);
        var z = (n(lat) * (1 - wgs84_e2) + alt) * math.sin(lat);
        return new List<double> {
            x,
            y,
            z
        };
    }
    
    // alt is in meters
    public static object ECEF2LLH(object x, object y, object z) {
        var ep = math.sqrt(old_div(wgs84_a2 - wgs84_b2, wgs84_b2));
        var p = math.sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        var th = math.atan2(wgs84_a * z, wgs84_b * p);
        var lon = math.atan2(y, x);
        var lat = math.atan2(z + Math.Pow(ep, 2) * wgs84_b * Math.Pow(math.sin(th), 3), p - wgs84_e2 * wgs84_a * Math.Pow(math.cos(th), 3));
        var N = old_div(wgs84_a, math.sqrt(1 - wgs84_e2 * Math.Pow(math.sin(lat), 2)));
        var alt = old_div(p, math.cos(lat)) - N;
        lon *= old_div(180.0, math.pi);
        lat *= old_div(180.0, math.pi);
        return new List<object> {
            lon,
            lat,
            alt
        };
    }
}
