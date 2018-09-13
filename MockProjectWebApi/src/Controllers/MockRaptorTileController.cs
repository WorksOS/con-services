using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.Productivity3D.Models.Enums;
using System.Security.Cryptography;

namespace MockProjectWebApi.Controllers
{
  public class MockRaptorTileController : Controller
  {
    [Route("api/v2/productiondatatiles/png")]
    [HttpGet]
    public async Task<FileResult> GetMockProductionDataTileRaw(
      [FromQuery] string service,
      [FromQuery] string version,
      [FromQuery] string request,
      [FromQuery] string format,
      [FromQuery] string transparent,
      [FromQuery] string layers,
      [FromQuery] string crs,
      [FromQuery] string styles,
      [FromQuery] ushort width,
      [FromQuery] ushort height,
      [FromQuery] string bbox,
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? cutFillDesignUid,
      [FromQuery] DisplayMode mode,
      [FromQuery] Guid? volumeBaseUid,
      [FromQuery] Guid? volumeTopUid,
      [FromQuery] VolumeCalcType? volumeCalcType)
    {
      Console.WriteLine($"GetMockProductionDataTileRaw: {Request.QueryString}");

      using (Image<Rgba32> bitmap = new Image<Rgba32>(width, height))
      {
        if (projectUid.ToString() == "ff91dd40-1569-4765-a2bc-014321f76ace")
        {
          //Just do a fixed block of color to represent production data
          Rgba32 color = Rgba32.Black;
          const int w = 100;
          const int h = 100;
          int x = (width - w) / 2;
          int y = (height - h) / 2;
          switch (mode)
          {
            case DisplayMode.Height:
              color = Rgba32.Red;
              break;
            case DisplayMode.CCV:
              color = Rgba32.Aqua;
              break;
            case DisplayMode.PassCount:
              color = Rgba32.Fuchsia;
              break;
            case DisplayMode.PassCountSummary:
              color = Rgba32.Green;
              break;
            case DisplayMode.CutFill:
              switch (volumeCalcType)
              {
                case VolumeCalcType.None:
                  color = Rgba32.Yellow;
                  break;
                case VolumeCalcType.DesignToGround:
                  color = Rgba32.Maroon;
                  break;
                case VolumeCalcType.GroundToDesign:
                  color = Rgba32.Teal;
                  break;
                case VolumeCalcType.GroundToGround:
                  color = Rgba32.Navy;
                  break;
              }
              break;
            case DisplayMode.TemperatureSummary:
              color = Rgba32.Purple;
              break;
            case DisplayMode.CCVPercentSummary:
              color = Rgba32.Blue;
              break;
            case DisplayMode.MDPPercentSummary:
              color = Rgba32.Lime;
              break;
            case DisplayMode.TargetSpeedSummary:
              color = Rgba32.Brown;
              break;
            case DisplayMode.CMVChange:
              color = Rgba32.Orange;
              break;
          }
          var rect = new RectangleF(x, y, w, h);
          bitmap.Mutate(ctx => ctx.Fill(color, rect));          
        }
        //else return Empty tile
 
        var bitmapStream = new MemoryStream();
        bitmap.SaveAsPng(bitmapStream);
        Console.WriteLine($"GetMockProductionDataTileRaw result: MD5={CreateMD5(bitmapStream)}");
        bitmapStream.Position = 0;
        return new FileStreamResult(bitmapStream, "image/png");
      }
    }

    private string CreateMD5(MemoryStream ms)
    {
      //Create MD5 hash
      using (MD5 md5 = MD5.Create())
      {
        ms.Position = 0;
        byte[] hashBytes = md5.ComputeHash(ms.ToArray());

        // Convert the hash byte array to hexadecimal string
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
          sb.Append(hashBytes[i].ToString("X2"));
        }
        return sb.ToString();
      }
    }

    [Route("api/v2/raptor/boundingbox")]
    [HttpGet]
    public async Task<string> GetMockBoundingBox(
      [FromQuery] Guid projectUid,
      [FromQuery] TileOverlayType[] overlays,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? cutFillDesignUid,
      [FromQuery] Guid? baseUid,
      [FromQuery] Guid? topUid,
      [FromQuery] VolumeCalcType? volumeCalcType)
    {
      Console.WriteLine($"GetMockBoundingBox: {Request.QueryString}");

      if (projectUid.ToString() == "ff91dd40-1569-4765-a2bc-014321f76ace")
      {
        if (filterUid.ToString() == "2811c7c3-d270-4d63-97e2-fc3340bf6c6b")
        {
          return "36.207113691064556349,-115.0195603225804319,36.207379376219485323,-115.018532588767485";
        }

        if (cutFillDesignUid.ToString() == "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff")
        {
          return "36.207068000089819293,-115.0206510002853264,36.207504000089812735,-115.01853100028530719";
        }

        if (baseUid.ToString() == "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff" &&
            topUid.ToString() == "a54e5945-1aaa-4921-9cc1-c9d8c0a343d3")
        {
          //DesignToGround
          return "36.207101000089814136,-115.0196040002853266,36.20749200008981461,-115.01839400028532623";
        }

        if (baseUid.ToString() == "F07ED071-F8A1-42C3-804A-1BDE7A78BE5B" &&
            topUid.ToString() == "A40814AA-9CDB-4981-9A21-96EA30FFECDD")
        {
          //GroundToGround
          return "36.206612363660681808,-115.02356429250137637,36.206627682610680097,-115.02355673174442074";
        }

        if (baseUid.ToString() == "9c27697f-ea6d-478a-a168-ed20d6cd9a22" &&
            topUid.ToString() == "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff")
        {
          //GroundToDesign
          return "36.207101000089814136,-115.0196040002853266,36.20749200008981461,-115.01839400028532623";
        }
        return "36.205460072631815649,-115.0262815573833564,36.210204042126029833,-115.01769848853631117";
      }

      return string.Empty;
    }

    [Route("api/v2/raptor/designboundarypoints")]
    [HttpGet]
    public async Task<PointsListResult> GetMockDesignBoundaryPoints(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid designUid)
    {
      Console.WriteLine($"GetMockDesignBoundaryPoints: {Request.QueryString}");

      if (projectUid.ToString() == "ff91dd40-1569-4765-a2bc-014321f76ace")
      {
        //design ids: 3
        if (designUid.ToString() == "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff")
        {
          return new PointsListResult
          {
            PointsList = new List<List<WGSPoint>>
            {
              new List<WGSPoint>
              {
                WGSPoint.CreatePoint(0.631932549093478, -2.00748906773731),
                WGSPoint.CreatePoint(0.631940158729017, -2.00748906773731),
                WGSPoint.CreatePoint(0.631940158729017, -2.00745206675717),
                WGSPoint.CreatePoint(0.631938717124322, -2.00745206675717),
                WGSPoint.CreatePoint(0.631932549093478, -2.00748906773731)
              }
            }
          };
        }       
      }

      return new PointsListResult();

    }

    [Route("api/v2/raptor/filterpoints")]
    [HttpGet]
    public async Task<PointsListResult> GetMockFilterPoints(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid filterUid)
    {
      //Not used at present
      Console.WriteLine($"GetMockFilterPoints: {Request.QueryString}");
      return new PointsListResult { PointsList = null };
    }

    [Route("api/v2/raptor/filterpointslist")]
    [HttpGet]
    public async Task<PointsListResult> GetMockFilterPointsList(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? filterUid,
      [FromQuery] Guid? baseUid,
      [FromQuery] Guid? topUid,
      [FromQuery] FilterBoundaryType boundaryType)
    {
      Console.WriteLine($"GetMockFilterPointsList: {Request.QueryString}");

      if (projectUid.ToString() == "ff91dd40-1569-4765-a2bc-014321f76ace")
      {
        if (baseUid.ToString() == "f07ed071-f8a1-42c3-804a-1bde7a78be5b" && topUid.ToString() == "a40814aa-9cdb-4981-9a21-96ea30ffecdd")//GroundToGround 
        {
          //SummaryVolumesBaseFilter and SummaryVolumesTopFilter
          return new PointsListResult
          {
            PointsList = new List<List<WGSPoint>>
            {
              new List<WGSPoint>
              {
                WGSPoint.CreatePoint(0.63192486410214576686, -2.0075397823134681907),
                WGSPoint.CreatePoint(0.63192486410214576686, -2.0075399142735710356),
                WGSPoint.CreatePoint(0.63192459673603029735, -2.0075399142735710356),
                WGSPoint.CreatePoint(0.63192459673603029735, -2.0075397823134681907)
              }
            }
          };
        }

        if (baseUid.ToString() == "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff" && topUid.ToString() == "a54e5945-1aaa-4921-9cc1-c9d8c0a343d3")//DesignToGround 
        {
          //SummaryVolumesFilterToday
          return new PointsListResult
          {
            PointsList = new List<List<WGSPoint>>
            {
              new List<WGSPoint>
              {
                WGSPoint.CreatePoint(0.631933421757, -2.00745117663),
                WGSPoint.CreatePoint(0.631937191668, -2.007449675651),
                WGSPoint.CreatePoint(0.631939949288, -2.007470794135),
                WGSPoint.CreatePoint(0.631933125051, -2.00746859502)
              }
            }
          };
        }
        if (baseUid.ToString() == "9c27697f-ea6d-478a-a168-ed20d6cd9a22" && topUid.ToString() == "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff")//GroundToDesign 
        {
          //SummaryVolumesFilterProjectExtentsEarliest
          return new PointsListResult
          {
            PointsList = new List<List<WGSPoint>>
            {
              new List<WGSPoint>
              {
                WGSPoint.CreatePoint(0.631933421757, -2.00745117663),
                WGSPoint.CreatePoint(0.631937191668, -2.007449675651),
                WGSPoint.CreatePoint(0.631939949288, -2.007470794135),
                WGSPoint.CreatePoint(0.631933125051, -2.00746859502)
              }
            }
          };
        }
        if (filterUid.ToString() == "2811c7c3-d270-4d63-97e2-fc3340bf6c6b")//'Large Sites Road' alignment
        {
          return new PointsListResult
          {
            PointsList = new List<List<WGSPoint>>
            {
              new List<WGSPoint>
              {
                /*
                WGSPoint.CreatePoint(0.631933977148023, -2.00748895357148),
                WGSPoint.CreatePoint(0.631933977148023, -2.00745211260671),
                WGSPoint.CreatePoint(0.631938717124322, -2.00748895357148),
                WGSPoint.CreatePoint(0.631938717124322, -2.00745211260671)
                */
                WGSPoint.CreatePoint(0.631933977148023, -2.00748895357148),
                WGSPoint.CreatePoint(0.631938717124322, -2.00745211260671)
              }
            }
          };
        }
      }

      return new PointsListResult();
    }

    [Route("api/v2/raptor/alignmentpoints")]
    [HttpGet]
    public async Task<AlignmentPointsResult> GetMockAlignmentPoints(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid alignmentUid)
    {
      //Not used at present
      Console.WriteLine($"GetMockAlignmentPoints: {Request.QueryString}");
      return new AlignmentPointsResult{AlignmentPoints = null};
    }

    [Route("api/v2/raptor/alignmentpointslist")]
    [HttpGet]
    public async Task<PointsListResult> GetMockAlignmentPointsList(
      [FromQuery] Guid projectUid)
    {
      Console.WriteLine($"GetMockAlignmentPointsList: {Request.QueryString}");

      if (projectUid.ToString() == "ff91dd40-1569-4765-a2bc-014321f76ace")
      {
        //Alignment ids: 112, 113, 114
        return new PointsListResult
        {
          PointsList = new List<List<WGSPoint>>
          {
            new List<WGSPoint>
            {
              WGSPoint.CreatePoint(0.631933977148023, -2.00748895357148),
              WGSPoint.CreatePoint(0.631938717124322, -2.00745211260671)
            }
          }
        };
      }

      return new PointsListResult();
    }
  }
}
