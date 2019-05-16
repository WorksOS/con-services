using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Json;
using VSS.Common.Abstractions.Http;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockAlkController : Controller
  {
    [Route("map")]
    [HttpGet]
    public FileResult GetMap(
      [FromQuery] string AuthToken,
      [FromQuery] string pt1,
      [FromQuery] string pt2,
      [FromQuery] int width,
      [FromQuery] int height,
      [FromQuery] string drawergroups,
      [FromQuery] string style,
      [FromQuery] string srs,
      [FromQuery] string region,
      [FromQuery] string dataset,
      [FromQuery] string language,
      [FromQuery] string imgSrc,
      [FromQuery] string imgOption)
    {
      //"http://pcmiler.alk.com/APIs/REST/v1.0/Service.svc/map?AuthToken=97CC5BD1CA28934796791B229AE9C3FA&pt1=-115.026711,36.204698&pt2=-115.017269,36.210966&width=439&height=362&drawergroups=Cities,Labels,Roads,Commercial,Borders,Areas&style=default&srs=EPSG:900913&region=NA&dataset=PCM_NA&language=en&imgSrc=Sat1"
      Console.WriteLine($"{nameof(GetMap)}: {Request.QueryString}");

      byte[] tileData = null;

      var resourceName = $"{MapTypePrefix(style, imgOption)}{width}x{height}";

      //Project thumbnails (Map)
      //Geofence - Dimensions (Map)
      if (pt1 == "-115.026711,36.204698" && pt2 == "-115.017269,36.210966" && width == 439 && height == 362)
        tileData = JsonResourceHelper.GetBaseMap("ProjectThumbnail");

      //Report Tiles
      //Note: No ALK base map is required for the following Report Tile acceptance tests
      //#1, #2, #6, #7, #8, #9, #10, #11, #12, #13
      //Cutfill and Volumes Explicit #1, #2
      //Cuutfill and Volumes #1, #5, #6, #7

      if (pt1 == "-115.027483,36.203400" && pt2 == "-115.016497,36.212264" && style == "satellite" && imgOption == "BACKGROUND")//1024x1024
        tileData = JsonResourceHelper.GetBaseMap("LargeReportTile");

      //#14, #15, #16, #17, #18, #19, #20, #21, #22, #23 satellite BACKGROUND (Satellite)
      //#24, #25, #26, #27, #28, #29, #30, #31 satellite (Hybrid)
      else if (pt1 == "-115.026711,36.204040" && pt2 == "-115.017269,36.211624" && width == 439 && height == 438)
        tileData = JsonResourceHelper.GetBaseMap(resourceName);

      //#3, #4, #5 (Map)
      else if (pt1 == "-115.026282,36.204387" && pt2 == "-115.017698,36.211277" && width == 399 && height == 398)
        tileData = JsonResourceHelper.GetBaseMap(resourceName);

      //#32, #33, #34 satellite (Hybrid)
      else if (pt1 == "-115.019611,36.206791" && pt2 == "-115.018482,36.207702" && width == 421 && height == 420)
        tileData = JsonResourceHelper.GetBaseMap(resourceName);

      //Cutfill and volumes #2 (Map)
      //Cutfill and volumes #3, #4 (Hybrid)
      else if (pt1 == "-115.020964,36.206178" && pt2 == "-115.018218,36.208394" && width == 1024 && height == 1024)
        tileData = JsonResourceHelper.GetBaseMap($"{resourceName}-cfv1");

      //Cutfill and volumes #8 (Hybrid)
      else if (pt1 == "-115.019686,36.206742" && pt2 == "-115.018312,36.207851" && width == 1024 && height == 1024)
        tileData = JsonResourceHelper.GetBaseMap($"{resourceName}-cfv2");

      //Cutfill and volumes #9 (Hybrid)
      else if (pt1 == "-115.019733,36.206692" && pt2 == "-115.018360,36.207801" && width == 1024 && height == 1024)
        tileData = JsonResourceHelper.GetBaseMap($"{resourceName}-cfv3");

      //Geofences
      //Point (Map)
      else if (pt1 == "96.946132,74.899225" && pt2 == "-83.053868,-53.885322" && width == 220 && height == 182)
        tileData = JsonResourceHelper.GetBaseMap("GeofencePoint");

      //Good polygons (Map)
      else if (pt1 == "144.960490,-37.816794" && pt2 == "144.964299,-37.814302" && width == 354 && height == 293)
        tileData = JsonResourceHelper.GetBaseMap("GeofenceGoodPolygon1");
      else if (pt1 == "144.964138,-37.817455" && pt2 == "144.970114,-37.813547" && width == 278 && height == 230)
        tileData = JsonResourceHelper.GetBaseMap("GeofenceGoodPolygon2");

      //Multiple (Map)
      else if (pt1 == "-115.020547,36.206736" && pt2 == "-115.019693,36.207309" && width == 318 && height == 264)
        tileData = JsonResourceHelper.GetBaseMap("GeofenceMultiple1");
      else if (pt1 == "-115.023150,36.206403" && pt2 == "-115.019850,36.208597" && width == 307 && height == 253)
        tileData = JsonResourceHelper.GetBaseMap("GeofenceMultiple2");

      return new FileStreamResult(new MemoryStream(tileData), ContentTypeConstants.ImagePng);
    }

    [Route("locations")]
    [HttpGet]
    public string GetRegion([FromQuery] string coords, [FromQuery] string AuthToken)
    {
      Console.WriteLine($"{nameof(GetRegion)}: {Request.QueryString}");
      /* 
     Example result
     [{"Address":{"StreetAddress":"Christchurch Southern Motorway","City":"Christchurch","State":"NZ","Zip":"8024","County":"Christchurch City","Country":"New Zealand","SPLC":null,"CountryPostalFilter":0,"AbbreviationFormat":0},"Coords":{"Lat":"-43.545639","Lon":"172.583091"},"Region":5,"Label":"","PlaceName":"","TimeZone":"+13:0","Errors":[]}]
     */
      //Only the region is used so return just the region
      var region = 4;
      if (coords == "6.94613249999986,10.5069511547539")
        region = 1;
      else if (coords == "144.962394833565,-37.8155476155167" || coords == "144.967126250267,-37.8155009840417")
        region = 5;
      return $"[{{\"Region\":{region}}}]";
    }

    private string MapTypePrefix(string style, string imgOption)
    {
      if (style == "terrain")
        return "Terrain";
      if (style == "satellite")
      {
        return imgOption == "BACKGROUND" ? "Satellite" : "Hybrid";
      }
      return "Map";
    }
  }
}
