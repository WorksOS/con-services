using System;
using System.Collections.Generic;
using System.Linq;
using TestUtility;
using VSS.MasterData.Models.Models;

namespace WebApiTests
{
  public class WebTestBase
  {
    protected readonly Msg Msg = new Msg();
    protected static readonly Guid ProjectUid = new Guid("7925f179-013d-4aaf-aff4-7b9833bb06d6");
    protected readonly Guid ProjectUid2 = new Guid("86a42bbf-9d0e-4079-850f-835496d715c5");
    protected static readonly Guid CustomerUid = new Guid("48003241-851d-4145-8c2a-7b099bbfd117");
    protected readonly string UserId = new Guid("98cdb619-b06b-4084-b7c5-5dcccc82af3b").ToString();

    protected static string GenerateWKTPolygon()
    {
      var x = new Random().Next(-200, -100);
      var y = new Random().Next(100);

      return $"POLYGON(({x}.347189366818 {y}.8361907402694,{x}.349260032177 {y}.8361656688414,{x}.349217116833 {y}.8387897637231,{x}.347275197506 {y}.8387145521594,{x}.347189366818 {y}.8361907402694))";
    }

    protected static List<WGSPoint> GetPointsFromWkt(string wkt)
    {
      List<WGSPoint> ret = new List<WGSPoint>();
      try
      {
        wkt = wkt.ToUpper();
        string prefix = "POLYGON";

        if (wkt.StartsWith(prefix) && (prefix.Length < wkt.Length))
        {
          //extract x from POLYGON (x) or POLYGON(lon1 lat1, lon2 lat2)
          string relevant = wkt.Substring(prefix.Length, wkt.Length - prefix.Length).Trim(' ').Trim('(', ')');
          //relevant will be lon1 lat1,lon2 lat2 ...
          var pairs = relevant.Split(',').Select(x => x.Trim()).ToList();
          var first = pairs.First();
          var last = pairs.Last();
          if (!first.Equals(last))
            return new List<WGSPoint>();

          foreach (var pair in pairs.Take(pairs.Count - 1))
          {
            string[] vals = pair.Split(' ');
            if (vals.Length == 2)
            {
              string longitude = vals[0].Substring(vals[0].StartsWith("(") ? 1 : 0); //remove prefix ( if present
              string latitude = vals[1].Substring(0, vals[1].Length - (vals[1].EndsWith(")") ? 1 : 0)); // remove suffix ) if present
              var pt = new WGSPoint(Convert.ToDouble(latitude), Convert.ToDouble(longitude));
              ret.Add(pt);
            }
          }
        }
      }
      catch (Exception)
      {
        return ret;
      }
      return ret;
    }
  }
}