using System;
using System.Collections.Generic;
using System.Linq;
using TestUtility;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Repositories.DBModels;

namespace WebApiTests
{
  public class WebTestBase
  {
    protected readonly Msg Msg = new Msg();
    protected static readonly Guid ProjectUid = new Guid("7925f179-013d-4aaf-aff4-7b9833bb06d6");
    protected readonly Guid ProjectUid2 = new Guid("86a42bbf-9d0e-4079-850f-835496d715c5");
    protected static readonly Guid CustomerUid = new Guid("48003241-851d-4145-8c2a-7b099bbfd117");
    protected readonly string UserId = new Guid("98cdb619-b06b-4084-b7c5-5dcccc82af3b").ToString();
    //Favorite geofences
    protected static readonly Guid SouthernMotorWayFavoriteGeofenceUid = Guid.Parse("ffdabc61-7ee9-4054-a3e1-f182dd1abec9");
    protected static readonly Guid WalnutCreekFavoriteGeofenceUid = Guid.Parse("09097669-34e7-4b34-b921-680018388505");
    protected static readonly Guid ZieglerBloomingtonFavoriteGeofenceUid = Guid.Parse("69de1f67-1b2a-413a-8936-659892379fd9");
    protected static readonly Guid GoldenDimensionFavoriteGeofenceUid = Guid.Parse("ac633c6c-f941-4fe0-a1f6-b5964f06b076");

    //Copied from MockWebApi
    protected static string SouthernMotorWayFavoriteGeometryWKT =>
      "POLYGON((172.525733009204 -43.5613699555099,172.527964607104 -43.5572026751871,172.539980903491 -43.5602504159773,172.553370490893 -43.5555232419366,172.571652427539 -43.5466276854031,172.566760078295 -43.542086090904,172.571652427539 -43.5402195830085,172.583067909106 -43.5438281128051,172.594998374804 -43.5441391828477,172.621777549609 -43.5459433574412,172.621949210986 -43.5494271279847,172.611220374926 -43.5504846613456,172.597916618212 -43.548929458806,172.588217750415 -43.5476852678816,172.585556999072 -43.5501114163959,172.568133369311 -43.5580112745029,172.563412681445 -43.5617431307312,172.552254691943 -43.5703255228523,172.544444099292 -43.5696414639818,172.53328610979 -43.567091721564,172.525733009204 -43.5613699555099))";
    protected static string SouthernMotorWayFavoriteName = "Southern Motorway";
    protected static GeofenceType SouthernMotorWayFavoriteType = GeofenceType.Generic;

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
