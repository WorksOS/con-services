using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic.CompilerServices;
using VSS.Map3D.Common;
using VSS.Map3D.DEM;
using VSS.Map3D.Mesh;
using VSS.Map3D.Models;
using VSS.Map3D.Models.QMTile;
using VSS.Map3D.Tiler;

namespace VSS.Map3D.TMS.Controllers
{
  [Route("api/[controller]")]
  [EnableCors("AllowAllOrigins")]
  [ApiController]
  public class TerrainController : ControllerBase
  {
    private IConfiguration _configuration;
    private IDEMSource _demSource;
    private readonly string _TerrainDataQM = "application/octet-stream";
    private int _GridSize = 10;

    public TerrainController(IConfiguration Configuration, IDEMSource demSource)
    {
      _configuration = Configuration;
      _demSource = demSource;

      if (int.TryParse(_configuration["DefaultGridSize"], out int gridSize))
        _GridSize = gridSize;
      else
        _GridSize = 5;

      // todo set defaults
      _demSource.Initalize(new MapHeader() {GridSize = _GridSize, MinElevation = 0, MaxElevation = 8000});

      System.Diagnostics.Debug.WriteLine($"*** Terrain Controller *** GridSize:{_GridSize}");

    }

    [HttpGet]
    public string Get()
    {
      return "You have reached the terrain controller";
    }


    /// <summary>
    /// Return Prerendered Quantized Mesh Tile
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="formatExtension"></param>
    /// <returns></returns>
    [HttpGet("trex/1.0/{z}/{x}/{y}.{formatExtension}")]
    public async Task<IActionResult> GetStaticTile(int x, int y, int z, string formatExtension, [FromQuery] Guid projectUid, [FromQuery] Guid filterUid)
    {
      // todo for now default values. these will be passed in eventually
      var testDataPath = _configuration["TestDataPath"];

      Guid projectId = projectUid;
      Guid filterid  = filterUid;
      bool isValid = Guid.TryParse(_configuration["DefaultProjectUid"], out projectId);
      if (!isValid)
        return NotFound(); // todo


      if (z < 1) // root level
      {
        // these two tiles are standard. Use tiler to fetch the correct tile
        var basicTile = await new Tiler.Tiler().FetchTile(testDataPath, x, y, z);
        if (basicTile != null)
        {
          HttpContext.Response.Headers.Add("Content-Encoding", "gzip"); // already compressed on disk
          HttpContext.Response.Headers.Add("Content-Length", basicTile.Length.ToString());
          HttpContext.Response.Headers.Add("Content-Type", "application/octet-stream");
          HttpContext.Response.Headers.Add("Content-Disposition", $"attachment;filename={y}.terrain");
          return File(basicTile, _TerrainDataQM);
        }

        Console.WriteLine($"*** Tile x:{x},y:{y},z:{z} was found ***");
        return NotFound();
      }

    
      var elevData = await _demSource.GetDemXYZ(x, y, z); // DI injected for single instance

      // This class constructs a cesium quantized mesh from the dem
      var vertices = new Mesh.Mesh().MakeFakeMesh(ref elevData);

      var tempHeadr = new TerrainTileHeader()
      {
        MaximumHeight = elevData.MaximumHeight,
        MinimumHeight = elevData.MinimumHeight,
        CenterX = elevData.CenterX,
        CenterY = elevData.CenterY,
        CenterZ = elevData.CenterZ,
        BoundingSphereCenterX = elevData.BoundingSphereCenterX,
        BoundingSphereCenterY = elevData.BoundingSphereCenterY,
        BoundingSphereCenterZ = elevData.BoundingSphereCenterZ,
        BoundingSphereRadius = elevData.BoundingSphereRadius,
        HorizonOcclusionPointX = elevData.HorizonOcclusionPointX,
        HorizonOcclusionPointY = elevData.HorizonOcclusionPointY,
        HorizonOcclusionPointZ = elevData.HorizonOcclusionPointZ
      };

      // This class constructs a quantized mesh tile mesh from the original mesh
      var tile = new Tiler.Tiler().MakeTile(vertices, tempHeadr, MapUtil.GridSizeToTriangleCount(elevData.GridSize),
        elevData.GridSize);
      if (tile != null)
      {
        var compressed = MapUtil.Compress(tile);
        HttpContext.Response.Headers.Add("Content-Encoding", "gzip"); // already compressed on disk
        HttpContext.Response.Headers.Add("Content-Length", compressed.Length.ToString());
        HttpContext.Response.Headers.Add("Content-Type", "application/octet-stream");
        HttpContext.Response.Headers.Add("Content-Disposition", $"attachment;filename={y}.terrain");
        return File(compressed, _TerrainDataQM);
      }

      return NotFound();
    }


    /**********************************************************************
     * Dynamic tile generation tests
     ************************************************************************/


    /*

  /// <summary>
  /// Return Fake Quantized Mesh Tile
  /// </summary>
  /// <param name="x"></param>
  /// <param name="y"></param>
  /// <param name="z"></param>
  /// <param name="formatExtension"></param>
  /// <returns></returns>
  [HttpGet("debug/fake/{z}/{x}/{y}.{formatExtension}")]
  public async Task<IActionResult> GetFakeTile(int x, int y, int z, string formatExtension)
  {
    // todo for now default values. these will be passed in eventually
    var testDataPath = _configuration["TestDataPath"];

    Guid projectId;
    bool isValid = Guid.TryParse(_configuration["DefaultProjectUid"], out projectId);
    if (!isValid)
      return NotFound(); // todo


    if (z < 1)
    { // these two tiles are standard
      // Use tiler to fetch the correct tile
      var basicTile = await new Tiler.Tiler().FetchTile(testDataPath, x, y, z);
      if (basicTile != null)
      {
        HttpContext.Response.Headers.Add("Content-Encoding", "gzip"); // already compressed on disk
        //   HttpContext.Response.Headers.Add("Access-Control-Allow-Origin","*"); // already compressed on disk

        HttpContext.Response.Headers.Add("Content-Length", basicTile.Length.ToString());
        HttpContext.Response.Headers.Add("Content-Type", "application/octet-stream");
        HttpContext.Response.Headers.Add("Content-Disposition", $"attachment;filename={y}.terrain");
        return File(basicTile, _TerrainDataQM);
      }
      Console.WriteLine($"*** Tile x:{x},y:{y},z:{z} was found ***");
    }


    // class to get digital elevation models(DEM)

    FakeDEMSource demSrc = new FakeDEMSource();

    demSrc.Initalize(new MapHeader(){GridSize = _GridSize});

    // todo x,y,z to lat lon
    var elevData = await demSrc.GetDemLL(1, 1, 1, 1); // just the same tile always for now


    // This class constructs a cesium quantized mesh from the dem
    var vertices = new Mesh.Mesh().MakeFakeMesh(ref elevData);

    // todo fill in header details
    var tempHeadr = new TerrainTileHeader()
    {
      MaximumHeight = elevData.MaxElevation,
      MinimumHeight = elevData.MinElevation
    };

    // This class constructs a quantized mesh tile mesh from the original mesh
    var tile = new Tiler.Tiler().MakeTile(vertices, tempHeadr, MapUtil.GridSizeToTriangleCount(elevData.GridSize), elevData.GridSize);

    if (tile != null)
    {
      HttpContext.Response.Headers.Add("Content-Encoding", "gzip"); // already compressed on disk
      //   HttpContext.Response.Headers.Add("Access-Control-Allow-Origin","*"); // already compressed on disk

      HttpContext.Response.Headers.Add("Content-Length", tile.Length.ToString());
      HttpContext.Response.Headers.Add("Content-Type", "application/octet-stream");
      HttpContext.Response.Headers.Add("Content-Disposition", $"attachment;filename={y}.terrain");
      return File(tile, _TerrainDataQM);
    }
    else
    {
      return NotFound();
    }

  }

*/



    /// <summary>
    /// Return Fake Quantized Mesh Tile. Testing Endpoint
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="formatExtension"></param>
    /// <returns></returns>
    [HttpGet("debug/fake/{z}/{x}/{y}.{formatExtension}")]
    public async Task<IActionResult> GetFakeTile(int x, int y, int z, string formatExtension)
    {
      // todo for now default values. these will be passed in eventually
      var testDataPath = _configuration["TestDataPath"];

      Guid projectId;
      bool isValid = Guid.TryParse(_configuration["DefaultProjectUid"], out projectId);
      if (!isValid)
        return NotFound(); // todo


      if (z < 1)
      {
        // these two tiles are standard
        // Use tiler to fetch the correct tile
        var basicTile = await new Tiler.Tiler().FetchTile(testDataPath, x, y, z);
        if (basicTile != null)
        {
          HttpContext.Response.Headers.Add("Content-Encoding", "gzip"); // already compressed on disk
          //   HttpContext.Response.Headers.Add("Access-Control-Allow-Origin","*"); // already compressed on disk

          HttpContext.Response.Headers.Add("Content-Length", basicTile.Length.ToString());
          HttpContext.Response.Headers.Add("Content-Type", "application/octet-stream");
          HttpContext.Response.Headers.Add("Content-Disposition", $"attachment;filename={y}.terrain");
          Console.WriteLine($"*** Tile x:{x},y:{y},z:{z} was found ***");
          return File(basicTile, _TerrainDataQM);
        }

        return NotFound();
      }

      FakeDEMSource demSrc = new FakeDEMSource();

      demSrc.Initalize(new MapHeader() {GridSize = _GridSize});

      // todo x,y,z to lat lon
      var elevData = await demSrc.GetDemLL(1, 1, 1, 1); // just the same tile always for now

      // This class constructs a cesium quantized mesh from the dem
      var vertices = new Mesh.Mesh().MakeFakeMesh(ref elevData);

      // todo fill in header details
      var tempHeadr = new TerrainTileHeader()
      {
        MaximumHeight = elevData.MaximumHeight,
        MinimumHeight = elevData.MinimumHeight
      };

      // This class constructs a quantized mesh tile mesh from the original mesh
      var tile = new Tiler.Tiler().MakeTile(vertices, tempHeadr, MapUtil.GridSizeToTriangleCount(elevData.GridSize),
        elevData.GridSize);

      if (tile != null)
      {
        var compressed = MapUtil.Compress(tile);
        HttpContext.Response.Headers.Add("Content-Encoding", "gzip"); // already compressed on disk
        //   HttpContext.Response.Headers.Add("Access-Control-Allow-Origin","*"); // already compressed on disk

        HttpContext.Response.Headers.Add("Content-Length", compressed.Length.ToString());
        HttpContext.Response.Headers.Add("Content-Type", "application/octet-stream");
        HttpContext.Response.Headers.Add("Content-Disposition", $"attachment;filename={y}.terrain");
        return File(compressed, _TerrainDataQM);
      }
      else
      {
        return NotFound();
      }
    }


    /// <summary>
    /// Return Bitmap Quantized Mesh Tile. Testing Endpoint
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="formatExtension"></param>
    /// <returns></returns>
    [HttpGet("debug/bitmap/{z}/{x}/{y}.{formatExtension}")]
    public async Task<IActionResult> GetBitmapTile(int x, int y, int z, string formatExtension)
    {
      // todo for now default values. these will be passed in eventually
      var testDataPath = _configuration["TestDataPath"];

      Guid projectId;
      bool isValid = Guid.TryParse(_configuration["DefaultProjectUid"], out projectId);
      if (!isValid)
        return NotFound(); // todo

      if (z < 1)
      {
        // these two tiles are standard
        // Use tiler to fetch the correct tile
        var basicTile = await new Tiler.Tiler().FetchTile(testDataPath, x, y, z);
        if (basicTile != null)
        {
          HttpContext.Response.Headers.Add("Content-Encoding", "gzip"); // already compressed on disk
          //   HttpContext.Response.Headers.Add("Access-Control-Allow-Origin","*"); // already compressed on disk

          HttpContext.Response.Headers.Add("Content-Length", basicTile.Length.ToString());
          HttpContext.Response.Headers.Add("Content-Type", "application/octet-stream");
          HttpContext.Response.Headers.Add("Content-Disposition", $"attachment;filename={y}.terrain");
          return File(basicTile, _TerrainDataQM);
        }

        Console.WriteLine($"*** Tile x:{x},y:{y},z:{z} was found ***");
        return NotFound(); // todo
      }

      // Bitmap tiler
      BitmapDEMSource demSrc = new BitmapDEMSource();
      var elevData = await demSrc.GetDemXYZ(x, y, z); // DI injected for single instance
      // This class constructs a cesium quantized mesh from the dem
      var vertices = new Mesh.Mesh().MakeFakeMesh(ref elevData);

      // todo fill in header details
      var tempHeadr = new TerrainTileHeader()
      {
        MaximumHeight = elevData.MaximumHeight,
        MinimumHeight = elevData.MinimumHeight,
        CenterX = elevData.CenterX,
        CenterY = elevData.CenterY,
        CenterZ = elevData.CenterZ,
        BoundingSphereCenterX = elevData.BoundingSphereCenterX,
        BoundingSphereCenterY = elevData.BoundingSphereCenterY,
        BoundingSphereCenterZ = elevData.BoundingSphereCenterZ,
        BoundingSphereRadius = elevData.BoundingSphereRadius,
        HorizonOcclusionPointX = elevData.HorizonOcclusionPointX,
        HorizonOcclusionPointY = elevData.HorizonOcclusionPointY,
        HorizonOcclusionPointZ = elevData.HorizonOcclusionPointZ

      };

      // This class constructs a quantized mesh tile mesh from the original mesh
      var tile = new Tiler.Tiler().MakeTile(vertices, tempHeadr, MapUtil.GridSizeToTriangleCount(elevData.GridSize),
        elevData.GridSize);

      // Debugging code
      string file = $"c:\\temp\\Test-{x}-{y}-{z}.terrain";
      FileInfo fi = new FileInfo(file);
      if (!fi.Exists && z == 8 && x == 501)
      {
        var ms = new MemoryStream(tile);
        using (FileStream fs = new FileStream(file, FileMode.Create))
        using (GZipStream zipStream = new GZipStream(fs, CompressionMode.Compress, false))
        {
          zipStream.Write(ms.ToArray(), 0, ms.ToArray().Length); // .Write(bytes, 0, bytes.Length);
        }
      }

      if (tile != null)
      {
        var compressed = MapUtil.Compress(tile);
        HttpContext.Response.Headers.Add("Content-Encoding", "gzip"); // already compressed on disk
        HttpContext.Response.Headers.Add("Content-Length", compressed.Length.ToString());
        HttpContext.Response.Headers.Add("Content-Type", "application/octet-stream");
        HttpContext.Response.Headers.Add("Content-Disposition", $"attachment;filename={y}.terrain");
        return File(compressed, _TerrainDataQM);
        // return Ok(MapUtil.Compress(tile));
      }
      else
      {
        return NotFound();
      }

    }




    /*************************************
     ********** LAYER FILES **************
     * These files act like headers to letr Cesium know what tiles it can call and how
     * Path matches the same endpoint as above
     ************************************/

    public string GetGenericLayerFile()
    {
      //      var fileLocation = Path.Combine(_configuration["TestDataPath"], "layerAllTiles.json");
      var fileLocation = Path.Combine(_configuration["TestDataPath"], "TRexlayer.json");
//      var fileLocation = Path.Combine(_configuration["TestDataPath"], "CesiumlayerAllTiles.json");

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

    [HttpGet("debug/fake/layer.json")]
    public string GetFakeLayerFile1()
    {
      return GetGenericLayerFile();
    }

    [HttpGet("debug/bitmap/layer.json")]
    public string GetBitmapLayerFile()
    {
      return GetGenericLayerFile();
    }

    [HttpGet("trex/1.0/layer.json")]
    public string GetTRexLayerFile()
    {
      return GetGenericLayerFile();
    }

    [HttpGet("1.0.0/layer.json")]
    public string GetLayerFile0()
    {
      // todo return constructed layer file controlling levels and real info
      var fileLocation = Path.Combine(_configuration["TestDataPath"], "layer.json");
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
