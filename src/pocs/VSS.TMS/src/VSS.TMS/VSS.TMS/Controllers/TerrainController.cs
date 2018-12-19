using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VSS.TMS.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
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
      return "value";
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
    // [EnableCors("AllowSpecificOrigin")]
    public async Task<IActionResult> GetImageTile(string tilesetName, int x, int y, int z, string formatExtension)
    {
      if (String.IsNullOrEmpty(tilesetName))
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


    [HttpGet("1.0.0/{z}/{x}/{y}.{formatExtension}")]
    public async Task<IActionResult> GetTile(int x, int y, int z, string formatExtension)
    {

      var tileSource = Startup.TileSources["terrain"];
      var data = await tileSource.GetTerrainTileAsync(x, y, z);
      if (data != null)
      {
        HttpContext.Response.Headers.Add("Content-Encoding", "gzip"); // already compressed on disk
        return File(data, tileSource.ContentType);
      }
      else
      {
        return NotFound();
      }
    }


    // Cheap way to return json
    [HttpGet("1.0.0/layer.json")]
    public async Task<String> GetLayerJSon()
    {
      string layer = "{\"tilejson\": \"2.1.0\",\"format\": \"heightmap-1.0\",\"version\": \"1.0.0\",\"scheme\": \"tms\",\"tiles\": [\"{z}/{x}/{y}.terrain\"]}";
      return layer;
    }



  }
}

