using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Json;
using VSS.Common.Abstractions.Http;

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

      //Project thumbnails
      if (pt1 == "-115.026711,36.204698" && pt2 == "-115.017269,36.210966")
        tileData = JsonResourceHelper.GetBaseMap("ProjectThumbnail");

      //******
      //TODO: style = default=MAP, satellite, terrain (report tiles)
      //***

      //Report Tiles
      if (pt1 == "-115.026282,36.204387" && pt2 == "-115.017698,36.211277")
        tileData = JsonResourceHelper.GetBaseMap("ReportTile1");
      if (pt1 == "-115.019733,36.206692" && pt2 == "-115.018360,36.207801")//1024x1024
        tileData = JsonResourceHelper.GetBaseMap("ReportTile2");

      if (pt1 == "-115.019611,36.206791" && pt2 == "-115.018482,36.207702")//421x420
        tileData = JsonResourceHelper.GetBaseMap("ReportTile5");


      if (pt1 == "-115.026711,36.204040" && pt2 == "-115.017269,36.211624") //439x438
      {
        if (string.Compare(imgOption, "BACKGROUND", StringComparison.OrdinalIgnoreCase) == 0)
          tileData = JsonResourceHelper.GetBaseMap("SatelliteReportTile");
        else
          tileData = JsonResourceHelper.GetBaseMap("ReportTile4");
      }

      if (pt1 == "-115.027483,36.203400" && pt2 == "-115.016497,36.212264")//1024x1024
        tileData = JsonResourceHelper.GetBaseMap("LargeReportTile");

      if (pt1 == "-115.020964,36.206178" && pt2 == "-115.018218,36.208394")//1024x1024
        tileData = JsonResourceHelper.GetBaseMap("CutFillVolumeReportTile");

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
      return "[{\"Region\":4}]";
    }
  }
}
