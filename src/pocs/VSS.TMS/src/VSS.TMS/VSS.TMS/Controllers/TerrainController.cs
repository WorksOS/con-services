using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VSS.TMS.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
 // [EnableCors("AllowAllOrigins")]
  public class TerrainController : ControllerBase
  {
    // GET: api/Terrain
    [HttpGet]
    public string Get()
    {
      return "You have reached the terrain controller";
    }

    // GET: api/Terrain/5
    [HttpGet("{id}", Name = "Get")]
    public string Get(int id)
    {
      return string.Format("Hello number {0}",id);
    }




    /// <summary>
    /// Get tile from tileset with specified coordinates 
    /// URL format according to TMS 1.0.0 specs, like http://localhost/MapTileService/tms/1.0.0/world/3/4/5.png
    /// </summary>
    /// <param name="tilesetName">Tileset name</param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z">Zoom level</param>
    /// <param name="formatExtension"></param>
    /// <returns></returns>
    /// 
    [HttpGet("1.0.0/{tilesetName}/{z}/{x}/{y}.{formatExtension}")]
    public async Task<IActionResult> GetImageTile(string tilesetName, int x, int y, int z, string formatExtension)
    {
      if (string.IsNullOrEmpty(tilesetName))
      {
        return BadRequest();
      }

      //      return Ok("It works"); 


      if (Startup.TileSources.ContainsKey(tilesetName))
      {
        var tileSource = Startup.TileSources[tilesetName];
        var data = await tileSource.GetImageTileAsync(x, y, z);
        if (data != null)
        {
          return File(data, tileSource.ContentType);
        }
        else
        {
          return NotFound();
        }
      }
      else
      {
        return NotFound($"Specified tileset '{tilesetName}' not exists on server");
      }


    }


    /// <summary>
    /// Return Height Map Tile
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="formatExtension"></param>
    /// <returns></returns>
    [HttpGet("1.0.1/{z}/{x}/{y}.{formatExtension}")]
    public async Task<IActionResult> GetTile(int x, int y, int z, string formatExtension)
    {

      var tileSource = Startup.TileSources["terrainqm"];
      var data = await tileSource.GetTerrainTileAsync(x, y, z);
     // var data = await tileSource.GetTerrainTileAsync(x, y, z);
      if (data != null)
      {
        HttpContext.Response.Headers.Add("Content-Encoding", "gzip"); // already compressed on disk
  //       HttpContext.Response.Headers.Add("Access-Control-Allow-Origin","*"); // already compressed on disk
        return File(data, tileSource.ContentType);
      }
      else
      {
        return NotFound();
      }
    }

    /// <summary>
    /// Return Quantized Mesh Tile
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="formatExtension"></param>
    /// <returns></returns>
    [HttpGet("1.0.0/{z}/{x}/{y}.{formatExtension}")]
    public async Task<IActionResult> GetQMTile(int x, int y, int z, string formatExtension)
    {

      var tileSource = Startup.TileSources["terrainqm"];
      var data = await tileSource.GetTerrainQMTileAsync(x, y, z, @"C:\map\data\mydata\tiles",0);
      if (data != null)
      {
      //  HttpContext.Response.Headers.Add("Access-Control-Allow-Origin","*"); // already compressed on disk
        HttpContext.Response.Headers.Add("Content-Length", data.Length.ToString());
        HttpContext.Response.Headers.Add("Content-Type", "application/octet-stream");
        HttpContext.Response.Headers.Add("Content-Encoding", "gzip"); // already compressed on disk
        HttpContext.Response.Headers.Add("Content-Disposition",$"attachment;filename={y}.terrain");
        var da = File(data, tileSource.ContentType);
        return da;

      }
      else
      {
        return NotFound();
      }
    }



    /// <summary>
    /// Return Quantized Mesh Tile
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="formatExtension"></param>
    /// <returns></returns>
    [HttpGet("2.0.0/{z}/{x}/{y}.{formatExtension}")]
    public async Task<IActionResult> GetQMTile2(int x, int y, int z, string formatExtension)
    {

      var tileSource = Startup.TileSources["terrainqm"];

      var data = await tileSource.GetTerrainQMTileAsync(x, y, z, @"C:\map\data\TestData",1);
     // var data = await tileSource.GetTerrainQMTileAsync(x, y, z, @"C:\map\data\Gtopo30QuantizedMesh",1);
      
      
      if (data != null)
      {
        HttpContext.Response.Headers.Add("Content-Encoding", "gzip"); // already compressed on disk
        //   HttpContext.Response.Headers.Add("Access-Control-Allow-Origin","*"); // already compressed on disk
        HttpContext.Response.Headers.Add("Content-Length", data.Length.ToString());
        HttpContext.Response.Headers.Add("Content-Type", "application/octet-stream");
        HttpContext.Response.Headers.Add("Content-Disposition", $"attachment;filename={y}.terrain");
        return File(data, tileSource.ContentType);
      }
      else
      {
        return NotFound();
      }
    }

    // Cheap way to return json
    [HttpGet("1.0.1/layer.json")]
    public async Task<string> GetLayerJSon()
    {
      string layer = "{\"tilejson\": \"2.1.0\",\"format\": \"heightmap-1.0\",\"version\": \"1.0.0\",\"scheme\": \"tms\",\"tiles\": [\"{z}/{x}/{y}.terrain\"]}";
      return layer;
    }

    // Cheap way to return json
    [HttpGet("1.0.0/layer.json")]
    public async Task<string> GetLayerJSonQM()
    {
     // string fileLocation = @"C:\map\data\Gtopo30QuantizedMesh\layer.json";
      string fileLocation = @"C:\map\data\mydata\tiles\layer.json";

      var fileInfo = new FileInfo(fileLocation);
      if (!fileInfo.Exists)
        throw new FileNotFoundException("[layer.json not in folder}]");

      try
      {
        return System.IO.File.ReadAllText(fileLocation);
      }
      catch (Exception e)
      {
        Console.WriteLine("[Data File Missing] {0}", e);
        throw new FileNotFoundException($"[layer.json not in {fileLocation}] ", e);
      }
    }

    // Cheap way to return json
    [HttpGet("2.0.0/layer.json")]
    public async Task<string> GetLayerJSonQM2()
    {
      string fileLocation = @"C:\map\data\TestData\layer.json";
     // string fileLocation = @"C:\map\data\Gtopo30QuantizedMesh\layer.json";

      var fileInfo = new FileInfo(fileLocation);
      if (!fileInfo.Exists)
        throw new FileNotFoundException("[layer.json not in folder}]");

      try
      {
        return System.IO.File.ReadAllText(fileLocation);
      }
      catch (Exception e)
      {
        Console.WriteLine("[Data File Missing] {0}", e);
        throw new FileNotFoundException($"[layer.json not in {fileLocation}] ", e);
      }
    }

  }
}

