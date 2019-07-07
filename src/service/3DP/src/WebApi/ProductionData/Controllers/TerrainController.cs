using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Executors;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Contracts;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
    [Route("api/[controller]")]
  //  [ApiController]
  //  [ProjectVerifier]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class TerrainController : ControllerBase, ITerrainContract
    {
    private readonly string _TerrainDataQM = "application/octet-stream";

    /// <summary>
    /// LoggerFactory for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// LoggerFactory factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    private readonly IConfigurationStore ConfigStore;

    /// <summary>
    /// Gets the custom headers for the request.
    /// </summary>
    protected IDictionary<string, string> CustomHeaders => Request.Headers.GetCustomHeaders();

    /// <summary>
    /// The TRex Gateway proxy for use by executor.
    /// </summary>
    private readonly ITRexCompactionDataProxy TRexCompactionDataProxy;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="logger">LoggerFactory</param>
    /// <param name="configStore">Configuration Store</param>
    /// <param name="trexCompactionDataProxy">Trex Gateway production data proxy</param>
    public TerrainController(ILoggerFactory logger, IConfigurationStore configStore, ITRexCompactionDataProxy trexCompactionDataProxy)
    {
      this.logger = logger;
      log = this.logger.CreateLogger<TerrainController>();
      ConfigStore = configStore;
      TRexCompactionDataProxy = trexCompactionDataProxy;
    }

    /*
    public async Task<byte[]> FetchTile(string tileDir, int x, int y, int z)
    {
      // todo go get precompiled tile from disk or cache

      var fileInfo = new FileInfo(Path.Combine(tileDir, string.Format(@"{0}\{1}\{2}.terrain", z, x, y)));
      if (fileInfo.Exists)
      {
        var buffer = new byte[fileInfo.Length];
        using (var fileStream = fileInfo.OpenRead())
        {
          await fileStream.ReadAsync(buffer, 0, buffer.Length);
          Console.WriteLine("Tile {0} sent", fileInfo);
          return buffer.ToArray();
        }
      }
      Console.WriteLine("*** Tile {0} was NOT sent ***", fileInfo);
      return null;
    }
    */

    public async Task<byte[]> FetchTile(Guid projectId, long filterId, int x, int y, int z)
    {
      var request = new QMTileRequest();

      request.BoundBoxGrid = null;
      request.BoundBoxLatLon = new BoundingBox2DLatLon(0.0, 0.0, 0.0, 0.0);
      request.CallId = new Guid();
      request.Filter1 = new FilterResult();
      request.FilterId1 = filterId;
      request.ProjectUid = projectId;



      request.Validate();
      var qmTileResult = RequestExecutorContainerFactory.Build<QMTilesExecutor>(logger,
        configStore: ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders).Process(request) as QMTileResult;
      return qmTileResult.TileData;
    }

    

    /// <summary>
    /// Request for a quantized mesh tile
    /// </summary>
    /// <param name="x">x tile coordinate</param>
    /// <param name="y">y tile coordinate</param>
    /// <param name="z">z tile coordinate</param>
    /// <param name="formatExtension">terrain ext</param>
    /// <param name="projectUid">Project UId</param>
    /// <param name="filterId">Filter Id</param>
    /// <returns></returns>
    [HttpGet("v1/qmesh/{z}/{x}/{y}.{formatExtension}")]
    public async Task<IActionResult> Get(int x, int y, int z, string formatExtension, [FromQuery] Guid projectUid, [FromQuery] long filterId)
    {

      log.LogInformation("Get: " + Request.QueryString);
      log.LogDebug($"QMesh tile request params. x:{x},y:{y},z:{z}, ProjectId:{projectUid}, FilterId:{filterId} *");
      //      Console.WriteLine($"* Params x:{x},y:{y},z:{z}, prjid:{projectUid}, filterid:{filterId} *");

      var testDataPath = ConfigStore.GetValueString("TestDataPath");
      if (testDataPath == null)
        testDataPath = @"c:\map\data\TestData\";

      // var basicTile = await FetchTile(testDataPath, x, y, z);
      var basicTile = await FetchTile(projectUid, filterId, x, y, z);
      if (basicTile != null)
      {
    //    HttpContext.Response.Headers.Add("Content-Encoding", "gzip"); // already compressed on disk
        HttpContext.Response.Headers.Add("Content-Length", basicTile.Length.ToString());
        HttpContext.Response.Headers.Add("Content-Type", "application/octet-stream");
        HttpContext.Response.Headers.Add("Content-Disposition", $"attachment;filename={y}.terrain");
        return File(basicTile, _TerrainDataQM);
      }

      Console.WriteLine($"Tile x:{x},y:{y},z:{z} was not found");
      return NotFound();

    }
    



      /* test 2

    [HttpGet("v1/qmesh/{z}/{x}/{y}.{formatExtension}")]
    public async Task<IActionResult> Get(int x, int y, int z, string formatExtension, [FromQuery] Guid projectUid, [FromQuery] long filterId)
    {

      log.LogInformation("Get: " + Request.QueryString);

      var basicTile = new byte[] { 0x41, 0x41, 0x41, 0x41, 0x41, 0x41, 0x41 };
      if (basicTile != null)
      {
      //  HttpContext.Response.Headers.Add("Content-Encoding", "gzip"); // already compressed on disk
        HttpContext.Response.Headers.Add("Content-Length", basicTile.Length.ToString());
        HttpContext.Response.Headers.Add("Content-Type", "application/octet-stream");
        HttpContext.Response.Headers.Add("Content-Disposition", $"attachment;filename={y}.terrain");
        return File(basicTile, _TerrainDataQM);
      }

      Console.WriteLine($"Tile x:{x},y:{y},z:{z} was not found");
      return NotFound();

    }

  */





    public string GetGenericLayerFile()
    {
      var fileLocation = @"C:\map\data\TestData\TRexlayer.json";

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

    /// <summary>
    /// Returns layer.json that controls the layout of all future tile requets
    /// </summary>
    /// <returns></returns>
    [HttpGet("v1/qmesh/layer.json")]
    public string GetTRexLayerFile()
    {
    
      // Its possible this layer file could be custom made to control tile requests to a certain area around 
      // a projects boundary only. Hence reducing overall tile requets
      return GetGenericLayerFile();
    }


  }
}
