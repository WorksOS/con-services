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
    //Associated geofences
    protected static readonly Guid GoldenDimensionAssociatedGeofenceUid = GoldenDimensionFavoriteGeofenceUid;
    protected static readonly Guid EOP13AssociatedGeofenceUid = Guid.Parse("87EBB1F2-0B52-445B-A819-DE58062B1ABC");

    //Copied from MockWebApi
    protected static string SouthernMotorWayFavoriteGeometryWKT =>
      "POLYGON((172.525733009204 -43.5613699555099,172.527964607104 -43.5572026751871,172.539980903491 -43.5602504159773,172.553370490893 -43.5555232419366,172.571652427539 -43.5466276854031,172.566760078295 -43.542086090904,172.571652427539 -43.5402195830085,172.583067909106 -43.5438281128051,172.594998374804 -43.5441391828477,172.621777549609 -43.5459433574412,172.621949210986 -43.5494271279847,172.611220374926 -43.5504846613456,172.597916618212 -43.548929458806,172.588217750415 -43.5476852678816,172.585556999072 -43.5501114163959,172.568133369311 -43.5580112745029,172.563412681445 -43.5617431307312,172.552254691943 -43.5703255228523,172.544444099292 -43.5696414639818,172.53328610979 -43.567091721564,172.525733009204 -43.5613699555099))";
    protected static string SouthernMotorWayFavoriteName = "Southern Motorway";
    protected static GeofenceType SouthernMotorWayFavoriteType = GeofenceType.Generic;
    protected static string EOP13AssociatedGeometryWKT =>
      "POLYGON((-115.02549172449778 36.20766663477707,-115.0255212343655 36.20767269625067,-115.02555873203417 36.20768335365172,-115.02559606127022 36.2076980333531,-115.0256291264681 36.207714627570766,-115.02566989097252 36.20772827857688,-115.02571250862133 36.207737886103445,-115.02575775919485 36.20774018096381,-115.0258013094834 36.20774574238747,-115.02584463451544 36.207751984080375,-115.02589034940131 36.207756907603454,-115.02593542158498 36.207761857802325,-115.02598200296123 36.207765279298485,-115.02602798705124 36.207770516552785,-115.02607403096836 36.20777575341158,-115.02611911591057 36.207779923389396,-115.026162357111 36.207785682222095,-115.02620413076232 36.20778933562491,-115.02622950525301 36.20779401722804,-115.0262305157184 36.20779355252638,-115.02622884218971 36.207761977481525,-115.02623186717689 36.20773025134997,-115.02623492990716 36.20772163571351,-115.0262008661269 36.20772197716637,-115.02616375617589 36.20771852675422,-115.02612331489051 36.20771601963081,-115.02608239623123 36.20771294303469,-115.02604093491162 36.20770805941656,-115.02600360972545 36.207705497146335,-115.02596681528081 36.207701471201105,-115.02592935743189 36.20769802234807,-115.02589293874767 36.207694291053414,-115.02586129511938 36.207691454745486,-115.02583815856379 36.2076886369952,-115.02581346283809 36.20768022523639,-115.02578280262647 36.20766731993827,-115.02575026091887 36.20765336305246,-115.02571722266835 36.207637677106575,-115.02567833578642 36.20762418856364,-115.02564280185918 36.20760741179461,-115.02560545779126 36.20759216512792,-115.02557004255425 36.20757721804324,-115.02554855180823 36.20756619463444,-115.02554859432566 36.207551274194316,-115.02555265389948 36.20752083920205,-115.02553875965361 36.20751595510478,-115.0255119644468 36.20751519518673,-115.02541556477513 36.2075082588445,-115.02541752809267 36.207520620364164,-115.02541290607995 36.2075504653226,-115.0254062664494 36.20757953135285,-115.02540436373236 36.20760926533825,-115.02540290356575 36.20763924414362,-115.02540208946557 36.20765539062184,-115.02549172449778 36.20766663477707))";
    protected static string EOP13AssociatedName = "EOP13";
    protected static GeofenceType EOP13AssociatedType = GeofenceType.Borrow;

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
